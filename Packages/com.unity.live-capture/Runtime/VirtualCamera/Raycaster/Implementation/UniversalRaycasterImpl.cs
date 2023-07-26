#if URP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.LiveCapture.Rendering;

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    class UniversalRaycasterImpl : BaseScriptableRenderPipelineRaycasterImpl
    {
        RenderTexture m_PlaceholderTarget;

        public override void Initialize()
        {
            base.Initialize();

            // We assign a target texture even though its content is not relevant to us to avoid
            // "Missing Vulkan framebuffer attachment image?" errors on Linux + Vulkan.
            m_PlaceholderTarget = new RenderTexture(1, 1, 0);
            m_Camera.targetTexture = m_PlaceholderTarget;

            RenderPipelineBridge.RequestRenderFeature<InjectionPointRenderFeature>();
            InjectionPointRenderPass.onExecute += OnExecute;
        }

        public override void Dispose()
        {
            m_PlaceholderTarget.Release();
            InjectionPointRenderPass.onExecute -= OnExecute;
            base.Dispose();
        }

        void OnExecute(ScriptableRenderContext context, RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            if (camera != m_Camera)
                return;

            context.SetupCameraProperties(camera);

            var cmd = CommandBufferPool.Get("Graphics Raycast");

            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

            DoRender(cmd, context, m_Camera);

            CommandBufferPool.Release(cmd);
            context.Submit();
        }
    }
}
#endif
