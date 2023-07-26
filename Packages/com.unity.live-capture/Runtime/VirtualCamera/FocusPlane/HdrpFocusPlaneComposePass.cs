#if HDRP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Custom Pass that blends the render target in which the focus plane was rendered with the final frame.
    /// </summary>
    /// <remarks>
    /// Note the order attribute, the focus plane should be blended before the film format is rendered.
    /// </remarks>
    [CustomPassOrder(0)]
    class HdrpFocusPlaneComposePass : CustomPass
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
                    CoreUtils.DrawFullScreen(ctx.cmd, focusPlane.ComposeMaterial);
                }
            }
        }
    }
}
#endif
