#if URP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A Scriptable Render Feature responsible for managing virtual camera related render passes,
    /// including the film format and focus plane.
    /// </summary>
    /// <remarks>
    /// The use of a single render feature that manages multiple passes gives a precise control over their submission order,
    /// which is important, for example, if you want to render the film format after the focus plane.
    /// </remarks>
    class VirtualCameraScriptableRenderFeature : ScriptableRendererFeature
    {
        UrpFrameLinesPass m_UrpFrameLinesPass;
        UrpFocusPlaneRenderPass m_UrpFocusPlaneRenderPass;
        UrpFocusPlaneComposePass m_UrpFocusPlaneComposePass;

        public override void Create()
        {
            m_UrpFrameLinesPass = new UrpFrameLinesPass { renderPassEvent = RenderPassEvent.AfterRendering };
            m_UrpFocusPlaneRenderPass = new UrpFocusPlaneRenderPass { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
            m_UrpFocusPlaneComposePass = new UrpFocusPlaneComposePass { renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            if (FrameLinesMap.Instance.TryGetInstance(camera, out var frameLines))
            {
                if (frameLines.ShouldRender())
                {
                    renderer.EnqueuePass(m_UrpFrameLinesPass);
                }
            }

            if (FocusPlaneMap.Instance.TryGetInstance(camera, out var focusPlane))
            {
                if (focusPlane.isActiveAndEnabled)
                {
                    focusPlane.AllocateTargetIfNeeded(camera.pixelWidth, camera.pixelHeight);

                    renderer.EnqueuePass(m_UrpFocusPlaneRenderPass);
                    renderer.EnqueuePass(m_UrpFocusPlaneComposePass);
                }
            }
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            m_UrpFocusPlaneRenderPass.Source = renderer.cameraColorTargetHandle;
        }
    }
}
#endif
