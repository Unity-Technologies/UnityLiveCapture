using UnityEngine;

#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.LiveCapture.VirtualCamera
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(Camera))]
    [HelpURL(Documentation.baseURL + "ref-component-physical-camera-driver" + Documentation.endURL)]
    [ExcludeFromPreset]
    class PhysicalCameraDriver : BaseCameraDriver
    {
        VanillaCameraDriverComponent m_VanillaCameraDriverComponent = new VanillaCameraDriverComponent();
#if HDRP_14_0_OR_NEWER
        [SerializeField, Tooltip("High Definition Render Pipeline camera driver component.")]
        HdrpCoreCameraDriverComponent m_HdrpCoreComponent = new HdrpCoreCameraDriverComponent();
#endif
#if URP_14_0_OR_NEWER
        [SerializeField, Tooltip("Universal Render Pipeline camera driver component.")]
        UrpCameraDriverComponent m_UrpComponent = new UrpCameraDriverComponent();
#endif

        Camera m_Camera;
        ICameraDriverImpl m_Impl;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Camera = GetComponent<Camera>();

            m_VanillaCameraDriverComponent.Camera = m_Camera;
#if HDRP_14_0_OR_NEWER
            if (!gameObject.TryGetComponent<HDAdditionalCameraData>(out var hdData))
            {
                gameObject.AddComponent<HDAdditionalCameraData>();
            }

            m_HdrpCoreComponent.Root = gameObject;
            m_HdrpCoreComponent.Camera = m_Camera;
#endif
#if URP_14_0_OR_NEWER
            m_UrpComponent.Camera = m_Camera;
#endif

            if (m_Impl == null)
            {
                m_Impl = new CompositeCameraDriverImpl(new ICameraDriverComponent[]
                {
                    m_VanillaCameraDriverComponent,
#if HDRP_14_0_OR_NEWER
                    m_HdrpCoreComponent,
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

        protected override ICameraDriverImpl GetImplementation() => m_Impl;

        /// <inheritdoc/>
        public override Camera GetCamera() => m_Camera;
    }
}
