using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Contains the minimum and maximum values for each camera lens parameter.
    /// </summary>
    static class LensLimits
    {
        /// <summary>
        /// The focal length range limits.
        /// </summary>
        public static readonly Vector2 FocalLength = new Vector2(4f, 1200f);

        /// <summary>
        /// The focus distance range limits.
        /// </summary>
        /// <remarks>
        /// The maximum is a large number that represents infinity.
        /// </remarks>
        public static readonly Vector2 FocusDistance = new Vector2(0.01f, 1e6f);

        /// <summary>
        /// The minimal focus distance range width.
        /// </summary>
        public static readonly float MinFocusDistanceRange = 1e-2f;

        /// <summary>
        /// The aperture range limits.
        /// </summary>
        public static readonly Vector2 Aperture = new Vector2(1f, 32f);

        /// <summary>
        /// The blade count range limits.
        /// </summary>
        public static readonly Vector2Int BladeCount = new Vector2Int(3, 11);

        /// <summary>
        /// The curvature range limits.
        /// </summary>
        public static readonly Vector2 Curvature = new Vector2(1f, 32f);

        /// <summary>
        /// The barrel clipping range limits.
        /// </summary>
        public static readonly Vector2 BarrelClipping = new Vector2(0f, 1f);

        /// <summary>
        /// The anamorphism range limits.
        /// </summary>
        public static readonly Vector2 Anamorphism = new Vector2(-1f, 1f);
    }
}
