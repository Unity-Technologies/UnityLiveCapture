#if URP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// URP specific focus plane implementation.
    /// </summary>
    /// <remarks>
    /// We find ourselves supporting both RenderTexture and RTHandle,
    /// as URP migrated to RTHandle at v13.1.2
    /// </remarks>
    class UrpFocusPlaneImpl : RTHandleFocusPlaneImpl
    {
        public UrpFocusPlaneImpl(Material composeMaterial) : base(composeMaterial)
        {
        }
    }
}
#endif
