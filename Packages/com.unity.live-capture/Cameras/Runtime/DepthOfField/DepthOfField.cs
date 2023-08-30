using UnityEngine;
#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
using DepthOfFieldHdrp = UnityEngine.Rendering.HighDefinition.DepthOfField;
using DepthOfFieldModeHdrp = UnityEngine.Rendering.HighDefinition.DepthOfFieldMode;
#endif
#if URP_14_0_OR_NEWER
using UnityEngine.Rendering.Universal;
using DepthOfFieldUrp = UnityEngine.Rendering.Universal.DepthOfField;
using DepthOfFieldModeUrp = UnityEngine.Rendering.Universal.DepthOfFieldMode;
#endif

namespace Unity.LiveCapture.Cameras
{
    /// <summary>
    /// A component that manages the Depth Of Field effect.
    /// </summary>
    [ExecuteAlways]
    [ExcludeFromPreset]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(SharedVolumeProfile))]
    [HelpURL(Documentation.baseURL + "ref-component-depth-of-field" + Documentation.endURL)]
    sealed public class DepthOfField : MonoBehaviour
    {
#if SRP_CORE_14_0_OR_NEWER
#if URP_14_0_OR_NEWER
        [SerializeField, Tooltip("Use high quality Depth Of Field. Trades performance for visual quality.")]
        bool m_HighQuality;
#endif

        Camera m_Camera;
        SharedVolumeProfile m_SharedVolumeProfile;

        void OnEnable()
        {
            m_Camera = GetComponent<Camera>();
            m_SharedVolumeProfile = GetComponent<SharedVolumeProfile>();
        }

        void OnDisable()
        {
            SetActive(false);
        }

        void OnDestroy()
        {
            if (m_SharedVolumeProfile == null)
            {
                return;
            }
#if HDRP_14_0_OR_NEWER
            {
                m_SharedVolumeProfile.DestroyVolumeComponent<DepthOfFieldHdrp>();
            }
#endif
#if URP_14_0_OR_NEWER
            {
                m_SharedVolumeProfile.DestroyVolumeComponent<DepthOfFieldUrp>();
            }
#endif
        }

        void LateUpdate()
        {
            SetFocusDistance(m_Camera.focusDistance);
        }

        void SetActive(bool value)
        {
#if HDRP_14_0_OR_NEWER
            {
                if (m_SharedVolumeProfile.TryGetVolumeComponent<DepthOfFieldHdrp>(out var depthOfField))
                {
                    depthOfField.active = value;
                }
            }
#endif
#if URP_14_0_OR_NEWER
            {
                if (m_SharedVolumeProfile.TryGetVolumeComponent<DepthOfFieldUrp>(out var depthOfField))
                {
                    depthOfField.active = value;
                }
            }
#endif
        }

        void SetFocusDistance(float value)
        {
#if HDRP_14_0_OR_NEWER
            {
                var depthOfField = m_SharedVolumeProfile.GetOrCreateVolumeComponent<DepthOfFieldHdrp>();

                depthOfField.active = true;

                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.focusDistance, value);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.focusMode, DepthOfFieldModeHdrp.UsePhysicalCamera);
            }
#endif
#if URP_14_0_OR_NEWER
            {
                var depthOfField = m_SharedVolumeProfile.GetOrCreateVolumeComponent<DepthOfFieldUrp>();

                depthOfField.active = true;

                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.highQualitySampling, m_HighQuality);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.mode, DepthOfFieldModeUrp.Bokeh);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.bladeCurvature, 1);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.focusDistance, m_Camera.focusDistance);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.focalLength, m_Camera.focalLength);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.aperture, m_Camera.aperture);
                VolumeComponentUtility.UpdateParameterIfNeeded(depthOfField.bladeCount, m_Camera.bladeCount);
            }
#endif
        }
#endif
    }
}
