#if HDRP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// HDRP specific focus plane implementation.
    /// </summary>
    class HdrpFocusPlaneImpl : RTHandleFocusPlaneImpl
    {
        CustomPassManager.Handle<HdrpFocusPlaneRenderPass> m_RenderPassHandle;
        CustomPassManager.Handle<HdrpFocusPlaneComposePass> m_ComposePassHandle;

        public HdrpFocusPlaneImpl(Material composeMaterial) : base(composeMaterial)
        {
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            m_RenderPassHandle = new CustomPassManager.Handle<HdrpFocusPlaneRenderPass>(CustomPassInjectionPoint.BeforePostProcess);
            m_ComposePassHandle = new CustomPassManager.Handle<HdrpFocusPlaneComposePass>(CustomPassInjectionPoint.AfterPostProcess);
            m_RenderPassHandle.GetPass().name = FocusPlaneConsts.RenderProfilingSamplerLabel;
            m_ComposePassHandle.GetPass().name = FocusPlaneConsts.ComposePlaneProfilingSamplerLabel;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            m_RenderPassHandle.Dispose();
            m_ComposePassHandle.Dispose();

            base.Dispose();
        }
    }
}
#endif
