using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.TouchFramework;

namespace Unity.CompanionApps.VirtualCamera
{
    // Focal length scale values according to FOV.
    // TODO: make internal, public for testing purposes.
    class FocalLengthScaler : IScaler
    {
        static readonly Vector2 k_DefaultSensorSize = new Vector2(10, 10);

        Vector2 m_SensorSize = k_DefaultSensorSize;
        Vector2 m_CachedSensorSize = k_DefaultSensorSize;
        float m_MinValue;
        float m_MaxValue;
        float m_AngularRange;
        bool m_IsDirty = false;

        public bool isDirty => m_IsDirty;

        public void MarkClean() { m_IsDirty = false; }

        public Vector2 SensorSize
        {
            set
            {
                m_SensorSize = value;
                // Updates may be costly so tolerate epsilon.
                if (!(
                    Mathf.Approximately(m_CachedSensorSize.x, m_SensorSize.x) &&
                    Mathf.Approximately(m_CachedSensorSize.y, m_SensorSize.y)))
                {
                    m_CachedSensorSize = m_SensorSize;
                    m_IsDirty = true;
                }
            }
        }

        public void Configure(float minVal, float maxVal, float angularRange)
        {
            m_MinValue = minVal;
            m_MaxValue = maxVal;
            m_AngularRange = angularRange;
        }

        public float ValueToAngle(float value)
        {
            if (Mathf.Approximately(m_MaxValue, m_MinValue))
                return 0;

            var fovMin = Camera.FocalLengthToFieldOfView(m_MinValue, m_SensorSize.y);
            var fovMax = Camera.FocalLengthToFieldOfView(m_MaxValue, m_SensorSize.y);
            var fov = Camera.FocalLengthToFieldOfView(value, m_SensorSize.y);
            var normalizedValue = (fovMax - fov) / (fovMax - fovMin);
            return Mathf.Lerp(-m_AngularRange, m_AngularRange, normalizedValue);
        }

        public float AngleToValue(float angle)
        {
            if (Mathf.Approximately(m_MaxValue, m_MinValue))
                return 0;

            var normalizedAngle = (m_AngularRange - angle) / (m_AngularRange * 2);
            var fovMin = Camera.FocalLengthToFieldOfView(m_MinValue, m_SensorSize.y);
            var fovMax = Camera.FocalLengthToFieldOfView(m_MaxValue, m_SensorSize.y);
            var fov = Mathf.Lerp(fovMin, fovMax, normalizedAngle);
            return Camera.FieldOfViewToFocalLength(fov, m_SensorSize.y);
        }
    }
}
