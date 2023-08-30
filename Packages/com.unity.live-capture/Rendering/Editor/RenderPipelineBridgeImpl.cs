using UnityEditor;
using UnityEngine;

#if URP_14_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.LiveCapture.Rendering.Editor
{
    class RenderPipelineBridgeImpl : IRenderPipelineBridge
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            RenderPipelineBridge.SetImplementation(new RenderPipelineBridgeImpl());
        }

#if URP_14_0_OR_NEWER
        /// <inheritdoc/>
        public T RequestRenderFeature<T>() where T : ScriptableRendererFeature
        {
            if (URPUtility.HasRenderFeature<T>(out var feature))
                return feature;

            return URPUtility.AddRenderFeature<T>();
        }

        /// <inheritdoc/>
        public bool HasRenderFeature<T>(out T result) where T : ScriptableRendererFeature
        {
            return URPUtility.HasRenderFeature<T>(out result);
        }

#endif
    }
}
