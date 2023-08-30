using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Contains the minimum and maximum values for each camera body parameter.
    /// </summary>
    static class CameraBodyParameterLimits
    {
        public static readonly Vector2 SensorSize = new Vector2(1f, 205f);
        public static readonly Vector2Int Iso = new Vector2Int(1, 10000);
        public static readonly Vector2 ShutterSpeed = new Vector2(0f, 10f);
    }

    /// <summary>
    /// Contains all the parameters needed to model a physical camera body.
    /// </summary>
    [Serializable]
    public struct CameraBody : IEquatable<CameraBody>
    {
        static readonly Vector2 k_DefaultSensorSize = new Vector2(24.89f, 18.66f);
        static readonly int k_DefaultIso = 200;
        static readonly float k_DefaultShutterSpeed = 0.005f;

        /// <summary>
        /// The default CameraBody.
        /// </summary>
        public static readonly CameraBody DefaultParams = new CameraBody
        {
            SensorSize = k_DefaultSensorSize,
            Iso = k_DefaultIso,
            ShutterSpeed = k_DefaultShutterSpeed,
        };

        /// <summary>
        /// The default sensor size.
        /// </summary>
        public static Vector2 DefaultSensorSize => k_DefaultSensorSize;

        [SerializeField, SensorSize]
        Vector2 m_SensorSize;
        [SerializeField]
        int m_Iso;
        [SerializeField]
        float m_ShutterSpeed;

        /// <summary>
        /// Set the size of the real-world camera sensor in millimeters.
        /// </summary>
        public Vector2 SensorSize
        {
            get => m_SensorSize;
            set => m_SensorSize = value;
        }

        /// <summary>
        /// Set the sensitivity of the real-world camera sensor.
        /// </summary>
        /// <remarks>
        /// Higher values increase the camera's sensitivity to light and result in faster exposure times.
        /// </remarks>
        public int Iso
        {
            get => m_Iso;
            set => m_Iso = value;
        }

        /// <summary>
        /// Sets the exposure time for the camera in seconds.
        /// </summary>
        /// <remarks>
        /// Lower values result in less exposed pictures.
        /// </remarks>
        public float ShutterSpeed
        {
            get => m_ShutterSpeed;
            set => m_ShutterSpeed = value;
        }

        /// <inheritdoc/>
        public bool Equals(CameraBody other)
        {
            return SensorSize == other.SensorSize
                && Iso == other.Iso
                && ShutterSpeed == other.ShutterSpeed;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current CameraBody.
        /// </summary>
        /// <param name="obj">The object to compare with the current CameraBody.</param>
        /// <returns>
        /// true if the specified object is equal to the current CameraBody; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is CameraBody other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the CameraBody.
        /// </summary>
        /// <returns>
        /// The hash value generated for this CameraBody.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SensorSize.GetHashCode();
                hashCode = (hashCode * 397) ^ Iso.GetHashCode();
                hashCode = (hashCode * 397) ^ ShutterSpeed.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified CameraBody are equal.
        /// </summary>
        /// <param name="a">The first CameraBody.</param>
        /// <param name="b">The second CameraBody.</param>
        /// <returns>
        /// true if the specified CameraBody are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(CameraBody a, CameraBody b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified CameraBody are different.
        /// </summary>
        /// <param name="a">The first CameraBody.</param>
        /// <param name="b">The second CameraBody.</param>
        /// <returns>
        /// true if the specified CameraBody are different; otherwise, false.
        /// </returns>
        public static bool operator !=(CameraBody a, CameraBody b)
        {
            return !(a == b);
        }
    }

    /// <summary>
    /// A class that contains extension methods for <see cref="CameraBody"/>.
    /// </summary>
    static class CameraBodyExtensions
    {
        /// <summary>
        /// Validates each parameter by setting them into their valid range.
        /// </summary>
        public static void Validate(this ref CameraBody cameraParams)
        {
            cameraParams.ValidateSensorSize();
            cameraParams.ValidateIso();
            cameraParams.ValidateShutterSpeed();
        }

        /// <summary>
        /// Validates <see cref="CameraBody.SensorSize"/> by setting it into its valid bounds.
        /// </summary>
        public static void ValidateSensorSize(this ref CameraBody cameraParams)
        {
            var sensorSize = cameraParams.SensorSize;

            sensorSize.x = Mathf.Clamp(
                sensorSize.x,
                CameraBodyParameterLimits.SensorSize.x,
                CameraBodyParameterLimits.SensorSize.y);

            sensorSize.y = Mathf.Clamp(
                sensorSize.y,
                CameraBodyParameterLimits.SensorSize.x,
                CameraBodyParameterLimits.SensorSize.y);

            cameraParams.SensorSize = sensorSize;
        }

        /// <summary>
        /// Validates <see cref="CameraBody.Iso"/> by setting it into its valid bounds.
        /// </summary>
        public static void ValidateIso(this ref CameraBody cameraParams)
        {
            cameraParams.Iso = Mathf.Clamp(
                cameraParams.Iso,
                CameraBodyParameterLimits.Iso.x,
                CameraBodyParameterLimits.Iso.y);
        }

        /// <summary>
        /// Validates <see cref="CameraBody.ShutterSpeed"/> by setting it into its valid bounds.
        /// </summary>
        public static void ValidateShutterSpeed(this ref CameraBody cameraParams)
        {
            cameraParams.ShutterSpeed = Mathf.Clamp(
                cameraParams.ShutterSpeed,
                CameraBodyParameterLimits.ShutterSpeed.x,
                CameraBodyParameterLimits.ShutterSpeed.y);
        }
    }
}
