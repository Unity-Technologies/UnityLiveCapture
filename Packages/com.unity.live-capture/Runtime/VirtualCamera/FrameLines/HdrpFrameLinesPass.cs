#if HDRP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A Custom Pass to render the film format associated with a Camera.
    /// </summary>
    /// <remarks>
    /// Note the order attribute, the film format should be rendered after the focus plane.
    /// </remarks>
    [CustomPassOrder(10)]
    class HdrpFrameLinesPass : CustomPass
    {
        protected override void Execute(CustomPassContext ctx)
        {
            var baseCamera = ctx.hdCamera.camera;
            if (baseCamera.cameraType == CameraType.SceneView)
                return;

            if (FrameLinesMap.Instance.TryGetInstance(baseCamera, out var frameLines))
            {
                if (frameLines.ShouldRender())
                {
                    CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer, ClearFlag.None);
                    frameLines.Render(ctx.cmd);
                }
            }
        }
    }
}
#endif
