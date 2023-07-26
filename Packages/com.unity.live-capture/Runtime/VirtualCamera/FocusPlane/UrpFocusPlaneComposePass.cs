#if URP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Pass that blends the render target in which the focus plane was rendered with the final frame.
    /// </summary>
    class UrpFocusPlaneComposePass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            if (camera.cameraType == CameraType.SceneView)
                return;

            if (FocusPlaneMap.Instance.TryGetInstance(camera, out var focusPlane))
            {
                // Compositing is done by drawing a fullscreen quad as opposed to using Blit,
                // since it saves us the need to explicitly access the right destination target,
                // which turns out to be buggy in case of passes executed after post processes.
                var cmd = CommandBufferPool.Get(FocusPlaneConsts.ComposePlaneProfilingSamplerLabel);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, focusPlane.ComposeMaterial);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
#endif
