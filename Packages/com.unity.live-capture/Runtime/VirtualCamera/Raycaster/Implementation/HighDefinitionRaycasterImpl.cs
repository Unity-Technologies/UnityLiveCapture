#if HDRP_14_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    class HighDefinitionRaycasterImpl : BaseScriptableRenderPipelineRaycasterImpl
    {
        FrameSettings m_FrameSettings;

        public override void Initialize()
        {
            base.Initialize();

            // by labeling the camera as a preview camera it will render using animated materials
            // correctly in edit mode for HDRP 7.1.x
            m_Camera.cameraType = CameraType.Preview;

            var data = m_Raycaster.AddComponent<HDAdditionalCameraData>();

            // Avoid an HDRP 10.2.2 Volumetric System leak.
            data.customRenderingSettings = true;
            data.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReprojectionForVolumetrics, false);
            data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ReprojectionForVolumetrics] = true;

            data.customRender += Render;
        }

        public override void Dispose()
        {
            var data = m_Raycaster.GetComponent<HDAdditionalCameraData>();
            data.customRender -= Render;

            base.Dispose();
        }

        void Render(ScriptableRenderContext context, HDCamera camera)
        {
            context.SetupCameraProperties(m_Camera);

            var cmd = CommandBufferPool.Get("Graphics Raycast");

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            DoRender(cmd, context, m_Camera);

            CommandBufferPool.Release(cmd);
        }
    }
}
#endif
