using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.LiveCapture.VirtualCamera.Raycasting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A class allowing to track the intersection of a ray and a mesh.
    /// Meant to be used in conjunction with <see cref="GraphicsRaycaster"/>, to provide a precise spatial-focus feature.
    /// </summary>
    class MeshIntersectionTracker : IDisposable
    {
        /// <summary>
        /// Mode of the tracker.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// No tracking.
            /// </summary>
            None,

            /// <summary>
            /// Tracks an intersection point with a static mesh. The local intersection point is not expected to change and can be cached.
            /// </summary>
            MeshStatic,

            /// <summary>
            /// Tracks an intersection point with a dynamic mesh.
            /// Mesh vertices may be procedurally modified and the local intersection point is recomputed on update.
            /// </summary>
            /// <remarks>
            /// Requires a readback of the mesh vertices which has an impact on performance.
            /// </remarks>
            MeshDynamic,

            /// <summary>
            /// Tracks an intersection point with a skinned mesh.
            /// Mesh vertices are expected to be animated and the local intersection point is recomputed on update.
            /// </summary>
            /// <remarks>
            /// Requires a readback of the mesh vertices which has an impact on performance.
            /// </remarks>
            SkinnedMesh,

            /// <summary>
            /// Tracks an intersection point with a skinned mesh.
            /// </summary>
            /// <remarks>
            /// Vertices of the triangle within which lies the intersection point are skinned CPU side to improve performance.
            /// </remarks>
            SkinnedMeshCpuSkinning
        }

        // No concurrent access expected, used to minimize allocations.
        static readonly List<Vector3> k_TmpVertices = new List<Vector3>();
        static readonly List<int> k_TmpTriangles = new List<int>();

        // A struct holding the information needed to read vertex weight info from a BoneWeight1 buffer.
        struct VertexBoneInfo
        {
            // Number of bones for this vertex.
            public byte Count;

            // Index of the first entry for this vertex in a BoneWeight1 buffer.
            public int Index;
        }

        // Avoid reallocating collections.
        readonly Vector3[] m_TriangleVertices = new Vector3[3];
        readonly Vector3[] m_TriangleSkinnedVertices = new Vector3[3];
        readonly VertexBoneInfo[] m_BonesPerVertex = new VertexBoneInfo[3];
        readonly List<BoneWeight1> m_BoneWeights = new List<BoneWeight1>();
        readonly List<Matrix4x4> m_BindPoses = new List<Matrix4x4>();

        // The renderer rendering the mesh. May be a <see cref="MeshRenderer"/> or a <see cref="SkinnedMeshRenderer"/>.
        Renderer m_Renderer;

        // The mesh whose intersection with is being tracked.
        Mesh m_Mesh;

        // Indices of the vertices of the triangle within which the intersection point lies.
        Vector3Int m_TriangleIndices;

        // Barycentric coordinates of the intersection point within the intersected triangle.
        Vector2 m_BarycentricCoordinates;

        // Position of the intersection point in local space.
        Vector3 m_LocalIntersectionPoint;

        // Index of the frame where a readback last occured, to prevent multiple readbacks during the same frame.
        int m_LastReadbackFrame;

        // Mesh used to store baked skinned meshes.
        Mesh m_BakedSkinnedMesh;

        CustomSampler m_TrackSampler;
        CustomSampler m_UpdateSampler;

        /// <summary>
        /// The current tracking mode.
        /// </summary>
        public Mode CurrentMode { get; private set; } = Mode.None;

        /// <summary>
        /// Initialize resources.
        /// </summary>
        public void Initialize()
        {
            if (m_TrackSampler == null)
            {
                m_TrackSampler = CustomSampler.Create($"{nameof(MeshIntersectionTracker)}.{nameof(TryTrack)}", true);
            }

            if (m_UpdateSampler == null)
            {
                m_UpdateSampler = CustomSampler.Create($"{nameof(MeshIntersectionTracker)}.{nameof(TryUpdate)}", true);
            }
        }

        /// <summary>
        /// Release resources and references to external components.
        /// </summary>
        public void Dispose()
        {
            Reset();
            AdditionalCoreUtils.DestroyIfNeeded(ref m_BakedSkinnedMesh);
        }

        /// <summary>
        /// Reset the tracker. Discards the currently tracked intersection point if any and clears any reference to external components.
        /// </summary>
        public void Reset()
        {
            CurrentMode = Mode.None;
            m_Renderer = null;
            m_Mesh = null;
        }

        /// <summary>
        /// Update the position of the currently tracked intersection point, if any.
        /// </summary>
        /// <param name="worldPosition">The world position of the currently tracked intersection point, if any</param>
        /// <param name="force">If true, the intersection is updated even if it already was during the current frame.</param>
        /// <returns>True if an intersection point is currently being tracked.</returns>
        public bool TryUpdate(out Vector3 worldPosition, bool force = false)
        {
            using (new CustomSamplerScope(m_UpdateSampler))
            {
                if (CurrentMode == Mode.None || !m_Renderer.gameObject.activeInHierarchy)
                {
                    Reset();
                    worldPosition = Vector3.zero;
                    return false;
                }

                if (m_Renderer.isVisible && (m_LastReadbackFrame != Time.frameCount || force))
                {
                    m_LastReadbackFrame = Time.frameCount;

                    switch (CurrentMode)
                    {
                        case Mode.MeshDynamic:
                        {
                            m_LocalIntersectionPoint = ReadbackVerticesAndReturnLocalPosition();
                        }
                        break;
                        case Mode.SkinnedMesh:
                        {
                            (m_Renderer as SkinnedMeshRenderer).BakeMesh(m_Mesh);
                            m_LocalIntersectionPoint = ReadbackVerticesAndReturnLocalPosition();
                        }
                        break;
                        case Mode.SkinnedMeshCpuSkinning:
                        {
                            var bones = (m_Renderer as SkinnedMeshRenderer).bones;

                            // CPU side skinning of the intersected triangle vertices.
                            for (var i = 0; i != 3; ++i)
                            {
                                var vertexBoneInfo = m_BonesPerVertex[i];
                                m_TriangleSkinnedVertices[i] = SkinVertex(
                                    m_TriangleVertices[i], vertexBoneInfo.Index, vertexBoneInfo.Count,
                                    m_BoneWeights, m_BindPoses, bones);
                            }

                            var skinnedIntersectionPoint = BarycentricToCartesian(
                                m_BarycentricCoordinates,
                                m_TriangleSkinnedVertices[0],
                                m_TriangleSkinnedVertices[1],
                                m_TriangleSkinnedVertices[2]);

                            // Inverse transform since we cache the local intersection point in all cases.
                            m_LocalIntersectionPoint = m_Renderer.transform.InverseTransformPoint(skinnedIntersectionPoint);
                        }
                        break;
                    }
                }

                worldPosition = m_Renderer.transform.TransformPoint(m_LocalIntersectionPoint);
                return true;
            }
        }

        /// <summary>
        /// Start tracking the intersection point of a mesh and a ray.
        /// </summary>
        /// <remarks>
        /// The tracking mode is inferred from the set of components held by the <see cref="gameObject"/>.
        /// </remarks>
        /// <param name="gameObject">The gameObject hit by the graphics raycaster, expected to hold a mesh.</param>
        /// <param name="worldRay">The ray used by the graphics raycaster, in world space.</param>
        /// <param name="worldIntersectionPoint">The intersection point evaluated by the graphics raycaster, in world space.</param>
        /// <param name="avoidReadback">If true, mode will be selected in order to avoid vertices readback in <see cref="TryUpdate"/>.</param>
        /// <returns>True if an intersection point was found.</returns>
        public bool TryTrack(GameObject gameObject, Ray worldRay, Vector3 worldIntersectionPoint, bool avoidReadback = true)
        {
            using (new CustomSamplerScope(m_TrackSampler))
            {
                var renderer = gameObject.GetComponent<Renderer>();
                switch (renderer)
                {
                    case MeshRenderer meshRenderer:
                        if (TryTrackMesh(meshRenderer, worldRay, worldIntersectionPoint, avoidReadback))
                        {
                            return true;
                        }

                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        if (TryTrackSkinnedMesh(skinnedMeshRenderer, worldRay, worldIntersectionPoint, avoidReadback))
                        {
                            return true;
                        }

                        break;
                }

                Reset();
                return false;
            }
        }

        bool TryTrackMesh(MeshRenderer meshRenderer, Ray worldRay, Vector3 worldIntersectionPoint, bool avoidReadback)
        {
            var filter = meshRenderer.GetComponent<MeshFilter>();
            Assert.IsNotNull(filter);

            // From the documentation, MeshFilter.mesh: If no mesh is assigned to the mesh filter a new mesh will be created and assigned.
            // However if the mesh was null the graphics raycaster could not have picked it so we do not need to consider this case.
            var mesh = filter.sharedMesh != null ? filter.sharedMesh : filter.mesh;

            if (RayMeshIntersection(mesh, meshRenderer.transform, worldRay, worldIntersectionPoint,
                out var triangle, out var barycentricCoordinates))
            {
                m_Renderer = meshRenderer;
                m_Mesh = mesh;
                m_TriangleIndices = triangle;
                m_BarycentricCoordinates = barycentricCoordinates;

                if (avoidReadback)
                {
                    CurrentMode = Mode.MeshStatic;

                    // In static mode, readback once and cache the local position.
                    m_LocalIntersectionPoint = ReadbackVerticesAndReturnLocalPosition();
                }
                else
                {
                    CurrentMode = Mode.MeshDynamic;
                }

                return true;
            }

            return false;
        }

        bool TryTrackSkinnedMesh(SkinnedMeshRenderer skinnedMeshRenderer, Ray worldRay, Vector3 worldIntersectionPoint, bool avoidReadback)
        {
            if (m_BakedSkinnedMesh == null)
                m_BakedSkinnedMesh = new Mesh();

            skinnedMeshRenderer.BakeMesh(m_BakedSkinnedMesh);

            if (RayMeshIntersection(m_BakedSkinnedMesh, skinnedMeshRenderer.transform, worldRay, worldIntersectionPoint,
                out var triangle, out var barycentricCoordinates))
            {
                m_Renderer = skinnedMeshRenderer;
                m_Mesh = m_BakedSkinnedMesh;
                m_TriangleIndices = triangle;
                m_BarycentricCoordinates = barycentricCoordinates;

                if (avoidReadback)
                {
                    CurrentMode = Mode.SkinnedMeshCpuSkinning;

                    var sharedMesh = skinnedMeshRenderer.sharedMesh;

                    sharedMesh.GetBindposes(m_BindPoses);

                    // Cache triangle vertices and bone weights.
                    sharedMesh.GetVertices(k_TmpVertices);
                    m_TriangleVertices[0] = k_TmpVertices[triangle.x];
                    m_TriangleVertices[1] = k_TmpVertices[triangle.y];
                    m_TriangleVertices[2] = k_TmpVertices[triangle.z];

                    var boneWeights = sharedMesh.GetAllBoneWeights();
                    var bonesPerVertices = sharedMesh.GetBonesPerVertex();

                    // Find start indices corresponding to vertices in the bone weights buffer.
                    var startIndices = new NativeArray<int>(bonesPerVertices.Length, Allocator.Temp);
                    var index = 0;
                    for (var i = 0; i != bonesPerVertices.Length; ++i)
                    {
                        startIndices[i] = index;
                        index += bonesPerVertices[i];
                    }

                    m_BoneWeights.Clear();

                    // Store bone information for intersected triangle vertices.
                    index = 0;
                    for (var i = 0; i != 3; ++i)
                    {
                        var vertexIndex = triangle[i];
                        var numBones = bonesPerVertices[vertexIndex];

                        for (var j = 0; j != numBones; ++j)
                        {
                            m_BoneWeights.Add(boneWeights[startIndices[vertexIndex] + j]);
                        }

                        m_BonesPerVertex[i] = new VertexBoneInfo
                        {
                            Count = numBones,
                            Index = index
                        };

                        index += numBones;
                    }

                    startIndices.Dispose();
                }
                else
                {
                    CurrentMode = Mode.SkinnedMesh;
                }

                return true;
            }

            return false;
        }

        // Readback mesh vertices and evaluate the local intersection point based on barycentric coordinates.
        Vector3 ReadbackVerticesAndReturnLocalPosition()
        {
            m_Mesh.GetVertices(k_TmpVertices);
            return BarycentricToCartesian(
                m_BarycentricCoordinates,
                k_TmpVertices[m_TriangleIndices.x],
                k_TmpVertices[m_TriangleIndices.y],
                k_TmpVertices[m_TriangleIndices.z]);
        }

        // Returns the cartesian coordinates of a point based on its barycentric coordinates on a triangle.
        static Vector3 BarycentricToCartesian(Vector2 barycentricCoords, Vector3 a, Vector3 b, Vector3 c)
        {
            var u = barycentricCoords.x;
            var v = barycentricCoords.y;
            return a + u * (b - a) + v * (c - a);
        }

        /// <summary>
        /// Applies skinning to a vertex.
        /// </summary>
        /// <param name="vertex">The vertex to apply skinning to.</param>
        /// <param name="firstBoneIndex">The index of the first boneWeight entry affecting the vertex.</param>
        /// <param name="numBones">The number of bones affecting the vertex.</param>
        /// <param name="boneWeights">The indices and weights of the bones affecting vertices.</param>
        /// <param name="bindposes">The bind poses of the skinned mesh.</param>
        /// <param name="bones">The bones of the skinned mesh.</param>
        /// <returns>The skinned vertex.</returns>
        static Vector3 SkinVertex(Vector3 vertex, int firstBoneIndex, int numBones,
            List<BoneWeight1> boneWeights, List<Matrix4x4> bindposes, Transform[] bones)
        {
            var skinnedVertex = Vector3.zero;

            for (var i = 0; i != numBones; ++i)
            {
                var boneWeight = boneWeights[firstBoneIndex + i];
                var restPosition = bindposes[boneWeight.boneIndex].MultiplyPoint3x4(vertex);
                skinnedVertex += bones[boneWeight.boneIndex].transform.localToWorldMatrix.MultiplyPoint3x4(restPosition) * boneWeight.weight;
            }

            return skinnedVertex;
        }

        /// <summary>
        /// Performs a ray-mesh intersection test. Note that we pass both a ray and an intersection point.
        /// Typically one would compute the intersection point based solely on the ray.
        /// In our case, given that we use a graphics raycaster, we know both the ray and a decent estimation of the intersection point.
        /// This allows us to filter triangles based on whether or not their axis aligned bounding box contains the intersection point,
        /// which is a cheap test to perform. We then use the ray to evaluate the precise intersection point
        /// and compare it against the less precise intersection point evaluated by the graphics raycaster.
        /// </summary>
        /// <param name="mesh">The mesh to intersect.</param>
        /// <param name="transform">The transform of the gameObject holding the mesh.</param>
        /// <param name="worldRay">The ray used by the raycaster, in world space coordinates.</param>
        /// <param name="estimatedWorldIntersectionPoint">The intersection point evaluated by the graphics raycaster, in world space coordinates.</param>
        /// <param name="triangle">Indices of the 3 vertices corresponding to the intersected triangle.</param>
        /// <param name="barycentricCoordinates">Thee barycentric coordinates of the intersection point within the intersected triangle.</param>
        /// <returns>True if the ray intersects the mesh.</returns>
        static bool RayMeshIntersection(
            Mesh mesh, Transform transform, Ray worldRay, Vector3 estimatedWorldIntersectionPoint,
            out Vector3Int triangle, out Vector2 barycentricCoordinates)
        {
            if (!mesh.isReadable)
            {
                Debug.LogWarning($"Mesh \"{mesh.name}\" is not readable. Could not evaluate intersection with ray.");

                triangle = Vector3Int.zero;
                barycentricCoordinates = Vector2.zero;
                return false;
            }

            Assert.IsFalse(mesh.vertexCount == 0);

            mesh.GetVertices(k_TmpVertices);

            for (var i = 0; i != mesh.subMeshCount; ++i)
            {
                mesh.GetTriangles(k_TmpTriangles, i);

                var localPoint = transform.InverseTransformPoint(estimatedWorldIntersectionPoint);
                var localRay = new Ray(
                    transform.InverseTransformPoint(worldRay.origin),
                    transform.InverseTransformDirection(worldRay.direction));

                if (RayMeshIntersection(
                    k_TmpVertices, k_TmpTriangles, localRay, localPoint,
                    out var triangleIndex, out barycentricCoordinates))
                {
                    triangle = new Vector3Int(
                        k_TmpTriangles[triangleIndex * 3],
                        k_TmpTriangles[triangleIndex * 3 + 1],
                        k_TmpTriangles[triangleIndex * 3 + 2]);
                    return true;
                }
            }

            triangle = Vector3Int.zero;
            barycentricCoordinates = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Performs a ray-mesh intersection test. Note that we pass both a ray and an intersection point.
        /// Typically one would compute the intersection point based solely on the ray.
        /// In our case, given that we use a graphics raycaster, we know both the ray and a decent estimation of the intersection point.
        /// This allows us to filter triangles based on whether or not their axis aligned bounding box contains the intersection point,
        /// which is a cheap test to perform. We then use the ray to evaluate the precise intersection point.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="triangles">The mesh triangles, expressed as a collection of vertex indices.</param>
        /// <param name="localRay">The ray used by the raycaster, in object space.</param>
        /// <param name="estimatedLocalIntersectionPoint">The intersection point evaluated by the graphics raycaster, in object space.</param>
        /// <param name="triangleIndex">The index of the intersected triangle (not the index of its first vertex index within triangles.)</param>
        /// <param name="barycentricCoordinates">The barycentric coordinates of the intersection point within the intersected triangle.</param>
        /// <returns>True if the ray intersects the mesh.</returns>
        static bool RayMeshIntersection(
            List<Vector3> vertices, List<int> triangles, Ray localRay, Vector3 estimatedLocalIntersectionPoint,
            out int triangleIndex, out Vector2 barycentricCoordinates)
        {
            Assert.IsTrue(triangles.Count % 3 == 0);
            var trianglesCount = triangles.Count / 3;

            triangleIndex = -1;
            barycentricCoordinates = Vector2.zero;

            var minSqrDist = float.MaxValue;

            // Find triangles who intersect the ray.
            for (var i = 0; i != trianglesCount; ++i)
            {
                var a = vertices[triangles[i * 3]];
                var b = vertices[triangles[i * 3 + 1]];
                var c = vertices[triangles[i * 3 + 2]];

                if (RayTriangleIntersection(localRay, a, b, c, out _, out var u, out var v))
                {
                    var intersectionPoint = BarycentricToCartesian(new Vector2(u, v), a, b, c);
                    var sqrDist = (intersectionPoint - estimatedLocalIntersectionPoint).sqrMagnitude;
                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        triangleIndex = i;
                        barycentricCoordinates = new Vector2(u, v);
                    }
                }
            }

            return triangleIndex != -1 && minSqrDist < .01f;
        }

        /// <summary>
        /// Möller-Trumbore ray-triangle intersection algorithm.
        /// We ignore culling, since the hit has already been determined using a graphics raycaster.
        /// </summary>
        /// <param name="ray">The ray to test against the triangle.</param>
        /// <param name="v0">The triangle 1st vertex.</param>
        /// <param name="v1">The triangle 2nd vertex.</param>
        /// <param name="v2">The triangle 3rd vertex.</param>
        /// <param name="t">The distance from the ray origin to the intersection point.</param>
        /// <param name="u">The intersection point barycentric coordinates u component.</param>
        /// <param name="v">The intersection point barycentric coordinates v component.</param>
        /// <returns>True if the ray intersects the triangle.</returns>
        static bool RayTriangleIntersection(
            Ray ray, Vector3 a, Vector3 b, Vector3 c,
            out float t, out float u, out float v)
        {
            var ab = b - a;
            var ac = c - a;
            var pVec = Vector3.Cross(ray.direction, ac);
            var det = Vector3.Dot(ab, pVec);

            // ray and triangle are parallel if det is close to 0
            if (Mathf.Abs(det) < Mathf.Epsilon)
            {
                t = 0;
                u = 0;
                v = 0;
                return false;
            }

            var invDet = 1 / det;

            var tVec = ray.origin - a;
            u = Vector3.Dot(tVec, pVec) * invDet;

            if (u < 0 || u > 1)
            {
                t = 0;
                v = 0;
                return false;
            }

            var qVec = Vector3.Cross(tVec, ab);
            v = Vector3.Dot(ray.direction, qVec) * invDet;

            if (v < 0 || u + v > 1)
            {
                t = 0;
                return false;
            }

            t = Vector3.Dot(ac, qVec) * invDet;

            return true;
        }
    }
}
