using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if HDRP_14_0_OR_NEWER
using LensDistortionVolumeComponent = Unity.LiveCapture.Cameras.Rendering.LensDistortionBrownConrady;
#endif

namespace Unity.LiveCapture.Cameras
{
    /// <summary>
    /// A component that manages the Lens Distortion effect.
    /// </summary>
    /// <remarks>
    /// This component uses the Brown-Conrady distortion model and is only available in HDRP.
    /// </remarks>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(SharedVolumeProfile))]
    [HelpURL(Documentation.baseURL + "ref-component-lens-distortion" + Documentation.endURL)]
    public class LensDistortionBrownConrady : MonoBehaviour
    {
        static readonly Vector2 k_MinMaxFoV = new Vector2(1f, 179);
        static readonly int k_ScaleBiasShaderProperty = Shader.PropertyToID("_ScaleAndBias");

        [SerializeField, Range(1f, 179f)]
        float m_FakeFieldOfView = 60f;
        [SerializeField]
        bool m_UseDistortionScale = true;
        [SerializeField]
        float m_DistortionScale = 1f;
        [SerializeField]
        Vector3 m_RadialCoefficients;
        [SerializeField]
        Vector2 m_TangentialCoefficients;
        Camera m_Camera;

        /// <summary>
        /// The field of view used for rendering.
        /// </summary>
        /// <remarks>
        /// If this value is greater than the camera's field of view, the image will be cropped.
        /// This can be used to eliminate edge artifacts created during the distortion without using
        /// a second camera.
        /// </remarks>
        public float FakeFieldOfView
        {
            get => m_FakeFieldOfView;
            set => m_FakeFieldOfView = Mathf.Clamp(value, k_MinMaxFoV.x, k_MinMaxFoV.y);
        }

        /// <summary>
        /// Whether to use the distortion scale.
        /// </summary>
        public bool UseDistortionScale
        {
            get => m_UseDistortionScale;
            set => m_UseDistortionScale = value;
        }

        /// <summary>
        /// The scale of the distortion effect.
        /// </summary>
        public float DistortionScale
        {
            get => m_DistortionScale;
            set => m_DistortionScale = Mathf.Max(0f, value);
        }

        /// <summary>
        /// The radial distortion coefficients.
        /// </summary>
        public Vector3 RadialCoefficients
        {
            get => m_RadialCoefficients;
            set => m_RadialCoefficients = value;
        }

        /// <summary>
        /// The tangential distortion coefficients.
        /// </summary>
        public Vector2 TangentialCoefficients
        {
            get => m_TangentialCoefficients;
            set => m_TangentialCoefficients = value;
        }

        SharedVolumeProfile m_SharedVolumeProfile;
        bool m_UsePhysicalProperties;

        void OnEnable()
        {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;

            m_Camera = GetComponent<Camera>();
            m_SharedVolumeProfile = GetComponent<SharedVolumeProfile>();
        }

        void OnDisable()
        {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;

            SetActive(false);
        }

        void OnDestroy()
        {
            if (m_SharedVolumeProfile == null)
            {
                return;
            }
#if HDRP_14_0_OR_NEWER
            m_SharedVolumeProfile.DestroyVolumeComponent<LensDistortionVolumeComponent>();
#endif
        }

        void OnValidate()
        {
            m_DistortionScale = Mathf.Max(0f, m_DistortionScale);
            m_FakeFieldOfView = Mathf.Clamp(m_FakeFieldOfView, 0f, k_MinMaxFoV.y);
        }

        void SetActive(bool value)
        {
#if HDRP_14_0_OR_NEWER
            {
                if (m_SharedVolumeProfile.TryGetVolumeComponent<LensDistortionVolumeComponent>(out var lensDistortion))
                {
                    lensDistortion.active = value;
                }
            }
#endif
        }

