#if HDRP_14_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.Assertions;

namespace Unity.LiveCapture.Cameras.Rendering
{
    /// <summary>
    /// Lens Distortion post process implementing the Brown–Conrady distortion model.
    /// </summary>
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Lens Distortion Brown-Conrady")]
    sealed public class LensDistortionBrownConrady : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        static class ShaderIDs
        {
            public static readonly int _InputTexture = Shader.PropertyToID("_InputTexture");
            public static readonly int _RadialLensDistParams = Shader.PropertyToID("_RadialLensDistParams");
            public static readonly int _TangentialLensDistParams = Shader.PropertyToID("_TangentialLensDistParams");
            public static readonly int _PrincipalPoint = Shader.PropertyToID("_PrincipalPoint");            
            public static readonly int _DistortedFocalLength = Shader.PropertyToID("_DistortedFocalLength");
            public static readonly int _UndistortedFocalLength = Shader.PropertyToID("_UndistortedFocalLength");
            public static readonly int _OutOfViewportColor = Shader.PropertyToID("_OutOfViewportColor");

            // Grid Visualization.
            public static readonly int _GridColor = Shader.PropertyToID("_GridColor");
            public static readonly int _GridResolution = Shader.PropertyToID("_GridResolution");
            public static readonly int _GridLineWidth = Shader.PropertyToID("_GridLineWidth");
        }

        static readonly Vector2 k_DefaultFieldOfView = new Vector2(45f, 45f);
        const float k_DefaultDistortionScale = 1f;

        [Tooltip("The horizontal and vertical field of view of the Camera, in degrees.")]
        public Vector2Parameter FieldOfView = new(k_DefaultFieldOfView);

        [Tooltip("The ratio between the distorted and undistorted focal lengths.")]
        public FloatParameter DistortionScale = new(k_DefaultDistortionScale);

        [Tooltip("Radial Distortion Coefficients.")]
        public Vector3Parameter RadialDistortionCoefficients = new (Vector3.zero);

        [Tooltip("Tangential Distortion Coefficients.")]
        public Vector2Parameter TangentialDistortionCoefficients = new (Vector2.zero);

        [Tooltip("Principal point normalized, expressed as an offset from the view center.")]
        public Vector2Parameter PrincipalPoint = new (Vector2.zero);

        [Tooltip("Color of the pixels lying outside of the viewport.")]
        public ColorParameter OutOfViewportColor = new (Color.black);

        [Tooltip("Show a grid to visualize lens distortion.")]
        public BoolParameter ShowGrid = new (false);

        [Tooltip("Color of the distortion visualization grid.")]
        public ColorParameter GridColor = new (Color.yellow);

        [Tooltip("Resolution of the distortion visualization grid.")]
        public FloatParameter GridResolution = new (12);

        [Tooltip("Thickness of the distortion visualization grid.")]
        public FloatParameter GridLineWidth = new (6);

        Material m_Material;
        
        public bool IsActive() => m_Material != null;

        /// <inheritdoc/>
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        /// <inheritdoc/>
        public override void Setup()
        {
            var shader = Shader.Find("Hidden/Hdrp/LensDistortionBrownConrady");
            Assert.IsNotNull(shader);
            m_Material = new Material(shader);
        }

        /// <inheritdoc/>
        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null)
                return;

            if (camera.camera.cameraType == CameraType.SceneView)
            {
                HDUtils.BlitCameraTexture(cmd, source, destination);
                return;
            }

            var distortedFocalLength = new Vector2(
                Camera.FieldOfViewToFocalLength(FieldOfView.value.x, 1f),
                Camera.FieldOfViewToFocalLength(FieldOfView.value.y, 1f));
            var undistortedFocalLength = DistortionScale.value * distortedFocalLength;
            var principalPoint = new Vector2(
                0.5f - PrincipalPoint.value.x,
                0.5f + PrincipalPoint.value.y);

            m_Material.SetVector(ShaderIDs._RadialLensDistParams, RadialDistortionCoefficients.value);
            m_Material.SetVector(ShaderIDs._TangentialLensDistParams, TangentialDistortionCoefficients.value);
            m_Material.SetVector(ShaderIDs._PrincipalPoint, principalPoint);
            m_Material.SetVector(ShaderIDs._UndistortedFocalLength, undistortedFocalLength);
            m_Material.SetVector(ShaderIDs._DistortedFocalLength, distortedFocalLength);
            m_Material.SetVector(ShaderIDs._OutOfViewportColor, OutOfViewportColor.value);
            
            if (ShowGrid.value)
            {
                m_Material.SetColor(ShaderIDs._GridColor, GridColor.value);
                m_Material.SetFloat(ShaderIDs._GridResolution, GridResolution.value);
                m_Material.SetFloat(ShaderIDs._GridLineWidth, GridLineWidth.value);
            }

            m_Material.SetTexture(ShaderIDs._InputTexture, source);

            var pass = ShowGrid.value ? 1 : 0;
            HDUtils.DrawFullScreen(cmd, m_Material, destination, null, pass);
        }

        /// <inheritdoc/>
        public override void Cleanup() => CoreUtils.Destroy(m_Material);
    }
}
#endif
