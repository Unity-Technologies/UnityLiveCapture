using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    static class FocusPlaneConsts
    {
        internal const string RenderProfilingSamplerLabel = "Focus Plane Render";
        internal const string ComposePlaneProfilingSamplerLabel = "Focus Plane Compose";
        internal static readonly int InputTextureProperty = Shader.PropertyToID("_InputTexture");
    }
}
