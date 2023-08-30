#if HDRP_14_0_OR_NEWER && CINEMACHINE_2_4_OR_NEWER
using System;
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
using CinemachineCamera = Cinemachine.CinemachineVirtualCamera;
using Cinemachine.PostFX;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.LiveCapture.Cameras;
using DepthOfFieldHdrp = UnityEngine.Rendering.HighDefinition.DepthOfField;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class HdrpCinemachineCameraDriverComponent : ICameraDriverComponent
    {
        const string k_ProfileName = "Camera Driver Profile";

        [SerializeField, HideInInspector]
        CinemachineVolumeSettings m_Volume;
        [SerializeField, HideInInspector]
        VolumeProfile m_Profile;
        DepthOfFieldHdrp m_DepthOfField;

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
                m_DepthOfField = VolumeComponentUtility.GetOrAddVolumeComponent<DepthOfFieldHdrp>(m_Profile);

                VolumeComponentUtility.UpdateParameterIfNeeded(m_DepthOfField.focusMode, DepthOfFieldMode.UsePhysicalCamera);
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
            if (CinemachineVirtualCamera != null)
            {
#if CINEMACHINE_3_0_0_OR_NEWER
                CinemachineVirtualCamera.Lens.PhysicalProperties.Anamorphism = intrinsics.Anamorphism;
                CinemachineVirtualCamera.Lens.PhysicalProperties.BarrelClipping = intrinsics.BarrelClipping;
                CinemachineVirtualCamera.Lens.PhysicalProperties.Curvature = intrinsics.Curvature;
                CinemachineVirtualCamera.Lens.PhysicalProperties.BladeCount = intrinsics.BladeCount;
                CinemachineVirtualCamera.Lens.PhysicalProperties.Iso = cameraBody.Iso;
                CinemachineVirtualCamera.Lens.PhysicalProperties.ShutterSpeed = cameraBody.ShutterSpeed;
                CinemachineVirtualCamera.Lens.PhysicalProperties.Aperture = lens.Aperture;
#else
                CinemachineVirtualCamera.m_Lens.Anamorphism = intrinsics.Anamorphism;
                CinemachineVirtualCamera.m_Lens.BarrelClipping = intrinsics.BarrelClipping;
                CinemachineVirtualCamera.m_Lens.Curvature = intrinsics.Curvature;
                CinemachineVirtualCamera.m_Lens.BladeCount = intrinsics.BladeCount;
                CinemachineVirtualCamera.m_Lens.Iso = cameraBody.Iso;
                CinemachineVirtualCamera.m_Lens.ShutterSpeed = cameraBody.ShutterSpeed;
                CinemachineVirtualCamera.m_Lens.Aperture = lens.Aperture;
#endif
            }
        }
    }
}
#endif
