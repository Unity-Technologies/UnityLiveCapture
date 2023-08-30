using UnityEngine;
#if URP_14_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.LiveCapture.Rendering
{
    /// <summary>
    /// The bridge's default implementation exists so that the bridge itself is not filled
    /// with null checks on the implementation property.
    /// It can be used to implement user warnings or more
    /// when for some reason the editor implementation has not been bound.
    /// </summary>
    class RenderPipelineBridgeDefaultImpl : IRenderPipelineBridge
    {
        internal static readonly IRenderPipelineBridge s_Instance = new RenderPipelineBridgeDefaultImpl();

#if URP_14_0_OR_NEWER
        /// <inheritdoc/>
        public T RequestRenderFeature<T>() where T : ScriptableRendererFeature
        {
            Error();
            return null;
        }

        /// <inheritdoc/>
        public bool HasRenderFeature<T>(out T result) where T : ScriptableRendererFeature
        {
            result = null;
            Error();
            return false;
        }
#endif

        static void Error()
        {
            Debug.LogError("RenderPipelineBridge default implementation has not been overriden.");
        }
    }
}
