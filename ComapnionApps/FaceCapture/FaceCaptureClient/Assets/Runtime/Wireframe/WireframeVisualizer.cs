using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.CompanionApps.FaceCapture
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class WireframeVisualizer : MonoBehaviour
    {
        static readonly int k_WireColorProp = Shader.PropertyToID("_WireColor");
        static readonly int k_FillColorProp = Shader.PropertyToID("_FillColor");
        static readonly int k_WireThicknessProp = Shader.PropertyToID("_WireThickness");
        static readonly int k_WireSmoothingProp = Shader.PropertyToID("_WireSmoothing");

        static readonly VertexAttributeDescriptor[] k_Layout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4)
        };

#pragma warning disable 649
        [SerializeField, Tooltip("Wireframe Color")]
        Color m_WireColor;
        [SerializeField, Tooltip("Fill Color")]
        Color m_FillColor;
        [SerializeField, Tooltip("Wireframe thickness"), Range(1f, 10f)]
        float m_WireThickness;
        [SerializeField, Tooltip("Wireframe smoothing"), Range(1f, 10f)]
        float m_WireSmoothing;
#pragma warning restore 649

        Material m_Material;
        Mesh m_Mesh;
        MeshFilter m_MeshFilter;
        MeshRenderer m_MeshRenderer;
        NativeArray<uint> m_Indices;
        NativeArray<Vector4> m_Vertices;

        // Using OnEnable since Awake() is not called on script reload.
        void OnEnable()
        {
            m_Mesh = new Mesh();
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            var shader = Shader.Find("Unlit/Wireframe");
            Assert.IsNotNull(shader);
            m_Material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            m_MeshRenderer.sharedMaterial = m_Material;
            m_MeshFilter.sharedMesh = m_Mesh;

            UpdateUniforms();
        }

        void OnDisable()
        {
            DisposeBuffersIfNeeded();

            if (Application.isPlaying)
            {
                Destroy(m_Material);
                Destroy(m_Mesh);
            }
            else
            {
                DestroyImmediate(m_Material);
                DestroyImmediate(m_Mesh);
            }
        }

        void OnValidate()
        {
            if (m_Material != null)
                UpdateUniforms();
        }

        void DisposeBuffersIfNeeded()
        {
            if (m_Indices.IsCreated)
                m_Indices.Dispose();
            if (m_Vertices.IsCreated)
                m_Vertices.Dispose();
        }

        void UpdateUniforms()
        {
            m_Material.SetColor(k_WireColorProp, m_WireColor);
            m_Material.SetColor(k_FillColorProp, m_FillColor);
            m_Material.SetFloat(k_WireThicknessProp, m_WireThickness);
            m_Material.SetFloat(k_WireSmoothingProp, m_WireSmoothing);
        }

        public void UpdateGeometry(NativeArray<Vector3> vertices, NativeArray<int> indices)
        {
            Debug.Assert(vertices.Length != 0, "Vertices array cannot be zero.");
            Debug.Assert(indices.Length != 0, "Vertices array cannot be zero.");
            Debug.Assert(indices.Length >= vertices.Length, "Size of vertices array must be smaller than " +
                "or equal to the size of the indices array.");

            var totalVertices = indices.Length;

            if (!m_Indices.IsCreated || m_Indices.Length != totalVertices)
            {
                DisposeBuffersIfNeeded();

                m_Indices = new NativeArray<uint>(totalVertices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                m_Vertices = new NativeArray<Vector4>(totalVertices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                for (var i = 0; i != totalVertices; ++i) m_Indices[i] = (uint)i;

                m_Mesh.Clear();

                m_Mesh.SetVertexBufferParams(totalVertices, k_Layout);
                m_Mesh.SetVertexBufferData(m_Vertices, 0, 0, totalVertices);

                m_Mesh.SetIndexBufferParams(totalVertices, IndexFormat.UInt32);
                m_Mesh.SetIndexBufferData(m_Indices, 0, 0, totalVertices);

                // Note that we're using an arbitrary value to set a (very) large bounding box,
                // accuracy is not important there but we do not want the object to be culled.
                m_Mesh.SetSubMesh(0, new SubMeshDescriptor(0, totalVertices));
                m_Mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            }

            for (var i = 0; i != totalVertices; ++i)
            {
                var pos = vertices[indices[i]];

                // The fourth component is used to encode vertex index within triangle.
                var index = (i % 3) * 0.1f + Mathf.Epsilon;

                m_Vertices[i] = new Vector4(pos.x, pos.y, pos.z, index);
            }

            m_Mesh.SetVertexBufferData(m_Vertices, 0, 0, totalVertices);
        }
    }
}
