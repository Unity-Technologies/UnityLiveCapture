using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct that contains the anchor settings of a virtual camera.
    /// </summary>
    [Serializable]
    struct AnchorDeviceSettings : IEquatable<AnchorDeviceSettings>
    {
        /// <summary>
        /// The default AnchorSettings, with anchoring disabled.
        /// </summary>
        public static readonly AnchorDeviceSettings Default = new AnchorDeviceSettings
        {
            Enabled = false,
            Target = null,
            Settings = AnchorSettings.Default,
        };

        /// <summary>
        /// Enable or disable anchoring.
        /// </summary>
        [Tooltip("Enable or disable anchoring.")]
        public bool Enabled;

        /// <summary>
        /// The target to anchor to.
        /// </summary>
        [Tooltip("The target to anchor to.")]
        public Transform Target;

        /// <summary>
        /// The settings of the anchoring behavior.
        /// </summary>
        [Tooltip("The settings of the anchoring behavior.")]
        public AnchorSettings Settings;

        /// <summary>
        /// Returns true if the settings are well configured for the given target.
        /// </summary>
        public bool IsValid => Target != null;

        /// <summary>
        /// Returns true if the settings are well configured and anchoring is enabled.
        /// </summary>
        public bool IsActive => Enabled && IsValid;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"enabled {Enabled}, target {Target}, settings {Settings}";
        }

        /// <inheritdoc/>
        public bool Equals(AnchorDeviceSettings other)
        {
            return Enabled == other.Enabled
                   && Target == other.Target
                   && Settings == other.Settings;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current AnchorDeviceSettings.
        /// </summary>
        /// <param name="obj">The object to compare with the current AnchorDeviceSettings.</param>
        /// <returns>
        /// true if the specified object is equal to the current AnchorDeviceSettings; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is AnchorDeviceSettings other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the AnchorDeviceSettings.
        /// </summary>
        /// <returns>
        /// The hash value generated for this AnchorDeviceSettings.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ Target.GetHashCode();
                hashCode = (hashCode * 397) ^ Settings.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified AnchorDeviceSettings are equal.
        /// </summary>
        /// <param name="a">The first AnchorDeviceSettings.</param>
        /// <param name="b">The second AnchorDeviceSettings.</param>
        /// <returns>
        /// true if the specified AnchorDeviceSettings are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(AnchorDeviceSettings a, AnchorDeviceSettings b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified AnchorDeviceSettings are different.
        /// </summary>
        /// <param name="a">The first AnchorDeviceSettings.</param>
        /// <param name="b">The second AnchorDeviceSettings.</param>
        /// <returns>
        /// true if the specified AnchorDeviceSettings are different; otherwise, false.
        /// </returns>
        public static bool operator !=(AnchorDeviceSettings a, AnchorDeviceSettings b)
        {
            return !(a == b);
        }
    }
}
