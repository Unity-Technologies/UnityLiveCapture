using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    struct FrameLinesSettings : IEquatable<FrameLinesSettings>
    {
        public enum LineType
        {
            None,
            Box,
            Corner
        }

        public enum MarkerType
        {
            Cross,
            Dot
        }

        public GateFit GateFit;

        [FormerlySerializedAs("RenderGateMask")]
        public bool GateMaskEnabled;

        [FormerlySerializedAs("RenderAspectRatio")]
        public bool AspectRatioLinesEnabled;

        [FormerlySerializedAs("RenderCenterMarker")]
        public bool CenterMarkerEnabled;

        [Range(0, 1)]
        public float GateMaskOpacity;

        [AspectRatio]
        public float AspectRatio;

        [FormerlySerializedAs("AspectMode")]
        public LineType AspectLineType;

        public Color AspectLineColor;

        [Range(1, 10)]
        public float AspectLineWidth;

        [Range(0, 1)]
        public float AspectFillOpacity;

        public MarkerType CenterMarkerType;

        public static FrameLinesSettings GetDefault()
        {
            return new FrameLinesSettings
            {
                GateMaskEnabled = true,
                AspectRatioLinesEnabled = true,
                CenterMarkerEnabled = true,
                GateMaskOpacity = 1f,
                AspectRatio = AspectRatioConsts.k_DefaultAspectRatio,
                AspectLineType = LineType.Corner,
                AspectLineColor = Color.cyan,
                AspectLineWidth = 3,
                AspectFillOpacity = 0f,
                CenterMarkerType = MarkerType.Cross,
            };
        }

        public void Validate()
        {
            GateMaskOpacity = Mathf.Clamp01(GateMaskOpacity);
            AspectFillOpacity = Mathf.Clamp01(AspectFillOpacity);
            AspectLineWidth = Mathf.Clamp(AspectLineWidth, 1, 10);
            AspectRatio = Mathf.Max(AspectRatioConsts.k_MinAspectRatio, AspectRatio);
        }

        public bool Equals(FrameLinesSettings other)
        {
            return GateFit == other.GateFit
                && GateMaskEnabled == other.GateMaskEnabled
                && AspectRatioLinesEnabled == other.AspectRatioLinesEnabled
                && CenterMarkerEnabled == other.CenterMarkerEnabled
                && GateMaskOpacity == other.GateMaskOpacity
                && AspectRatio == other.AspectRatio
                && AspectLineType == other.AspectLineType
                && AspectLineColor == other.AspectLineColor
                && AspectLineWidth == other.AspectLineWidth
                && AspectFillOpacity == other.AspectFillOpacity
                && CenterMarkerType == other.CenterMarkerType;
        }

        public override bool Equals(object obj)
        {
            return obj is FrameLinesSettings other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the CameraState.
        /// </summary>
        /// <returns>
        /// The hash value generated for this CameraState.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = GateFit.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectRatioLinesEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ GateMaskEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ CenterMarkerEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ GateMaskOpacity.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectLineType.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectLineColor.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectLineWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectFillOpacity.GetHashCode();
                hashCode = (hashCode * 397) ^ CenterMarkerType.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(FrameLinesSettings a, FrameLinesSettings b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FrameLinesSettings a, FrameLinesSettings b)
        {
            return !(a == b);
        }
    }
}
