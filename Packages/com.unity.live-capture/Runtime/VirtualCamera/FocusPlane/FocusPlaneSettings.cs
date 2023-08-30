using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.LiveCapture.VirtualCamera
{
    static class ShaderIDs
    {
        public static readonly int _CameraDepthThreshold = Shader.PropertyToID("_CameraDepthThreshold");
        public static readonly int _Color = Shader.PropertyToID("_Color");
        public static readonly int _CellSize = Shader.PropertyToID("_CellSize");
        public static readonly int _IntersectionLineWidth = Shader.PropertyToID("_IntersectionLineWidth");
        public static readonly int _BackgroundOpacity = Shader.PropertyToID("_BackgroundOpacity");
        public static readonly int _GridOpacity = Shader.PropertyToID("_GridOpacity");
    }

    /// <summary>
    /// A struct that holds the focus plane rendering settings.
    /// </summary>
    [Serializable]
    struct FocusPlaneSettings : IEquatable<FocusPlaneSettings>
    {
        const string k_UseGridKeyword = "USE_GRID";
        const int k_GridResolution = 8;

        [Tooltip("The camera space depth at which the focus plane will be rendered.")]
        public float CameraDepthThreshold;
        [Tooltip("Color of the focus plane.")]
        public Color Color;
        [Tooltip("Width of the intersection of the scene geometry and the focus plane."), Range(0, 1)]
        public float IntersectionLineWidth;
        [Tooltip("Opacity of the focus plane's background."), Range(0, 1)]
        public float BackgroundOpacity;
        [FormerlySerializedAs("ShowGrid")]
        [Tooltip("Show the grid overlay.")]
        public bool Grid;
        [Tooltip("Opacity of the focus plane's grid overlay."), Range(0, 1)]
        public float GridOpacity;

        /// <summary>
        /// Configures material uniforms based on rendering settings.
        /// </summary>
        /// <param name="material">The material used to render the focus plane.</param>
        public void Apply(Material material)
        {
            material.SetFloat(ShaderIDs._CameraDepthThreshold, CameraDepthThreshold);
            material.SetColor(ShaderIDs._Color, Color);
            material.SetFloat(ShaderIDs._IntersectionLineWidth, IntersectionLineWidth);
            material.SetFloat(ShaderIDs._GridOpacity, GridOpacity);
            material.SetFloat(ShaderIDs._BackgroundOpacity, BackgroundOpacity);

            var isGridVisible = material.IsKeywordEnabled(k_UseGridKeyword);
            if (isGridVisible != Grid)
            {
                if (Grid)
                {
                    material.EnableKeyword(k_UseGridKeyword);
                }
                else
                {
                    material.DisableKeyword(k_UseGridKeyword);
                }
            }

            // Grid cell size is affected by camera depth.
            var cellSize = 1 + Mathf.Max(0, CameraDepthThreshold);
            var n = Mathf.Floor(Mathf.Log(cellSize, 2));
            cellSize = k_GridResolution * cellSize / Mathf.Pow(2, n);
            material.SetFloat(ShaderIDs._CellSize, cellSize);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CameraDepthThreshold.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ IntersectionLineWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ GridOpacity.GetHashCode();
                hashCode = (hashCode * 397) ^ BackgroundOpacity.GetHashCode();
                hashCode = (hashCode * 397) ^ Grid.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(FocusPlaneSettings other)
        {
            return
                CameraDepthThreshold == other.CameraDepthThreshold &&
                Color == other.Color &&
                IntersectionLineWidth == other.IntersectionLineWidth &&
                GridOpacity == other.GridOpacity &&
                BackgroundOpacity == other.BackgroundOpacity &&
                Grid == other.Grid;
        }

        public override bool Equals(object obj)
        {
            return obj is Lens other && Equals(other);
        }

        public static bool operator ==(FocusPlaneSettings lhs, FocusPlaneSettings rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FocusPlaneSettings lhs, FocusPlaneSettings rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns default rendering settings.
        /// </summary>
        /// <remarks>
        /// Provides users with arbitrary default settings for rendering the focus plane.
        /// </remarks>
        public static FocusPlaneSettings GetDefault()
        {
            return new FocusPlaneSettings
            {
                CameraDepthThreshold = 5,
                Color = new Color(0.269f, 0.546f, 1, 1),
                BackgroundOpacity = 0.1f,
                GridOpacity = 0.5f,
                IntersectionLineWidth = 0.1f
            };
        }
    }
}
