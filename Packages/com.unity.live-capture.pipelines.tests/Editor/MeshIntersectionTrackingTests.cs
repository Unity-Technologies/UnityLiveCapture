using System;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Tests.Editor
{
    public class MeshIntersectionTrackingTests
    {
        const float k_DistToCam = 10;
        const float k_SphereRadius = .5f;
        const float k_HalfCubeSize = .5f;
        const float k_Tolerance = .01f;
        static readonly Vector2 k_NormalizedScreenCenter = Vector2.one * .5f;
        static readonly Vector2 k_CameraFarNear = new Vector2(0.3f, 1000);

        MeshIntersectionTracker m_MeshIntersectionTracker = new MeshIntersectionTracker();
        IRaycaster m_Raycaster;
        Camera m_MainCamera;
        Transform m_Sphere;
        Transform m_Bone;
        Transform m_SkinnedCube;

        [SetUp]
        public void Setup()
        {
            Utility.OpenGameView();

            m_Raycaster = RaycasterFactory.Create();
            m_MeshIntersectionTracker.Initialize();

            // Create camera
            {
                var camera = new GameObject("Test Camera", typeof(Camera))
                {
                    hideFlags = HideFlags.DontSave
                };
                m_MainCamera = camera.GetComponent<Camera>();
                m_MainCamera.nearClipPlane = k_CameraFarNear.x;
                m_MainCamera.farClipPlane = k_CameraFarNear.y;
                m_MainCamera.usePhysicalProperties = false;
                m_MainCamera.fieldOfView = 90;
            }

            // Create static sphere.
            {
                var sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereGo.hideFlags = HideFlags.DontSave;
                m_Sphere = sphereGo.transform;
            }

            // Create skinned cube.
            {
                var tmpGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var cubeMesh = tmpGo.GetComponent<MeshFilter>().sharedMesh;

                var vertices = cubeMesh.vertices;
                var boneWeights = new BoneWeight[vertices.Length];

                for (var i = 0; i != boneWeights.Length; ++i)
                {
                    boneWeights[i] = new BoneWeight
                    {
                        weight0 = 1,
                        weight1 = 0,
                        weight2 = 0,
                        weight3 = 0,
                        boneIndex0 = 0,
                        boneIndex1 = 0,
                        boneIndex2 = 0,
                        boneIndex3 = 0
                    };
                }

                cubeMesh.boneWeights = boneWeights;

                cubeMesh.bindposes = new[] { Matrix4x4.identity };

                var boneGo = new GameObject("Bone");
                m_Bone = boneGo.transform;

                var skinnedMeshGo = new GameObject("Skinned Cube", typeof(SkinnedMeshRenderer))
                {
                    hideFlags = HideFlags.DontSave
                };
                m_SkinnedCube = skinnedMeshGo.transform;
                var skinnedMeshRenderer = skinnedMeshGo.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.sharedMesh = cubeMesh;
                skinnedMeshRenderer.rootBone = m_Bone;
                skinnedMeshRenderer.bones = new[] { m_Bone };
                skinnedMeshRenderer.sharedMaterial = tmpGo.GetComponent<MeshRenderer>().sharedMaterial;

                Object.DestroyImmediate(tmpGo);
            }
        }

        [TearDown]
        public void TearDown()
        {
            m_MeshIntersectionTracker.Dispose();
            RaycasterFactory.Dispose(m_Raycaster);
            Object.DestroyImmediate(m_MainCamera.gameObject);
            Object.DestroyImmediate(m_SkinnedCube.gameObject);
            Object.DestroyImmediate(m_Sphere.gameObject);
            Object.DestroyImmediate(m_Bone.gameObject);
        }

        [TestCase(false, MeshIntersectionTracker.Mode.MeshDynamic)]
        [TestCase(true, MeshIntersectionTracker.Mode.MeshStatic)]
        public void TrackStaticMesh(bool avoidReadback, object expectedModeAsObj)
        {
            MeshIntersectionTracker.Mode expectedMode = (MeshIntersectionTracker.Mode)expectedModeAsObj;

            m_SkinnedCube.gameObject.SetActive(false);
            m_Sphere.gameObject.SetActive(true);

            m_Sphere.position = Vector3.zero;
            m_MainCamera.transform.position = Vector3.forward * -k_DistToCam;
            m_MainCamera.transform.forward = Vector3.forward;

            Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);

            if (m_Raycaster.Raycast(m_MainCamera, k_NormalizedScreenCenter, out var ray, out var gameObject, out var hit))
            {
                Assert.IsTrue(gameObject == m_Sphere.gameObject);
                Assert.IsTrue(Approximately(hit.distance, k_DistToCam - k_SphereRadius, k_Tolerance));

                Assert.IsTrue(m_MeshIntersectionTracker.TryTrack(gameObject, ray, hit.point, avoidReadback));
                Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == expectedMode);

                // We have started tracking the sphere, we will now move it around and make sure the intersection is being tracked properly.
                // Rotate the sphere, the sphere has not moved but the tracking point does.
                m_Sphere.Rotate(Vector3.up, 180);
                Assert.IsTrue(m_MeshIntersectionTracker.TryUpdate(out var worldPosition));
                Assert.IsTrue(Approximately(Vector3.forward * k_SphereRadius, worldPosition, k_Tolerance));

                // Move the transform, make sure the tracked point follows.
                m_Sphere.position += Vector3.one;
                var previousWorldPosition = worldPosition;
                Assert.IsTrue(m_MeshIntersectionTracker.TryUpdate(out worldPosition));
                Assert.IsTrue(Approximately(worldPosition, previousWorldPosition + Vector3.one, k_Tolerance));

                // Tracking stops when gameObject is deactivated.
                m_Sphere.gameObject.SetActive(false);
                Assert.IsFalse(m_MeshIntersectionTracker.TryUpdate(out worldPosition));
                Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);
            }
            else
            {
                throw new InvalidOperationException($"Raycast failed to hit [{m_Sphere.name}].");
            }
        }

        [TestCase(false, MeshIntersectionTracker.Mode.SkinnedMesh)]
        [TestCase(true, MeshIntersectionTracker.Mode.SkinnedMeshCpuSkinning)]
        public void TrackSkinnedMesh(bool avoidReadback, object expectedModeAsObj)
        {
            MeshIntersectionTracker.Mode expectedMode = (MeshIntersectionTracker.Mode)expectedModeAsObj;

            m_SkinnedCube.gameObject.SetActive(true);
            m_Sphere.gameObject.SetActive(false);

            m_SkinnedCube.position = Vector3.zero;
            m_MainCamera.transform.position = Vector3.forward * -k_DistToCam;
            m_MainCamera.transform.forward = Vector3.forward;

            Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);

            if (m_Raycaster.Raycast(m_MainCamera, k_NormalizedScreenCenter, out var ray, out var gameObject, out var hit))
            {
                Assert.IsTrue(gameObject == m_SkinnedCube.gameObject);

                Assert.IsTrue(m_MeshIntersectionTracker.TryTrack(gameObject, ray, hit.point, avoidReadback));
                Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == expectedMode);
                Assert.IsTrue(m_MeshIntersectionTracker.TryUpdate(out var worldPosition, true));
                Assert.IsTrue(Approximately(worldPosition, new Vector3(0, 0, -k_HalfCubeSize), k_Tolerance));

                // Move the bone, make sure the tracked point follows.
                var previousWorldPosition = worldPosition;
                m_Bone.Translate(Vector3.right);
                Assert.IsTrue(m_MeshIntersectionTracker.TryUpdate(out worldPosition, true));
                Assert.IsTrue(Approximately(worldPosition, previousWorldPosition + Vector3.right, k_Tolerance));

                // Tracking stops when gameObject is deactivated.
                m_SkinnedCube.gameObject.SetActive(false);
                Assert.IsFalse(m_MeshIntersectionTracker.TryUpdate(out worldPosition));
                Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Raycast failed to hit [{m_SkinnedCube.name}]. " +
                    "This test is known to have issues when ran on Parallels.");
            }
        }

        [Test]
        public void TrackingFailsIfHitPointIsInaccurate()
        {
            var hitPoint = Vector3.one * 1200;
            var ray = new Ray
            {
                origin = Vector3.zero,
                direction = Vector3.right
            };
            Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);
            Assert.IsFalse(m_MeshIntersectionTracker.TryTrack(m_SkinnedCube.gameObject, ray, hitPoint));
            Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);
            Assert.IsFalse(m_MeshIntersectionTracker.TryTrack(m_Sphere.gameObject, ray, hitPoint));
            Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);
        }

        [Test]
        public void UpdateFailsIfNotTracking()
        {
            Assert.IsTrue(m_MeshIntersectionTracker.CurrentMode == MeshIntersectionTracker.Mode.None);
            Assert.IsFalse(m_MeshIntersectionTracker.TryUpdate(out _));
        }

        // Compare two vectors on a per-component basis with a tolerance.
        static bool Approximately(Vector3 a, Vector3 b, float tolerance)
        {
            return Approximately(a.x, b.x, tolerance) &&
                Approximately(a.y, b.y, tolerance) &&
                Approximately(a.z, b.z, tolerance);
        }

        static bool Approximately(float a, float b, float tolerance)
        {
            return Mathf.Abs(b - a) < tolerance;
        }
    }
}