        void LateUpdate()
        {
#if HDRP_14_0_OR_NEWER
            var lensDistortion = m_SharedVolumeProfile.GetOrCreateVolumeComponent<LensDistortionVolumeComponent>();

            lensDistortion.active = true;

            var fieldOfView = m_Camera.GetGateFittedFieldOfView();
            var lendShift = m_Camera.GetGateFittedLensShift();

            if (!m_Camera.usePhysicalProperties)
            {
                lendShift = m_Camera.lensShift;
            }

            lensDistortion.FieldOfView.overrideState = true;
            lensDistortion.FieldOfView.value = new Vector2(
                Camera.VerticalToHorizontalFieldOfView(fieldOfView, m_Camera.aspect),
                fieldOfView);

            var distortionScale = m_UseDistortionScale ? m_DistortionScale : 1f;

            lensDistortion.DistortionScale.overrideState = true;
            lensDistortion.DistortionScale.value = distortionScale;

            lensDistortion.RadialDistortionCoefficients.overrideState = true;
            lensDistortion.RadialDistortionCoefficients.value = m_RadialCoefficients;

            lensDistortion.TangentialDistortionCoefficients.overrideState = true;
            lensDistortion.TangentialDistortionCoefficients.value = m_TangentialCoefficients;

            lensDistortion.PrincipalPoint.overrideState = true;
            lensDistortion.PrincipalPoint.value = lendShift;
#endif
        }

        void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            m_UsePhysicalProperties = m_Camera.usePhysicalProperties;

            var fov = m_Camera.GetGateFittedFieldOfView();
            var lensShift = m_Camera.GetGateFittedLensShift();

            if (!m_UsePhysicalProperties)
            {
                // Lens distortion needs lens shift regardless of the PhysicalCamera mode.
                // We store it in the component anyways and fallback into it when PhysicalCamera
                // mode is disabled.
                lensShift = m_Camera.lensShift;
            }

            var normalizedViewportSubsection = new Rect(lensShift.x, lensShift.y, 1f, 1f);
            var fovRatio = fov / m_Camera.fieldOfView;
            var fakeFov = m_FakeFieldOfView * fovRatio;

            fakeFov = Mathf.Max(fov, fakeFov);

            var fakeProjectionMatrix = Matrix4x4.Perspective(fakeFov, m_Camera.aspect, m_Camera.nearClipPlane, m_Camera.farClipPlane);
            var realProjectionMatrix = Matrix4x4.Perspective(fov, m_Camera.aspect, m_Camera.nearClipPlane, m_Camera.farClipPlane);
            // TODO: try replace SliceFrustum with a translation matrix
            var fakeFrustumPlanes = SliceFrustum(fakeProjectionMatrix.decomposeProjection, normalizedViewportSubsection);
            var realFrustumPlanes = SliceFrustum(realProjectionMatrix.decomposeProjection, normalizedViewportSubsection);
            var scaleAndBias = new Vector4(
                (realFrustumPlanes.right - realFrustumPlanes.left) / (fakeFrustumPlanes.right - fakeFrustumPlanes.left),
                (realFrustumPlanes.top - realFrustumPlanes.bottom) / (fakeFrustumPlanes.top - fakeFrustumPlanes.bottom),
                Mathf.InverseLerp(fakeFrustumPlanes.left, fakeFrustumPlanes.right, realFrustumPlanes.left),
                Mathf.InverseLerp(fakeFrustumPlanes.bottom, fakeFrustumPlanes.top, realFrustumPlanes.bottom));

            var projectionMatrix = Matrix4x4.Frustum(fakeFrustumPlanes);

            m_Camera.cullingMatrix = projectionMatrix * m_Camera.worldToCameraMatrix;
            m_Camera.projectionMatrix = projectionMatrix;

            // TODO Avoid global uniforms?
            Shader.SetGlobalVector(k_ScaleBiasShaderProperty, scaleAndBias);
        }

        void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            m_Camera.ResetAspect();
            m_Camera.ResetProjectionMatrix();
            m_Camera.ResetCullingMatrix();

            m_Camera.usePhysicalProperties = m_UsePhysicalProperties;

            Shader.SetGlobalVector(k_ScaleBiasShaderProperty, new Vector4(1f, 1f, 0f, 0f));
        }

        static FrustumPlanes SliceFrustum(FrustumPlanes frustumPlanes, Rect normalizedViewportSubsection)
        {
            return new FrustumPlanes
            {
                zNear = frustumPlanes.zNear,
                zFar = frustumPlanes.zFar,
                left = Mathf.LerpUnclamped(frustumPlanes.left, frustumPlanes.right, normalizedViewportSubsection.xMin),
                right = Mathf.LerpUnclamped(frustumPlanes.left, frustumPlanes.right, normalizedViewportSubsection.xMax),
                bottom = Mathf.LerpUnclamped(frustumPlanes.bottom, frustumPlanes.top, normalizedViewportSubsection.yMin),
                top = Mathf.LerpUnclamped(frustumPlanes.bottom, frustumPlanes.top, normalizedViewportSubsection.yMax)
            };
        }
    }
}
