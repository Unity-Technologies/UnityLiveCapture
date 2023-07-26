#if URP_14_0_OR_NEWER || HDRP_14_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.Rendering.RendererUtils;
#else
using UnityEngine.Experimental.Rendering;
#endif

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    /// <summary>
    /// A base class holding shared functionalities for SRP compatible graphics raycasters.
    /// </summary>
    class BaseScriptableRenderPipelineRaycasterImpl : BaseRaycasterImpl
    {
        // Object picking is regular graphics raycasting with extra steps.
        // This temporary object implements these extra steps, namely:
        // - Before raycasting: assign object ids to renderers, and build a map of ids to objects.
        // - After raycasting: clear the map.

        static readonly ShaderTagId[] k_ShaderTagsDepthOnly = { new ShaderTagId("DepthOnly") };

        ShaderTagId[] m_ShaderTagsObjectId;

        protected override void Render()
        {
            m_Camera.Render();
        }

        // Lazily instantiated since it will be invoked rarely if at all.
        ShaderTagId[] GetShaderTagsObjectId()
        {
            // During the renderer collection process,
            // renderers are filtered based on the tagged passes in their materials,
            // so we aim at having a shaderTag collection covering SRPs built-in materials.
            // If an object cannot be picked,
            // chances are it's because its material does not have a pass whose tag is included here.
            // If the material is an SRP provided one, the tag should be added here,
            // if it's a custom material, it should be tagged properly.
            if (m_ShaderTagsObjectId == null)
                m_ShaderTagsObjectId = new[]
                {
                    new ShaderTagId("Forward"),
                    new ShaderTagId("ForwardOnly"),
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("GBuffer"),
                    new ShaderTagId("ForwardLit"),
                    new ShaderTagId("Unlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId(GetPickingMaterial().GetPassName(0))
                };
            return m_ShaderTagsObjectId;
        }

        static void DrawRenderers(ScriptableRenderContext context, CommandBuffer cmd, Camera camera, ShaderTagId[] shaderTags, Material material = null, int pass = 0)
        {
            if (camera.TryGetCullingParameters(false, out var cullingParameters))
            {
                var cullingResults = context.Cull(ref cullingParameters);

                var rendererListDesc = new RendererListDesc(shaderTags, cullingResults, camera)
                {
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.all,
                    sortingCriteria = SortingCriteria.BackToFront,
                    excludeObjectMotionVectors = false,
                    layerMask = camera.cullingMask,
                    overrideMaterial = material,
                    overrideMaterialPassIndex = pass
                };

#if UNITY_2021_2_OR_NEWER
                var rendererList = context.CreateRendererList(rendererListDesc);
                if (rendererList.isValid)
                {
                    CoreUtils.DrawRendererList(context, cmd, rendererList);
                }
                else
                {
                    Debug.LogError("Invalid renderer list!", camera.gameObject);
                }
#else
                var rendererList = RendererList.Create(rendererListDesc);

                if (rendererList.isValid)
                {
                    if (rendererList.stateBlock == null)
                    {
                        context.DrawRenderers(rendererList.cullingResult, ref rendererList.drawSettings, ref rendererList.filteringSettings);
                    }
                    else
                    {
                        var renderStateBlock = rendererList.stateBlock.Value;
                        context.DrawRenderers(rendererList.cullingResult, ref rendererList.drawSettings, ref rendererList.filteringSettings, ref renderStateBlock);
                    }
                }
                else
                {
                    Debug.LogError("Invalid renderer list!", camera.gameObject);
                }
#endif
            }
        }

        protected void DoRender(CommandBuffer cmd, ScriptableRenderContext context, Camera camera)
        {
            var isPickingActive = PickingScope.IsActive(this);

            var colorTexture = isPickingActive ? m_ColorTexture : m_DepthAsColorTexture;
            cmd.SetRenderTarget(colorTexture.colorBuffer, m_DepthTexture.colorBuffer);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.SetViewport(new Rect(0, 0, 1, 1));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // Render scene.
            if (isPickingActive)
                DrawRenderers(context, cmd, camera, GetShaderTagsObjectId(), GetPickingMaterial(), 0);
            else
                DrawRenderers(context, cmd, camera, k_ShaderTagsDepthOnly);

            cmd.Blit(m_DepthTexture.colorBuffer, m_DepthAsColorTexture.colorBuffer);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}
#endif
