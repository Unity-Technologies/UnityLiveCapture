#if URP_14_0_OR_NEWER && CINEMACHINE_2_4_OR_NEWER
using System;
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
using Cinemachine.PostFX;
using CinemachineCamera = Cinemachine.CinemachineVirtualCamera;
#endif
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityObject = UnityEngine.Object;
using Unity.LiveCapture.Cameras;
using DepthOfFieldUrp = UnityEngine.Rendering.Universal.DepthOfField;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class UrpCinemachineCameraDriverComponent : ICameraDriverComponent
    {
        const string k_ProfileName = "Camera Driver Profile";

        [SerializeField, HideInInspector]
        CinemachineVolumeSettings m_Volume;
        [SerializeField, HideInInspector]
        VolumeProfile m_Profile;
        DepthOfFieldUrp m_DepthOfField;

        /// <summary>
        /// The Cinemachine virtual camera driven by this component.
        /// </summary>

        public CinemachineCamera CinemachineVirtualCamera { get; set; }

        void PrepareIfNeeded()
        {
            HandleVirtualCameraChange();

            if (CinemachineVirtualCamera != null)
            {
                m_Volume = VolumeComponentUtilityCM.GetOrAddVolumeSettings(CinemachineVirtualCamera);

                // Profile instances will end up being shared when duplicating objects through serialization.
                // We have to invalidate the profile when we don't know who created it.
                if (!VolumeProfileTracker.Instance.TryRegisterProfileOwner(m_Profile, m_Volume))
                {
                    m_Profile = null;
                    m_DepthOfField = null;
                }
            }

            if (m_Profile == null)
            {
                m_Profile = ScriptableObject.CreateInstance<VolumeProfile>();
                m_Profile.name = k_ProfileName;
            }

            if (m_DepthOfField == null)
            {
                m_DepthOfField = VolumeComponentUtility.GetOrAddVolumeComponent<DepthOfFieldUrp>(m_Profile);

                VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.mode, DepthOfFieldMode.Bokeh);
                VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.bladeCurvature, 1);
            }

            if (m_Volume != null)
            {
#if CINEMACHINE_3_0_0_OR_NEWER
                m_Volume.Profile = m_Profile;
#else
                m_Volume.m_Profile = m_Profile;
#endif

                VolumeProfileTracker.Instance.TryRegisterProfileOwner(m_Profile, m_Volume);
            }
        }

        void HandleVirtualCameraChange()
        {
            if (CinemachineVirtualCamera == null)
            {
                DisposeVolumeIfNeeded();
            }
            else if (m_Volume != null && m_Volume.gameObject != CinemachineVirtualCamera.gameObject)
            {
                DisposeVolumeIfNeeded();
            }
        }

        void DisposeVolumeIfNeeded()
        {
            if (VolumeProfileTracker.Instance.UnregisterProfile(m_Profile))
            {
                AdditionalCoreUtils.DestroyIfNeeded(ref m_Profile);
                AdditionalCoreUtils.DestroyIfNeeded(ref m_DepthOfField);
            }

            m_Volume = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposeVolumeIfNeeded();
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
            PrepareIfNeeded();

            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.focalLength, lens.FocalLength);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.aperture, lens.Aperture);
            VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.bladeCount, intrinsics.BladeCount);
        }
    }
}
#endif
