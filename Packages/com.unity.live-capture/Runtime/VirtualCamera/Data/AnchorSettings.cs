using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct that contains the anchor settings of a virtual camera.
    /// </summary>
    [Serializable]
    public struct AnchorSettings : IEquatable<AnchorSettings>
    {
        /// <summary>
        /// The default AnchorSettings.
        /// </summary>
        public static readonly AnchorSettings Default = new AnchorSettings
        {
            PositionOffset = Vector3.zero,
            PositionLock = Axis.X | Axis.Y | Axis.Z,
            RotationLock = Axis.None,
            Damping = Damping.Default,
        };

        /// <summary>
        /// Offset local to the target's anchor Transform.
        /// </summary>
        [Tooltip("Offset local to the target's anchor Transform.")]
        public Vector3 PositionOffset;

        /// <summary>
        /// The position axes to follow when anchored.
        /// </summary>
        [Tooltip("The position axes to follow when anchored.")]
        [EnumFlagButtonGroup(60f)]
        public Axis PositionLock;

        /// <summary>
        /// The rotation axes to follow when anchored.
        /// </summary>
        [Tooltip("The rotation axes to follow when anchored.")]
        [EnumFlagButtonGroup(60f)]
        public Axis RotationLock;

        /// <summary>
        /// The settings used to configure the smoothing applied to the anchor's motion.
        /// </summary>
        [Tooltip("The settings used to configure the smoothing applied to the anchor's motion.")]
        public Damping Damping;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"positionOffset {PositionOffset}, " +
                $"positionLock {PositionLock}, rotationLock {RotationLock} " +
                $"damping {Damping}";
        }

        /// <summary>
        /// Determines whether the <see cref="AnchorSettings"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="AnchorSettings"/> to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(AnchorSettings other)
        {
            return PositionOffset == other.PositionOffset
               && PositionLock == other.PositionLock
               && RotationLock == other.RotationLock
               && Damping == other.Damping;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current AnchorSettings.
        /// </summary>
        /// <param name="obj">The object to compare with the current AnchorSettings.</param>
        /// <returns>
        /// True if the specified object is equal to the current AnchorSettings; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is AnchorSettings other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the AnchorSettings.
        /// </summary>
        /// <returns>
        /// The hash value generated for these AnchorSettings.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PositionOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ PositionLock.GetHashCode();
                hashCode = (hashCode * 397) ^ RotationLock.GetHashCode();
                hashCode = (hashCode * 397) ^ Damping.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified AnchorSettings are equal.
        /// </summary>
        /// <param name="a">The first AnchorSettings.</param>
        /// <param name="b">The second AnchorSettings.</param>
        /// <returns>
        /// True if the specified AnchorSettings are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(AnchorSettings a, AnchorSettings b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified AnchorSettings are different.
        /// </summary>
        /// <param name="a">The first AnchorSettings.</param>
        /// <param name="b">The second AnchorSettings.</param>
        /// <returns>
        /// True if the specified AnchorSettings are different; otherwise, false.
        /// </returns>
        public static bool operator !=(AnchorSettings a, AnchorSettings b)
        {
            return !(a == b);
        }
    }
}
