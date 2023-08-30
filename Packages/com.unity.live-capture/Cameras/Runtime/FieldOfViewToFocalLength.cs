using UnityEngine;

namespace Unity.LiveCapture.Cameras
{
    /// <summary>
    /// A component that synchronizes the field of view of a camera with its focal length.
    /// </summary>
    /// <remarks>
    /// When the camera is using physical properties, Unity ignores any field of view value set from
    /// an animation clip. This component allows the field of view to be animated while
    /// maintaining the correct focal length.
    /// When not using physical properties, the component will update the field of view field to match
    /// the camera's one.
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [HelpURL(Documentation.baseURL + "ref-component-field-of-view-to-focal-length" + Documentation.endURL)]
    public sealed class FieldOfViewToFocalLength : MonoBehaviour, IPreviewable
    {
        const float k_FieldOfViewMin = 1f;
        const float k_FieldOfViewMax = 179f;
        const float k_DefaultFieldOfView = 45f;

        [SerializeField, Range(k_FieldOfViewMin, k_FieldOfViewMax)]
        float m_FieldOfView = k_DefaultFieldOfView;

        Camera m_Camera;

        /// <summary>
        /// The field of view of the camera.
        /// </summary>
        public float FieldOfView
        {
            get => m_FieldOfView;
            set => m_FieldOfView = value;
        }

        void OnValidate()
        {
            m_FieldOfView = Mathf.Clamp(m_FieldOfView, k_FieldOfViewMin, k_FieldOfViewMax);
        }

        void OnEnable()
        {
            m_Camera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (m_Camera.usePhysicalProperties)
            {
                m_Camera.focalLength = Camera.FieldOfViewToFocalLength(m_FieldOfView, m_Camera.sensorSize.y);
            }
            else
            {
                m_FieldOfView = m_Camera.fieldOfView;
            }
        }

        /// <inheritdoc />
        void IPreviewable.Register(IPropertyPreviewer previewer)
        {
            previewer.Register(m_Camera, "m_FocalLength");
        }
    }
}
