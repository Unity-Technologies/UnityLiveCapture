using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    static class LensIntrinsicsExtensions
    {
        /// <summary>
        /// Validates each parameter by setting them into their valid range.
        /// </summary>
        public static void Validate(this ref LensIntrinsics parameters)
        {
            parameters.ValidateFocusDistanceRange();
            parameters.ValidateFocalLengthRange();
            parameters.ValidateApertureRange();
            parameters.ValidateBladeCount();
            parameters.ValidateCurvature();
            parameters.ValidateBarrelClipping();
            parameters.ValidateAnamorphism();
        }

        /// <summary>
        /// Validates the provided focal length value by setting it into its valid range.
        /// </summary>
        public static float ValidateFocalLength(this LensIntrinsics parameters, float focalLength)
        {
            return Mathf.Clamp(
                focalLength,
                parameters.FocalLengthRange.x,
                parameters.FocalLengthRange.y);
        }

        /// <summary>
        /// Validates the provided focus distance value by setting it into its valid range.
        /// </summary>
        public static float ValidateFocusDistance(this LensIntrinsics parameters, float focusDistance)
        {
            return Mathf.Clamp(
                focusDistance,
                parameters.CloseFocusDistance,
                LensLimits.FocusDistance.y);
        }

        /// <summary>
        /// Validates the provided aperture value by setting it into its valid range.
        /// </summary>
        public static float ValidateAperture(this LensIntrinsics parameters, float aperture)
        {
            return Mathf.Clamp(
                aperture,
                parameters.ApertureRange.x,
                parameters.ApertureRange.y);
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.FocalLengthRange"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateFocalLengthRange(this ref LensIntrinsics parameters)
        {
            var range = parameters.FocalLengthRange;

            range.x = Mathf.Clamp(
                range.x,
                LensLimits.FocalLength.x,
                LensLimits.FocalLength.y);

            range.y = Mathf.Clamp(
                range.y,
                LensLimits.FocalLength.x,
                LensLimits.FocalLength.y);

            range.x = Mathf.Min(range.x, range.y);
            range.y = Mathf.Max(range.x, range.y);

            parameters.FocalLengthRange = range;
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.CloseFocusDistance"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateFocusDistanceRange(this ref LensIntrinsics parameters)
        {
            parameters.CloseFocusDistance = Mathf.Clamp(
                parameters.CloseFocusDistance,
                LensLimits.FocusDistance.x,
                LensLimits.FocusDistance.y - LensLimits.MinFocusDistanceRange);
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.ApertureRange"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateApertureRange(this ref LensIntrinsics parameters)
        {
            var range = parameters.ApertureRange;

            range.x = Mathf.Clamp(
                range.x,
                LensLimits.Aperture.x,
                LensLimits.Aperture.y);

            range.y = Mathf.Clamp(
                range.y,
                LensLimits.Aperture.x,
                LensLimits.Aperture.y);

            range.x = Mathf.Min(range.x, range.y);
            range.y = Mathf.Max(range.x, range.y);

            parameters.ApertureRange = range;
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.BladeCount"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateBladeCount(this ref LensIntrinsics parameters)
        {
            parameters.BladeCount = Mathf.Clamp(
                parameters.BladeCount,
                LensLimits.BladeCount.x,
                LensLimits.BladeCount.y);
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.Curvature"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateCurvature(this ref LensIntrinsics parameters)
        {
            var curvature = parameters.Curvature;

            curvature.x = Mathf.Clamp(
                curvature.x,
                LensLimits.Curvature.x,
                LensLimits.Curvature.y);

            curvature.y = Mathf.Clamp(
                curvature.y,
                LensLimits.Curvature.x,
                LensLimits.Curvature.y);

            parameters.Curvature = curvature;
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.BarrelClipping"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateBarrelClipping(this ref LensIntrinsics parameters)
        {
            parameters.BarrelClipping = Mathf.Clamp(
                parameters.BarrelClipping,
                LensLimits.BarrelClipping.x,
                LensLimits.BarrelClipping.y);
        }

        /// <summary>
        /// Validates <see cref="LensIntrinsics.Anamorphism"/> by setting it into its valid bounds.
        /// </summary>
        static void ValidateAnamorphism(this ref LensIntrinsics parameters)
        {
            parameters.Anamorphism = Mathf.Clamp(
                parameters.Anamorphism,
                LensLimits.Anamorphism.x,
                LensLimits.Anamorphism.y);
        }
    }
}
