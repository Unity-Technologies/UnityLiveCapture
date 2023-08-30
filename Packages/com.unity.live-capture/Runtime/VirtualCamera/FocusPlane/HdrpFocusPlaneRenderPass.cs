#if HDRP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Custom Pass that renders the focus plane to an intermediary render target.
    /// </summary>
    sealed class HdrpFocusPlaneRenderPass : CustomPass
    {
        protected override void Execute(CustomPassContext ctx)
        {
            var baseCamera = ctx.hdCamera.camera;
            if (baseCamera.cameraType == CameraType.SceneView)
                return;

            if (FocusPlaneMap.Instance.TryGetInstance(baseCamera, out var focusPlane))
            {
                if (focusPlane.isActiveAndEnabled)
                {
                    // width and height won't matter here since we're using an RTHandle with dynamic scaling.
                    focusPlane.AllocateTargetIfNeeded(ctx.hdCamera.actualWidth, ctx.hdCamera.actualHeight);

                    if (focusPlane.TryGetRenderTarget(out RTHandle targetHandle))
                    {
                        CoreUtils.SetRenderTarget(ctx.cmd, targetHandle);
                        CoreUtils.DrawFullScreen(ctx.cmd, focusPlane.RenderMaterial);
                    }
                }
            }
        }
    }
}
#endif
