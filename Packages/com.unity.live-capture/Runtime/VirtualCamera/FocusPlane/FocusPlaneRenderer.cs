using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;

#endif

#if URP_14_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Map between Camera and FocusPlane components.
    /// </summary>
    class FocusPlaneMap : ComponentMap<Camera, FocusPlaneRenderer> { }

    /// <summary>
    /// A component that provides focus plane visualization.
    /// </summary>
    /// <remarks>
    /// Each instance is tied to a specific camera.
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Live Capture/Virtual Camera/Focus Plane Renderer")]
    [HelpURL(Documentation.baseURL + "ref-component-focus-plane-renderer" + Documentation.endURL)]
    public class FocusPlaneRenderer : MonoBehaviour
    {
        class NullImpl : IFocusPlaneImpl
        {
            public void SetCamera(Camera camera) { }

            public void Initialize() { }

            public void Dispose() { }

            public void Update() { }

            public bool TryGetRenderTarget<T>(out T target)
            {
                target = default;
                return false;
            }

            public bool AllocateTargetIfNeeded(int width, int height)
            {
                return false;
            }
        }

        internal const string k_RenderShaderPath = "Hidden/LiveCapture/FocusPlane/Render";
        internal const string k_ComposeShaderPath = "Hidden/LiveCapture/FocusPlane/Compose";

        [SerializeField, Tooltip("The camera to use to render the focus plane.")]
        Camera m_Camera;
        [SerializeField]
        FocusPlaneSettings m_Settings = FocusPlaneSettings.GetDefault();

        Material m_RenderMaterial;
        Material m_ComposeMaterial;
        FocusPlaneSettings m_CachedSettings;

#pragma warning disable 649
        IFocusPlaneImpl m_Impl;
#pragma warning restore 649

        /// <summary>
        /// The material used to render the focus plane.
        /// </summary>
        internal Material RenderMaterial => m_RenderMaterial;

        /// <summary>
        /// The material used to blend the rasterized focus plane with the final frame.
        /// </summary>
        internal Material ComposeMaterial => m_ComposeMaterial;

        /// <inheritdoc cref="IFocusPlaneImpl.TryGetRenderTarget{T}" />
        internal bool TryGetRenderTarget<T>(out T target)
        {
            return m_Impl.TryGetRenderTarget(out target);
        }

        /// <summary>
        /// Sets the camera to use to render the focus plane.
        /// </summary>
        /// <param name="camera">The camera to set.</param>
        public void SetCamera(Camera camera)
        {
            if (m_Camera != camera)
            {
                m_Camera = camera;
                if (m_Impl != null)
                {
                    m_Impl.SetCamera(m_Camera);
                }

                Validate();
            }
        }

        /// <summary>
        /// Sets the focus distance.
        /// </summary>
        /// <param name="value">The focus distance</param>
        public void SetFocusDistance(float value)
        {
            var settings = m_Settings;
            settings.CameraDepthThreshold = value;
            m_Settings = settings;
        }

        void Awake()
        {
            m_Camera = GetComponent<Camera>();
        }

        void OnDestroy()
        {
            FocusPlaneMap.Instance.RemoveInstance(this);
        }

        void OnEnable()
        {
            m_CachedSettings = m_Settings;

            // Make sure settings and its cached version are different,
            // so that material properties get updated right away.
            m_CachedSettings.BackgroundOpacity += 1;

            m_RenderMaterial = AdditionalCoreUtils.CreateEngineMaterial(k_RenderShaderPath);
            m_ComposeMaterial = AdditionalCoreUtils.CreateEngineMaterial(k_ComposeShaderPath);

            // Shaders require Unity 2020.3.16f1 or newer.
            if (m_RenderMaterial == null || m_ComposeMaterial == null)
            {
                Debug.LogError(
                    $"Could not create {nameof(FocusPlaneRenderer)} materials." +
                    " Make sure that you are using Unity 2020.3.16f1 or higher.");

                m_Impl = new NullImpl();
                return;
            }

#if HDRP_14_0_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset is HDRenderPipelineAsset)
            {
                m_Impl = new HdrpFocusPlaneImpl(m_ComposeMaterial);
            }
#endif

#if URP_14_0_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset)
            {
                m_Impl = new UrpFocusPlaneImpl(m_ComposeMaterial);
            }
#endif

            if (m_Impl == null)
            {
                Assert.IsNull(GraphicsSettings.renderPipelineAsset,
                    $"{nameof(FocusPlaneRenderer)}: no SRP implementation, yet cannot default to legacy render pipeline.");

                m_Impl = new LegacyFocusPlaneImpl(m_RenderMaterial, m_ComposeMaterial);
            }

            m_Impl.Initialize();
            m_Impl.SetCamera(m_Camera);
        }

        void OnDisable()
        {
            m_Impl.Dispose();
            m_Impl = null;

            AdditionalCoreUtils.DestroyIfNeeded(ref m_RenderMaterial);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_ComposeMaterial);
        }

        void OnValidate()
        {
            Validate();
        }

        void Update()
        {
#if UNITY_EDITOR
            // In the Editor, the Asset Database has a tendency to stomp over our
            // material bindings (uniforms, textures, etc.). As a slightly inefficient workaround,
            // let's just re-send the uniform values on every Update.
            if (m_RenderMaterial != null)
            {
                m_Settings.Apply(m_RenderMaterial);
            }
#else
            if (m_Settings != m_CachedSettings)
            {
                m_CachedSettings = m_Settings;

                if (m_RenderMaterial != null)
                {
                    m_Settings.Apply(m_RenderMaterial);
                }
            }
#endif
            m_Impl.Update();
        }

        void Validate()
        {
            FocusPlaneMap.Instance.RemoveInstance(this);

            if (m_Camera != null)
            {
                FocusPlaneMap.Instance.AddUniqueInstance(m_Camera, this);
            }
        }

        /// <inheritdoc cref="IFocusPlaneImpl.AllocateTargetIfNeeded" />
        internal void AllocateTargetIfNeeded(int width, int height)
        {
            m_Impl.AllocateTargetIfNeeded(width, height);
        }
    }
}
