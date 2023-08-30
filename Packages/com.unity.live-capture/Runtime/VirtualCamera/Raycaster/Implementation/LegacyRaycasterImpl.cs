using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    class LegacyRaycasterImpl : BaseRaycasterImpl
    {
        CommandBuffer m_CommandBuffer;

        public override void Initialize()
        {
            base.Initialize();

            m_CommandBuffer = new CommandBuffer();
            m_CommandBuffer.Blit(m_DepthTexture.depthBuffer, m_DepthAsColorTexture.colorBuffer);

            m_Camera.depthTextureMode |= DepthTextureMode.Depth;
            m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, m_CommandBuffer);
        }

        public override void Dispose()
        {
            m_Camera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_CommandBuffer);
            m_CommandBuffer.Release();
            base.Dispose();
        }

        protected override void Render()
        {
            var isPickingActive = PickingScope.IsActive(this);

            var colorTexture = isPickingActive ? m_ColorTexture : m_DepthAsColorTexture;

            m_Camera.SetTargetBuffers(colorTexture.colorBuffer, m_DepthTexture.depthBuffer);
            m_Camera.rect = new Rect(0, 0, 1, 1);

            if (isPickingActive)
            {
                m_Camera.RenderWithShader(GetPickingShader(), String.Empty);
            }
            else
            {
                m_Camera.Render();
            }
        }
    }
}
