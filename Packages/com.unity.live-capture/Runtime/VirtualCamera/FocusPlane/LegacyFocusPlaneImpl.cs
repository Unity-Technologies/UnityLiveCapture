using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VirtualCamera
{
    class LegacyFocusPlaneImpl : IFocusPlaneImpl, IRenderTargetProvider<RenderTexture>
    {
        Camera m_Camera;
        CommandBuffer m_ComposeCommandBuffer;
        CommandBuffer m_RenderCommandBuffer;
        RenderTexture m_Target;
        Material m_RenderMaterial;
        Material m_ComposeMaterial;
        bool m_AddedCommandBuffers;

        public LegacyFocusPlaneImpl(Material renderMaterial, Material composeMaterial)
        {
            m_RenderMaterial = renderMaterial;
            m_ComposeMaterial = composeMaterial;
        }

        /// <inheritdoc/>
        public void SetCamera(Camera camera)
        {
            RemoveCommandBufferIfNeeded(false);
            m_Camera = camera;
            AddCommandBufferIfNeeded();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            m_RenderCommandBuffer = new CommandBuffer();
            m_RenderCommandBuffer.name = "FocusPlane Render";
            m_ComposeCommandBuffer = new CommandBuffer();
            m_ComposeCommandBuffer.name = "FocusPlane Compose";

            Update();

            AddCommandBufferIfNeeded();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RemoveCommandBufferIfNeeded(true);

            m_RenderMaterial = null;
            m_ComposeMaterial = null;

            m_RenderCommandBuffer.Dispose();
            m_ComposeCommandBuffer.Dispose();
            m_RenderCommandBuffer = null;
            m_ComposeCommandBuffer = null;

            if (m_Target != null)
            {
                m_Target.Release();
                m_Target = null;
            }
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (m_Camera == null)
            {
                return;
            }

            if (AllocateTargetIfNeeded(m_Camera.pixelWidth, m_Camera.pixelHeight))
            {
                UpdateCommandBuffers();
            }
        }

        /// <inheritdoc/>
        public bool TryGetRenderTarget<T>(out T target)
        {
            if (this is IRenderTargetProvider<T> specialized)
            {
                return specialized.TryGetRenderTarget(out target);
            }

            target = default;
            return false;
        }

        /// <inheritdoc/>
        bool IRenderTargetProvider<RenderTexture>.TryGetRenderTarget(out RenderTexture target)
        {
            target = m_Target;
            return true;
        }

        /// <inheritdoc/>
        public bool AllocateTargetIfNeeded(int width, int height)
        {
            if (m_Target == null ||
                m_Target.width != width ||
                m_Target.height != height)
            {
                if (m_Target != null)
                {
                    m_Target.Release();
                }

                m_Target = new RenderTexture(width, height, 0, RenderTextureFormat.BGRA32)
                {
                    useMipMap = false,
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.HideAndDontSave,
                };

                return true;
            }

            return false;
        }

        void AddCommandBufferIfNeeded()
        {
            Assert.IsFalse(m_AddedCommandBuffers);

            if (m_Camera != null && m_RenderCommandBuffer != null)
            {
                Assert.IsNotNull(m_ComposeCommandBuffer);
                m_Camera.depthTextureMode |= DepthTextureMode.Depth;
                m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_RenderCommandBuffer);
                m_Camera.AddCommandBuffer(CameraEvent.AfterImageEffects, m_ComposeCommandBuffer);
                m_AddedCommandBuffers = true;
            }
        }

        void RemoveCommandBufferIfNeeded(bool disposing)
        {
            if (m_AddedCommandBuffers)
            {
                Assert.IsNotNull(m_RenderCommandBuffer, $"{nameof(m_RenderCommandBuffer)} disposed before being removed from the camera.");
                Assert.IsNotNull(m_ComposeCommandBuffer, $"{nameof(m_ComposeCommandBuffer)} disposed before being removed from the camera.");

                if (m_Camera == null)
                {
                    if (disposing)
                    {
                        m_AddedCommandBuffers = false;
                        return;
                    }

                    throw new InvalidOperationException($"{nameof(m_Camera)} disposed before command buffers were removed.");
                }

                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_RenderCommandBuffer);
                m_Camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, m_ComposeCommandBuffer);
                m_AddedCommandBuffers = false;
            }
        }

        void UpdateCommandBuffers()
        {
            m_RenderCommandBuffer.Clear();
            m_RenderCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, m_Target, m_RenderMaterial);

            m_ComposeCommandBuffer.Clear();
            m_ComposeCommandBuffer.Blit(m_Target, BuiltinRenderTextureType.CameraTarget, m_ComposeMaterial);
        }
    }
}
