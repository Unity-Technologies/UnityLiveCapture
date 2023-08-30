using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Contains all the parameters needed to model a physical camera lens.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LensIntrinsics : IEquatable<LensIntrinsics>
    {
        const int k_DefaultBladeCount = 11;
        const float k_DefaultMinCurvature = 1f;
        const float k_DefaultMaxCurvature = 30f;
        const float k_DefaultBarrelClipping = 0f;
        const float k_DefaultAnamorphism = 0f;
        const float k_DefaultCloseFocusDistance = 0.3f;
        static readonly Vector2 k_DefaultFocalLengthRange = new Vector2(18f, 75f);
        static readonly Vector2 k_DefaultApertureRange = new Vector2(1f, 22f);

        /// <summary>
        /// The default Lens.
        /// </summary>
        public static readonly LensIntrinsics DefaultParams = new LensIntrinsics
        {
            FocalLengthRange = k_DefaultFocalLengthRange,
            CloseFocusDistance = k_DefaultCloseFocusDistance,
            ApertureRange = k_DefaultApertureRange,
            LensShift = Vector2.zero,
            BladeCount = k_DefaultBladeCount,
            Curvature = new Vector2(k_DefaultMinCurvature, k_DefaultMaxCurvature),
            BarrelClipping = k_DefaultBarrelClipping,
            Anamorphism = k_DefaultAnamorphism,
        };

        [SerializeField]
        Vector2 m_FocalLengthRange;

        [SerializeField]
        float m_CloseFocusDistance;

        [SerializeField]
        Vector2 m_ApertureRange;

        [SerializeField]
        Vector2 m_LensShift;

        [SerializeField]
        int m_BladeCount;

        [SerializeField]
        Vector2 m_Curvature;

        [SerializeField]
        float m_BarrelClipping;

        [SerializeField]
        float m_Anamorphism;

        /// <summary>
        /// Range of the focal length of the lens.
        /// </summary>
        public Vector2 FocalLengthRange
        {
            get => m_FocalLengthRange;
            set => m_FocalLengthRange = value;
        }

        /// <summary>
        /// The lens close focus distance.
        /// </summary>
        /// <remarks>
        /// The close focus distance represents the minimal focus distance supported by the lens.
        /// </remarks>>
        public float CloseFocusDistance
        {
            get => m_CloseFocusDistance;
            set => m_CloseFocusDistance = value;
        }

        /// <summary>
        /// Range of the aperture of the lens.
        /// </summary>
        public Vector2 ApertureRange
        {
            get => m_ApertureRange;
            set => m_ApertureRange = value;
        }

        /// <summary>
        /// The horizontal and vertical shift from the center.
        /// </summary>
        /// <remarks>
        /// Values are multiples of the sensor size; for example,
        /// a shift of 0.5 along the X axis offsets the sensor by half its horizontal size. You can use lens shifts to
        /// correct distortion that occurs when the camera is at an angle to the subject (for example, converging parallel
        /// lines). Shift the lens along either axis to make the camera frustum oblique.
        /// </remarks>
        public Vector2 LensShift
        {
            get => m_LensShift;
            set => m_LensShift = value;
        }

        /// <summary>
        /// Number of diaphragm blades the Camera uses to form the aperture.
        /// </summary>
        public int BladeCount
        {
            get => m_BladeCount;
            set => m_BladeCount = value;
        }

        /// <summary>
        /// Maps an aperture range to blade curvature.
        /// </summary>
        /// <remarks>
        /// Aperture blades become more visible on bokeh at higher aperture
        /// values. Tweak this range to define how the bokeh looks at a given aperture. The minimum value results in
        /// fully-curved, perfectly-circular bokeh, and the maximum value results in fully-shaped bokeh with visible
        /// aperture blades.
        /// </remarks>
        public Vector2 Curvature
        {
            get => m_Curvature;
            set => m_Curvature = value;
        }

        /// <summary>
        /// The strength of the “cat eye” effect. You can see this effect on bokeh as a result of lens shadowing
        /// (distortion along the edges of the frame).
        /// </summary>
        public float BarrelClipping
        {
            get => m_BarrelClipping;
            set => m_BarrelClipping = value;
        }

        /// <summary>
        /// Stretch the sensor to simulate an anamorphic look. Positive values distort the Camera vertically, negative
        /// will distort the Camera horizontally.
        /// </summary>
        public float Anamorphism
        {
            get => m_Anamorphism;
            set => m_Anamorphism = value;
        }

        /// <summary>
        /// Determines whether the specified LensIntrinsics is equal to the current LensIntrinsics.
        /// </summary>
        /// <param name="other">The LensIntrinsics to compare with the current LensIntrinsics.</param>
        /// <returns>
        /// true if the specified LensIntrinsics is equal to the current LensIntrinsics; otherwise, false.
        /// </returns>
        public bool Equals(LensIntrinsics other)
        {
            return FocalLengthRange == other.FocalLengthRange
                && CloseFocusDistance == other.CloseFocusDistance
                && ApertureRange == other.ApertureRange
                && LensShift == other.LensShift
                && BladeCount == other.BladeCount
                && Curvature == other.Curvature
                && BarrelClipping == other.BarrelClipping
                && Anamorphism == other.Anamorphism;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current LensIntrinsics.
        /// </summary>
        /// <param name="obj"> The object to compare with the current LensIntrinsics.</param>
        /// <returns>
        /// true if the specified object is equal to the current LensIntrinsics; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is LensIntrinsics other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the LensIntrinsics.
        /// </summary>
        /// <returns>
        /// The hash value generated for this LensIntrinsics.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FocalLengthRange.GetHashCode();
                hashCode = (hashCode * 397) ^ CloseFocusDistance.GetHashCode();
                hashCode = (hashCode * 397) ^ ApertureRange.GetHashCode();
                hashCode = (hashCode * 397) ^ LensShift.GetHashCode();
                hashCode = (hashCode * 397) ^ BladeCount.GetHashCode();
                hashCode = (hashCode * 397) ^ Curvature.GetHashCode();
                hashCode = (hashCode * 397) ^ BarrelClipping.GetHashCode();
                hashCode = (hashCode * 397) ^ Anamorphism.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified LensIntrinsics are equal.
        /// </summary>
        /// <param name="a">The first LensIntrinsics.</param>
        /// <param name="b">The second LensIntrinsics.</param>
        /// <returns>
        /// true if the specified LensIntrinsics are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(LensIntrinsics a, LensIntrinsics b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified LensIntrinsics are different.
        /// </summary>
        /// <param name="a">The first LensIntrinsics.</param>
        /// <param name="b">The second LensIntrinsics.</param>
        /// <returns>
        /// true if the specified LensIntrinsics are different; otherwise, false.
        /// </returns>
        public static bool operator !=(LensIntrinsics a, LensIntrinsics b)
        {
            return !(a == b);
        }
    }
}
