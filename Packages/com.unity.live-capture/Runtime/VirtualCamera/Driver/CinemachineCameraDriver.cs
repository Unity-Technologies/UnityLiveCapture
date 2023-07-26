using UnityEngine;
#if CINEMACHINE_2_4_OR_NEWER
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine ;
#else
using Cinemachine ;
using CinemachineCamera = Cinemachine.CinemachineVirtualCamera;
#endif
#endif

namespace Unity.LiveCapture.VirtualCamera
{
    [AddComponentMenu("")]
    [HelpURL(Documentation.baseURL + "ref-component-cinemachine-camera-driver" + Documentation.endURL)]
    class CinemachineCameraDriver : BaseCameraDriver, ICustomDamping
    {
#if CINEMACHINE_2_4_OR_NEWER
        [SerializeField, Tooltip("Cinemachine camera driver component.")]
        CinemachineDriverComponent m_CinemachineComponent = new CinemachineDriverComponent();
#if HDRP_14_0_OR_NEWER
        [SerializeField, Tooltip("High Definition Render Pipeline camera driver component.")]
        HdrpCinemachineCameraDriverComponent m_HdrpCinemachineComponent = new HdrpCinemachineCameraDriverComponent();
#endif
#if URP_14_0_OR_NEWER
        [SerializeField, Tooltip("Universal Render Pipeline camera driver component.")]
        UrpCinemachineCameraDriverComponent m_UrpComponent = new UrpCinemachineCameraDriverComponent();
#endif
        ICameraDriverImpl m_Impl;

        public CinemachineCamera CinemachineVirtualCamera
        {
            get => m_CinemachineComponent.CinemachineVirtualCamera;
            set
            {
                m_CinemachineComponent.CinemachineVirtualCamera = value;
                Validate();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_Impl == null)
            {
                m_Impl = new CompositeCameraDriverImpl(new ICameraDriverComponent[]
                {
                    m_CinemachineComponent,
#if HDRP_14_0_OR_NEWER
                    m_HdrpCinemachineComponent,
#endif
#if URP_14_0_OR_NEWER
                    m_UrpComponent,
#endif
                });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_Impl.Dispose();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            m_CinemachineComponent.Validate();

            Validate();
        }

        void Validate()
        {
#if HDRP_14_0_OR_NEWER
            m_HdrpCinemachineComponent.CinemachineVirtualCamera = CinemachineVirtualCamera;
#endif
#if URP_14_0_OR_NEWER
            m_UrpComponent.CinemachineVirtualCamera = CinemachineVirtualCamera;
#endif
        }

        protected override ICameraDriverImpl GetImplementation() => m_Impl;

        /// <inheritdoc/>
        public override Camera GetCamera()
        {
#if CINEMACHINE_3_0_0_OR_NEWER
            var brain = CinemachineCore.FindPotentialTargetBrain(CinemachineVirtualCamera);
#else
            var brain = CinemachineCore.Instance.FindPotentialTargetBrain(CinemachineVirtualCamera);
#endif
            if (brain != null)
                return brain.OutputCamera;

            return null;
        }

#else
        protected override void OnEnable()
        {
            base.OnEnable();

            Debug.LogError(
                $"A {nameof(CinemachineCameraDriver)} is used yet Cinemachine is not installed." +
                $"a {nameof(PhysicalCameraDriver)} should be used instead.");
        }

        protected override ICameraDriverImpl GetImplementation()
        {
            return null;
        }

        public override Camera GetCamera()
        {
            return null;
        }

#endif
        /// <inheritdoc/>
        public void SetDamping(Damping damping)
        {
#if CINEMACHINE_2_4_OR_NEWER
            m_CinemachineComponent.SetDamping(damping);
#endif
        }
    }
}
