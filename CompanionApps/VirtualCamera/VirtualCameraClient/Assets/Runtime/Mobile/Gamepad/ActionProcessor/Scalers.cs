using Unity.LiveCapture.VirtualCamera;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// No scaling, always increase/decrease in seconds.
    /// Defers timeline start/end clamping to the server.
    /// </summary>
    class TimeScaler : IScaler<double>
    {
        public double ClampToInputScale(double value)
        {
            return value;
        }

        public double ToInputScale(double value)
        {
            return value;
        }

        public double ToValueScale(double value)
        {
            return value;
        }
    }

    class FocalLengthScaler : IScaler<float>
    {
        Vector2 m_Range;
        Vector2 m_SensorSize;

        public Vector2 Range
        {
            get => m_Range;
            set => m_Range = value;
        }

        public Vector2 SensorSize
        {
            get => m_SensorSize;
            set => m_SensorSize = value;
        }

        public float ClampToInputScale(float value)
        {
            return Mathf.Clamp01(value);
        }

        public float ToInputScale(float value)
        {
            if (Mathf.Approximately(m_Range.x, m_Range.y))
                return 0;

            var fovMin = Camera.FocalLengthToFieldOfView(m_Range.y, m_SensorSize.y);
            var fovMax = Camera.FocalLengthToFieldOfView(m_Range.x, m_SensorSize.y);
            var fov = Camera.FocalLengthToFieldOfView(value, m_SensorSize.y);
            var normalizedValue = (fovMax - fov) / (fovMax - fovMin);
            return Mathf.Clamp01(normalizedValue);
        }

        public float ToValueScale(float value)
        {
            if (Mathf.Approximately(m_Range.x, m_Range.y))
                return 0;

            var fovMin = Camera.FocalLengthToFieldOfView(m_Range.x, m_SensorSize.y);
            var fovMax = Camera.FocalLengthToFieldOfView(m_Range.y, m_SensorSize.y);
            var fov = Mathf.Lerp(fovMin, fovMax, value);
            return Camera.FieldOfViewToFocalLength(fov, m_SensorSize.y);
        }
    }

    class ApertureScaler : IScaler<float>
    {
        Vector2 m_Range;

        public Vector2 Range
        {
            get => m_Range;
            set => m_Range = value;
        }

        public float ClampToInputScale(float value)
        {
            return Mathf.Clamp01(value);
        }

        public float ToInputScale(float value)
        {
            return Mathf.InverseLerp(m_Range.x, m_Range.y, value);
        }

        public float ToValueScale(float value)
        {
            return Mathf.Lerp(m_Range.x, m_Range.y, value);
        }
    }

    class FocusDistanceScaler : IScaler<float>
    {
        Vector2 m_Range;

        public Vector2 Range
        {
            get => m_Range;
            set => m_Range = value;
        }

        public float ClampToInputScale(float value)
        {
            return Mathf.Clamp01(value);
        }

        public float ToInputScale(float value)
        {
            return FocusDistanceUtility.Normalize(m_Range.x, m_Range.y, value);
        }

        public float ToValueScale(float value)
        {
            return FocusDistanceUtility.Denormalize(m_Range.x, m_Range.y, value);
        }
    }
}
