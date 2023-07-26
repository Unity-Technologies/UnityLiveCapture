#if URP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.LiveCapture.VirtualCamera
{
    internal class UrpFrameLinesPass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            if (camera.cameraType == CameraType.SceneView)
                return;

            if (FrameLinesMap.Instance.TryGetInstance(camera, out var frameLines))
            {
                CommandBuffer cmd = CommandBufferPool.Get(FrameLines.k_ProfilingSamplerLabel);
                frameLines.Render(cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
#endif
