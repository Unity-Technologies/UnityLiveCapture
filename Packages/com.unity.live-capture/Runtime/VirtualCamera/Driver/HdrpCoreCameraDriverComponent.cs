#if HDRP_14_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.LiveCapture.Cameras;
using DepthOfFieldHdrp = UnityEngine.Rendering.HighDefinition.DepthOfField;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class HdrpCoreCameraDriverComponent : ICameraDriverComponent
    {
        const string k_ProfileName = "Camera Driver Profile";

        [SerializeField, HideInInspector]
        VolumeProfile m_Profile;
        DepthOfFieldHdrp m_DepthOfField;

        /// <summary>
        /// The GameObject to add the Volume to.
        /// </summary>
        public GameObject Root { get; set; }

        /// <summary>
        /// Camera to be driven by this component.
        /// </summary>
        public Camera Camera { get; set; }

        void PrepareIfNeeded()
        {
            Debug.Assert(Root != null);

            var volume = VolumeComponentUtility.GetOrAddVolume(Root, false);

            // Profile instances will end up being shared when duplicating objects through serialization.
            // We have to invalidate the profile when we don't know who created it.
            if (!VolumeProfileTracker.Instance.TryRegisterProfileOwner(m_Profile, volume))
            {
                m_Profile = null;
            }

            if (m_Profile == null)
            {
                m_Profile = volume.profile;

                if (string.IsNullOrEmpty(m_Profile.name))
                {
                    m_Profile.name = k_ProfileName;
                }

                VolumeProfileTracker.Instance.TryRegisterProfileOwner(m_Profile, volume);
            }

            volume.profile = m_Profile;

            m_DepthOfField = VolumeComponentUtility.GetOrAddVolumeComponent<DepthOfFieldHdrp>(m_Profile);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.focusMode, DepthOfFieldMode.UsePhysicalCamera);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VolumeProfileTracker.Instance.UnregisterProfile(m_Profile);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_Profile);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_DepthOfField);
        }

        /// <inheritdoc/>
        public void SetDamping(Damping dampingData) {}

        /// <inheritdoc/>
        public void EnableDepthOfField(bool value)
        {
            PrepareIfNeeded();

            m_DepthOfField.active = value;
        }

        /// <inheritdoc/>
        public void SetFocusDistance(float value)
        {
            PrepareIfNeeded();

            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.focusDistance, value);
        }

        /// <inheritdoc/>
        public void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
        }
    }
}
#endif
