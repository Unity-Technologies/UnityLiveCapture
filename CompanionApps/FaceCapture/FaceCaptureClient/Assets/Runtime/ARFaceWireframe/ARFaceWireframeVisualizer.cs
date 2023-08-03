using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.CompanionApps.FaceCapture
{
    [RequireComponent(typeof(ARFace), typeof(WireframeVisualizer))]
    public class ARFaceWireframeVisualizer : MonoBehaviour
    {
        ARFace m_Face;
        WireframeVisualizer m_WireframeVisualizer;

        void Awake()
        {
            m_Face = GetComponent<ARFace>();
            m_WireframeVisualizer = GetComponent<WireframeVisualizer>();
        }

        void OnEnable()
        {
            m_Face.updated += OnUpdated;

            SetMeshGeometry();
        }

        void OnDisable()
        {
            m_Face.updated -= OnUpdated;
        }

        void SetMeshGeometry()
        {
            var nativeVertices = m_Face.vertices;
            var nativeIndices = m_Face.indices;

            m_WireframeVisualizer.UpdateGeometry(nativeVertices, nativeIndices);
        }

        void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
        {
            SetMeshGeometry();
        }
    }
}
