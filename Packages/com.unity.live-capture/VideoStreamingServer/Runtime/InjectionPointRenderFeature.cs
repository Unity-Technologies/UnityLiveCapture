#if URP_14_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A render pass whose purpose is to invoke an event at the desired stage of rendering.
    /// </summary>
    public class InjectionPointRenderPass : ScriptableRenderPass
    {
        public static event Action<ScriptableRenderContext, RenderingData> onExecute = delegate {};

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            onExecute.Invoke(context, renderingData);
        }
    }

    /// <summary>
    /// A render feature whose purpose is to provide an event invoked at a given rendering stage.
    /// Meant to abstract away the render feature mechanism and allow for simple graphics code injection.
    /// </summary>
    public class InjectionPointRenderFeature : ScriptableRendererFeature
    {
        InjectionPointRenderPass m_Pass;

        public override void Create()
        {
            m_Pass = new InjectionPointRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_Pass);
        }
    }
}
#endif
