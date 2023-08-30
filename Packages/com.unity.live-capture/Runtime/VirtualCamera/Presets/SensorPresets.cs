using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct that stores a sensor preset.
    /// </summary>
    [Serializable]
    public struct SensorPreset : IEquatable<SensorPreset>
    {
        [SerializeField, Tooltip("The name of the sensor preset.")]
        string m_Name;

        [SerializeField, Tooltip("The width and height of sensor in mm.")]
        Vector2 m_SensorSize;

        /// <summary>
        /// The name of the preset.
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        /// The size of the sensor in the preset.
        /// </summary>
        public Vector2 SensorSize
        {
            get => m_SensorSize;
            set => m_SensorSize = value;
        }

        /// <summary>
        /// Determines whether the specified SensorPreset is equal to the current SensorPreset.
        /// </summary>
        /// <param name="other">The SensorPreset to compare with the current SensorPreset.</param>
        /// <returns>
        /// true if the specified SensorPreset is equal to the current SensorPreset; otherwise, false.
        /// </returns>
        public bool Equals(SensorPreset other)
        {
            return Name == other.Name && SensorSize.Equals(other.SensorSize);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current SensorPreset.
        /// </summary>
        /// <param name="obj">The object to compare with the current SensorPreset.</param>
        /// <returns>
        /// true if the specified object is equal to the current SensorPreset; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is SensorPreset other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the SensorPreset.
        /// </summary>
        /// <returns>
        /// The hash value generated for this SensorPreset.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ SensorSize.GetHashCode();
            }
        }

        /// <summary>
        /// Determines whether the two specified SensorPreset are equal.
        /// </summary>
        /// <param name="left">The first SensorPreset.</param>
        /// <param name="right">The second SensorPreset.</param>
        /// <returns>
        /// true if the specified SensorPreset are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(SensorPreset left, SensorPreset right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether the two specified SensorPreset are different.
        /// </summary>
        /// <param name="left">The first SensorPreset.</param>
        /// <param name="right">The second SensorPreset.</param>
        /// <returns>
        /// true if the specified SensorPreset are different; otherwise, false.
        /// </returns>
        public static bool operator !=(SensorPreset left, SensorPreset right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Asset that stores lists of presets for sensor sizes.
    /// </summary>
    [CreateAssetMenu(fileName = "Sensor Presets", menuName = "Live Capture/Virtual Camera/Sensor Presets", order = 1)]
    [HelpURL(Documentation.baseURL + "ref-asset-sensor-presets" + Documentation.endURL)]
    [ExcludeFromPreset]
    public class SensorPresets : ScriptableObject
    {
        [SerializeField]
        List<SensorPreset> m_Sensors = new List<SensorPreset>();

        /// <summary>
        /// The array of sensor size presets.
        /// </summary>
        /// <remarks>This will return a new copy of the array.</remarks>
        public SensorPreset[] Sensors => m_Sensors.ToArray();
    }
}
