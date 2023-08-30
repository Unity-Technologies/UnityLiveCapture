using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Contains the damping data.
    /// </summary>
    [Serializable]
    public struct Damping : IEquatable<Damping>
    {
        /// <summary>
        /// A rig that applies no damping.
        /// </summary>
        public static readonly Damping Default = new Damping
        {
            Enabled = false,
            Body = Vector3.one,
            Aim = 1
        };

        /// <summary>
        /// Enable or disable the damping.
        /// </summary>
        [Tooltip("Enable or disable damping.")]
        public bool Enabled;

        /// <summary>
        /// Time in seconds for the camera to reach reach the target position.
        /// </summary>
        /// <remarks>
        /// Values should not be negative. Typical values would range between 0 and 3.
        /// </remarks>
        [Tooltip("Time in seconds for the camera to reach reach the target position.")]
        public Vector3 Body;

        /// <summary>
        /// Time in seconds for the camera to catch up with the target rotation.
        /// </summary>
        /// <remarks>
        /// Values should not be negative. Typical values would range between 0 and 3.
        /// </remarks>
        [Tooltip("Time in seconds for the camera to catch up with the target rotation.")]
        public float Aim;

        /// <summary>
        /// Determines whether the two specified Damping are equal.
        /// </summary>
        /// <param name="a">The first Damping.</param>
        /// <param name="b">The second Damping.</param>
        /// <returns>
        /// true if the specified Damping are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(Damping a, Damping b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified Damping are different.
        /// </summary>
        /// <param name="a">The first Damping.</param>
        /// <param name="b">The second Damping.</param>
        /// <returns>
        /// true if the specified Damping are different; otherwise, false.
        /// </returns>
        public static bool operator !=(Damping a, Damping b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current Damping.
        /// </summary>
        /// <param name="obj">The object to compare with the current Damping.</param>
        /// <returns>
        /// true if the specified object is equal to the current Damping; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Damping damping && Equals(damping);
        }

        /// <summary>
        /// Determines whether the <see cref="Damping"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Damping"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Damping other)
        {
            return Enabled == other.Enabled &&
                Body == other.Body &&
                Aim == other.Aim;
        }

        /// <summary>
        /// Gets the hash code for the Damping.
        /// </summary>
        /// <returns>
        /// The hash value generated for this Damping.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -273304878;
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + Body.GetHashCode();
            hashCode = hashCode * -1521134295 + Aim.GetHashCode();
            return hashCode;
        }
    }
}
