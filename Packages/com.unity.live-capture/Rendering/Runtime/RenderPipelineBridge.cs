using UnityEngine;
#if URP_14_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.LiveCapture.Rendering
{
    /// <summary>
    /// Provides access to a set of render-pipeline related editor-side features.
    /// </summary>
    /// <remarks>Primarily introduced so that runtime code can request render features directly.</remarks>
    static class RenderPipelineBridge
    {
        static IRenderPipelineBridge s_Impl = RenderPipelineBridgeDefaultImpl.s_Instance;

        internal static void SetImplementation(IRenderPipelineBridge impl)
        {
            if (s_Impl != RenderPipelineBridgeDefaultImpl.s_Instance && s_Impl != impl)
                Debug.LogWarning($"Overriding non-default implementation of {nameof(RenderPipelineBridge)}, override is expected to occur once.");

            s_Impl = impl;
        }

#if URP_14_0_OR_NEWER
        /// <inheritdoc cref="IRenderPipelineBridge.RequestRenderFeature{T}"/>
        public static T RequestRenderFeature<T>() where T : ScriptableRendererFeature
        {
            return s_Impl.RequestRenderFeature<T>();
        }

        /// <inheritdoc cref="IRenderPipelineBridge.HasRenderFeature{T}"/>
        public static bool HasRenderFeature<T>(out T result) where T : ScriptableRendererFeature
        {
            return s_Impl.HasRenderFeature<T>(out result);
        }

#endif
    }
}
