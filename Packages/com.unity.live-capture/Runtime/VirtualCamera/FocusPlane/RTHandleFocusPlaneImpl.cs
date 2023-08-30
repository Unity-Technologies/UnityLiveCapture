#if SRP_CORE_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VirtualCamera
{
    class RTHandleFocusPlaneImpl : IFocusPlaneImpl, IRenderTargetProvider<RTHandle>
    {
        RTHandle m_Target;
        Material m_ComposeMaterial;
        bool m_InitializedTarget;

        public RTHandleFocusPlaneImpl(Material composeMaterial)
        {
            m_ComposeMaterial = composeMaterial;
        }

        /// <inheritdoc/>
        public bool TryGetRenderTarget<T>(out T target)
        {
            if (this is IRenderTargetProvider<T> specialized)
            {
                return specialized.TryGetRenderTarget(out target);
            }

            target = default(T);
            return false;
        }

        /// <inheritdoc/>
        bool IRenderTargetProvider<RTHandle>.TryGetRenderTarget(out RTHandle target)
        {
            target = m_Target;
            return true;
        }

        /// <inheritdoc/>
        public virtual void Initialize()
        {
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            m_ComposeMaterial = null;

            if (m_InitializedTarget)
            {
                m_Target.Release();
                m_InitializedTarget = false;
            }
        }

        /// <inheritdoc/>
        public bool AllocateTargetIfNeeded(int width, int height)
        {
            if (!m_InitializedTarget)
            {
                m_Target = RTHandles.Alloc(Vector2.one, 1,
                    dimension: TextureDimension.Tex2D,
                    colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                    useDynamicScale: true,
                    name: "Focus Plane Buffer");

                m_InitializedTarget = true;

                m_ComposeMaterial.SetTexture(FocusPlaneConsts.InputTextureProperty, m_Target);

                return true;
            }

            return false;
        }

        // These 2 methods are only used with the Legacy Render Pipeline so far.

        /// <inheritdoc/>
        public void SetCamera(Camera camera) {}

        /// <inheritdoc/>
        public void Update()
        {
#if UNITY_EDITOR
            // In the Editor, the Asset Database has a tendency to stomp over our
            // material bindings (uniforms, textures, etc.). As a slightly inefficient workaround,
            // let's just rebind on every Update.
            if (m_InitializedTarget)
            {
                m_ComposeMaterial.SetTexture(FocusPlaneConsts.InputTextureProperty, m_Target);
            }
#endif
        }
    }
}
#endif
