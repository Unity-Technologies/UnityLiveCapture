#if CINEMACHINE_2_4_OR_NEWER
using System;
using UnityEngine;
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine ;
using CinemachineCamera = Cinemachine.CinemachineVirtualCamera;
using CinemachineFollow = Cinemachine.CinemachineTransposer;
#endif

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class CinemachineDriverComponent : ICameraDriverComponent
    {
        [SerializeField, Tooltip("The virtual camera to be driven.")]
        CinemachineCamera m_CinemachineVirtualCamera;
        CinemachineFollow m_Transposer;
        CinemachineSameAsFollowTarget m_Aim;

        /// <summary>
        /// The Cinemachine virtual camera driven by this component.
        /// </summary>
        public CinemachineCamera CinemachineVirtualCamera
        {
            get => m_CinemachineVirtualCamera;
            set
            {
                m_CinemachineVirtualCamera = value;
                Validate();
            }
        }

        /// <summary>
        /// Tries and fetch required Cinemachine components from the virtual camera.
        /// </summary>
        public void Validate()
        {
            if (m_CinemachineVirtualCamera == null)
                return;

#if CINEMACHINE_3_0_0_OR_NEWER
            var transposer = m_CinemachineVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFollow;
#else
            var transposer = m_CinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
#endif

            if (transposer != null)
            {
                // Transposer needs reset only if it has changed.
                if (transposer != m_Transposer)
                {
#if CINEMACHINE_3_0_0_OR_NEWER
                    transposer.TrackerSettings.BindingMode = Cinemachine.TargetTracking.BindingMode.LockToTarget;
                    transposer.FollowOffset = Vector3.zero;
                    transposer.TrackerSettings.PositionDamping = Vector3.zero;
#else
                    transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
                    transposer.m_FollowOffset = Vector3.zero;
                    transposer.m_XDamping = 0f;
                    transposer.m_YDamping = 0f;
                    transposer.m_ZDamping = 0f;
#endif
                }
                m_Transposer = transposer;
            }
            else
            {
                Debug.LogError($"{nameof(m_CinemachineVirtualCamera)} is expected to hold a {nameof(CinemachineFollow)} component.");
            }

            m_Aim = m_CinemachineVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineSameAsFollowTarget;
            if (m_Aim == null)
            {
                Debug.LogError($"{nameof(m_CinemachineVirtualCamera)} is expected to hold a {nameof(CinemachineSameAsFollowTarget)} component.");
            }

            if (m_Transposer == null || m_Aim == null)
            {
                m_CinemachineVirtualCamera = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose() {}

        /// <inheritdoc/>
        public void SetDamping(Damping damping)
        {
            // In case the component was not assigned a virtual camera yet.
            if (m_Transposer == null || m_Aim == null)
                return;

            var targetDamping = damping;

            if (!damping.Enabled)
            {
                targetDamping.Body = Vector3.zero;
                targetDamping.Aim = 0;
            }

#if CINEMACHINE_3_0_0_OR_NEWER
            m_Transposer.TrackerSettings.PositionDamping = targetDamping.Body;
            m_Aim.Damping = targetDamping.Aim;
#else
            m_Transposer.m_XDamping = targetDamping.Body.x;
            m_Transposer.m_YDamping = targetDamping.Body.y;
            m_Transposer.m_ZDamping = targetDamping.Body.z;
            m_Aim.m_Damping = targetDamping.Aim;
#endif
        }

        /// <inheritdoc/>
        public void SetFocusDistance(float value) {}

        /// <inheritdoc/>
        public void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
            // In case the component was not assigned a virtual camera yet.
            if (m_CinemachineVirtualCamera == null)
                return;

            // The Cinemachine brain's Camera must use physical properties
            // for sensor size to be pulled from the brain's output Camera's sensorSize property.
            // lens.SensorSize will be overwritten by Cinemachine so no point in assigning it here.
            // See `LensSettings.SnapshotCameraReadOnlyProperties`

#if CINEMACHINE_3_0_0_OR_NEWER
            var brain = CinemachineCore.FindPotentialTargetBrain(CinemachineVirtualCamera);
#else
            var brain = CinemachineCore.Instance.FindPotentialTargetBrain(CinemachineVirtualCamera);
#endif
            if (brain != null)
            {
#if CINEMACHINE_3_0_0_OR_NEWER
                brain.LensModeOverride = new CinemachineBrain.LensModeOverrideSettings
                {
                    DefaultMode = LensSettings.OverrideModes.Physical,
                    Enabled = true,
                };
#endif
#if !CINEMACHINE_2_7_0_OR_NEWER
                brain.OutputCamera.sensorSize = cameraBody.SensorSize;
#endif
                brain.OutputCamera.usePhysicalProperties = true;
            }
#if CINEMACHINE_3_0_0_OR_NEWER
            m_CinemachineVirtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.Physical;
            m_CinemachineVirtualCamera.Lens.PhysicalProperties.SensorSize = cameraBody.SensorSize;
            m_CinemachineVirtualCamera.Lens.FieldOfView = Camera.FocalLengthToFieldOfView(lens.FocalLength, cameraBody.SensorSize.y);
#else
#if CINEMACHINE_2_7_0_OR_NEWER
            m_CinemachineVirtualCamera.m_Lens.ModeOverride = LensSettings.OverrideModes.Physical;
#endif
            m_CinemachineVirtualCamera.m_Lens.SensorSize = cameraBody.SensorSize;
            m_CinemachineVirtualCamera.m_Lens.FieldOfView = Camera.FocalLengthToFieldOfView(lens.FocalLength, cameraBody.SensorSize.y);
#endif
        }

        /// <inheritdoc/>
        public void EnableDepthOfField(bool value) {}
    }
}
#endif
