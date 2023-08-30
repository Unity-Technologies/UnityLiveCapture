#if URP_14_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.LiveCapture.Cameras;
using DepthOfFieldUrp = UnityEngine.Rendering.Universal.DepthOfField;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class UrpCameraDriverComponent : ICameraDriverComponent
    {
        const string k_ProfileName = "Camera Driver Profile";

        [SerializeField, Tooltip("Use high quality Depth Of Field. Trades performance for visual quality.")]
        bool m_UseHighQualityDepthOfField;
        [SerializeField, HideInInspector]
        VolumeProfile m_Profile;
        DepthOfFieldUrp m_DepthOfField;

        public Camera Camera { get; set; }

        public bool UseHighQualityDepthOfField
        {
            get => m_UseHighQualityDepthOfField;
            set => m_UseHighQualityDepthOfField = value;
        }

        void PrepareIfNeeded()
        {
            Debug.Assert(Camera != null);

            Assert.IsNotNull(Camera, $"{nameof(UrpCameraDriverComponent)} expects a GameObject holding a Camera.");

            var volume = VolumeComponentUtility.GetOrAddVolume(Camera.gameObject, false);

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

            m_DepthOfField = VolumeComponentUtility.GetOrAddVolumeComponent<DepthOfFieldUrp>(m_Profile);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.mode, DepthOfFieldMode.Bokeh);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.bladeCurvature, 1);
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

            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.highQualitySampling, m_UseHighQualityDepthOfField);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.focusDistance, value);
        }

        /// <inheritdoc/>
        public void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
            PrepareIfNeeded();

            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.highQualitySampling, m_UseHighQualityDepthOfField);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.focalLength, lens.FocalLength);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.aperture, lens.Aperture);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.bladeCount, intrinsics.BladeCount);
        }
    }
}
#endif
