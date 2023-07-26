using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct that contains the settings of a virtual camera.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Settings : IEquatable<Settings>
    {
        internal const float k_MaxDamping = 1;
        internal const float k_MaxAbsFocusDistanceOffset = 0.3048f; // 12 inches

        /// <summary>
        /// The default CameraState.
        /// </summary>
        public static readonly Settings DefaultData = new Settings
        {
            Damping = Damping.Default,
            Rebasing = false,
            MotionScale = Vector3.one,
            ErgonomicTilt = 0,
            JoystickSensitivity = Vector3.one,
            PedestalSpace = Space.World,
            MotionSpace = Space.Self,
            AspectRatio = AspectRatioConsts.k_DefaultAspectRatio,
            GateFit = GateFit.Fill
        };

        /// <summary>
        /// The settings used to configure the smoothing applied to the camera motion.
        /// </summary>
        [Tooltip("The settings used to configure the smoothing applied to the camera motion.")]
        public Damping Damping;

        /// <summary>
        /// The position axes along which camera cannot move.
        /// </summary>
        [Tooltip("The position axes along which camera cannot move.")]
        [EnumFlagButtonGroup(60f)]
        public PositionAxis PositionLock;

        /// <summary>
        /// The rotation axes along which camera cannot rotate.
        /// </summary>
        [Tooltip("The rotation axes along which camera cannot rotate.")]
        [EnumFlagButtonGroup(60f)]
        public RotationAxis RotationLock;

        /// <summary>
        /// Force roll angle to be zero. Roll must be locked for this to take effect.
        /// </summary>
        [Tooltip("Force roll angle to be zero. Roll must be locked for this to take effect.")]
        public bool AutoHorizon;

        /// <summary>
        /// The angle around the x-axis to offset the local camera rotation.
        /// </summary>
        [Tooltip("The angle around the x-axis to offset the local camera rotation.")]
        public float ErgonomicTilt;

        /// <summary>
        /// Rebasing set to true indicates that the Virtual Camera is frozen in virtual space allowing the user
        /// to change position in real world space.
        /// </summary>
        [HideInInspector]
        public bool Rebasing;

        /// <summary>
        /// Scale of the movement for each axis. A scale of (1, 1, 1) means that the virtual camera position will match the device position in real world.
        /// </summary>
        [Tooltip("Scale of the movement for each axis. A scale of (1, 1, 1) means that the virtual camera position will match the device position in real world.")]
        public Vector3 MotionScale;

        /// <summary>
        /// Scaling applied to joystick motion.
        /// A scale of (1, 2, 1) means the joystick will translate the Y axis twice faster than the X and Z axis.
        /// </summary>
        [Tooltip("Scaling applied to joystick motion, A scale of (1, 2, 1) means the joystick will translate the Y axis twice faster than the X and Z axis.")]
        public Vector3 JoystickSensitivity;

        /// <summary>
        /// The space of the rig's pedestal movement.
        /// World space will translate the rig pedestal relative to the world axis.
        /// Self space will translate the rig pedestal relative to the camera's look direction.
        /// </summary>
        [Tooltip("The space of the rig's pedestal movement. \n" +
             "World space will translate the rig pedestal relative to the world axis. \n" +
             "Self space will translate the rig pedestal relative to the camera's look direction. \n")]
        [EnumButtonGroup(60f)]
        public Space PedestalSpace;

        /// <summary>
        /// The space of the rig's dolly and truck movement.
        /// World space will move the rig relative to the world axis.
        /// Self space will move the rig relative to the camera's look direction.
        /// </summary>
        [Tooltip("The space of the rig's dolly and truck movement. \n" +
             "World space will move the rig relative to the world axis. \n" +
             "Self space will move the rig relative to the camera's look direction. \n")]
        [EnumButtonGroup(60f)]
        public Space MotionSpace;

        /// <summary>
        /// The aspect ratio of the crop mask.
        /// </summary>
        [FormerlySerializedAs("CropAspect")]
        [AspectRatio]
        [Tooltip("The aspect ratio of the crop mask.")]
        public float AspectRatio;

        /// <summary>
        /// The current focusing behavior.
        /// </summary>
        [EnumButtonGroup(80f)]
        public FocusMode FocusMode;

        /// <summary>
        /// The position of the focus reticle.
        /// </summary>
        [Reticle]
        public Vector2 ReticlePosition;

        /// <summary>
        /// Offset applied to the focus distance when using auto focus.
        /// </summary>
        [Range(-k_MaxAbsFocusDistanceOffset, k_MaxAbsFocusDistanceOffset)]
        [Tooltip("Offset applied to the focus distance when using auto focus.")]
        public float FocusDistanceOffset;

        /// <summary>
        /// Damping applied to the focus distance.
        /// </summary>
        [Tooltip("Damping applied to the focus distance.")]
        [Range(0, k_MaxDamping)]
        public float FocusDistanceDamping;

        /// <summary>
        /// Damping applied to the focal length.
        /// </summary>
        [Tooltip("Damping applied to the focal length.")]
        [Range(0, k_MaxDamping)]
        public float FocalLengthDamping;

        /// <summary>
        /// Damping applied to the aperture.
        /// </summary>
        [Tooltip("Damping applied to the aperture.")]
        [Range(0, k_MaxDamping)]
        public float ApertureDamping;

        /// <summary>
        /// The gate fit mode.
        /// </summary>
        [Tooltip("The gate fit mode.")]
        [EnumButtonGroup(80f)]
        public GateFit GateFit;

        /// <summary>
        /// Whether or not to show the gate mask.
        /// </summary>
        [Tooltip("Whether or not to show the gate mask.")]
        public bool GateMask;

        /// <summary>
        /// Whether or not to show the aspect ratio frame lines.
        /// </summary>
        [FormerlySerializedAs("FrameLines")]
        [Tooltip("Whether or not to show the aspect ratio frame lines.")]
        public bool AspectRatioLines;

        /// <summary>
        /// Whether or not to show the center marker.
        /// </summary>
        [Tooltip("Whether or not to show the center marker.")]
        public bool CenterMarker;

        /// <summary>
        /// Whether or not to show the focus plane.
        /// </summary>
        [Tooltip("Whether or not to show the focus plane.")]
        public bool FocusPlane;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"(damping {Damping}, rotationLock {RotationLock}, positionLock {PositionLock}, " +
                $"ergonomicTilt {ErgonomicTilt}, rebasing {Rebasing}, motionScale {MotionScale}, " +
                $"focusMode {FocusMode}, joystickSensitivity {JoystickSensitivity}, pedestalSpace {PedestalSpace}, motionSpace {MotionSpace}, " +
                $"autoHorizon {AutoHorizon}, reticlePosition {ReticlePosition}, aspectRatio {AspectRatio}, focusDistanceOffset{FocusDistanceOffset}), " +
                $"focusDistanceDamping {FocusDistanceDamping}, focalLengthDamping {FocalLengthDamping}, apertureDamping {ApertureDamping}, " +
                $"gateMask {GateMask}, focusPlane {FocusPlane}, frameLines {AspectRatioLines}, centerMarker {CenterMarker}";
        }

        /// <summary>
        /// Sets the parameters into their valid domain.
        /// </summary>
        public void Validate()
        {
            AspectRatio = Mathf.Max(AspectRatioConsts.k_MinAspectRatio, AspectRatio);
            FocusDistanceOffset = Mathf.Clamp(FocusDistanceOffset, -k_MaxAbsFocusDistanceOffset, k_MaxAbsFocusDistanceOffset);
            FocusDistanceDamping = Mathf.Clamp(FocusDistanceDamping, 0, k_MaxDamping);
            FocalLengthDamping = Mathf.Clamp(FocalLengthDamping, 0, k_MaxDamping);
            ApertureDamping = Mathf.Clamp(ApertureDamping, 0, k_MaxDamping);
            ReticlePosition.x = Mathf.Clamp01(ReticlePosition.x);
            ReticlePosition.y = Mathf.Clamp01(ReticlePosition.y);
        }

        /// <inheritdoc/>
        public bool Equals(Settings other)
        {
            return Damping == other.Damping
                && RotationLock == other.RotationLock
                && PositionLock == other.PositionLock
                && ErgonomicTilt == other.ErgonomicTilt
                && Rebasing == other.Rebasing
                && MotionScale == other.MotionScale
                && FocusMode == other.FocusMode
                && JoystickSensitivity == other.JoystickSensitivity
                && PedestalSpace == other.PedestalSpace
                && MotionSpace == other.MotionSpace
                && AutoHorizon == other.AutoHorizon
                && ReticlePosition == other.ReticlePosition
                && AspectRatio == other.AspectRatio
                && FocusDistanceOffset == other.FocusDistanceOffset
                && FocusDistanceDamping == other.FocusDistanceDamping
                && FocalLengthDamping == other.FocalLengthDamping
                && ApertureDamping == other.ApertureDamping
                && GateFit == other.GateFit
                && GateMask == other.GateMask
                && FocusPlane == other.FocusPlane
                && AspectRatioLines == other.AspectRatioLines
                && CenterMarker == other.CenterMarker;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current CameraState.
        /// </summary>
        /// <param name="obj">The object to compare with the current CameraState.</param>
        /// <returns>
        /// true if the specified object is equal to the current CameraState; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Settings other && Equals(other);
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
                var hashCode = Damping.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)RotationLock;
                hashCode = (hashCode * 397) ^ (int)PositionLock;
                hashCode = (hashCode * 397) ^ ErgonomicTilt.GetHashCode();
                hashCode = (hashCode * 397) ^ Rebasing.GetHashCode();
                hashCode = (hashCode * 397) ^ MotionScale.GetHashCode();
                hashCode = (hashCode * 397) ^ FocusMode.GetHashCode();
                hashCode = (hashCode * 397) ^ JoystickSensitivity.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)PedestalSpace;
                hashCode = (hashCode * 397) ^ (int)MotionSpace;
                hashCode = (hashCode * 397) ^ AutoHorizon.GetHashCode();
                hashCode = (hashCode * 397) ^ ReticlePosition.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ FocusDistanceOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ FocusDistanceDamping.GetHashCode();
                hashCode = (hashCode * 397) ^ FocalLengthDamping.GetHashCode();
                hashCode = (hashCode * 397) ^ ApertureDamping.GetHashCode();
                hashCode = (hashCode * 397) ^ GateFit.GetHashCode();
                hashCode = (hashCode * 397) ^ GateMask.GetHashCode();
                hashCode = (hashCode * 397) ^ FocusPlane.GetHashCode();
                hashCode = (hashCode * 397) ^ AspectRatioLines.GetHashCode();
                hashCode = (hashCode * 397) ^ CenterMarker.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified CameraState are equal.
        /// </summary>
        /// <param name="a">The first CameraState.</param>
        /// <param name="b">The second CameraState.</param>
        /// <returns>
        /// true if the specified CameraState are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(Settings a, Settings b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified CameraState are different.
        /// </summary>
        /// <param name="a">The first CameraState.</param>
        /// <param name="b">The second CameraState.</param>
        /// <returns>
        /// true if the specified CameraState are different; otherwise, false.
        /// </returns>
        public static bool operator !=(Settings a, Settings b)
        {
            return !(a == b);
        }
    }
}
