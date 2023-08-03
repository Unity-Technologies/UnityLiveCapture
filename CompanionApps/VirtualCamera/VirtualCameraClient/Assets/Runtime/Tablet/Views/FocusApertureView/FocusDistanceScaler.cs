using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    /// <summary>
    /// This scaler simulates the hyperbolic behaviour of the focus distance dial.
    /// </summary>
    class FocusDistanceScaler : IScaler
    {
        float m_MinValue;
        float m_MaxValue;
        float m_AngularRange;

        public void Configure(float minVal, float maxVal, float angularRange)
        {
            m_MinValue = Mathf.Max(Mathf.Epsilon, minVal);
            m_MaxValue = Mathf.Max(m_MinValue + Mathf.Epsilon, maxVal);
            m_AngularRange = Mathf.Max(Mathf.Epsilon, angularRange);
        }

        public float ValueToAngle(float value)
        {
            var normalizedValue = FocusDistanceUtility.Normalize(m_MinValue, m_MaxValue, value);
            return Mathf.Lerp(m_AngularRange, -m_AngularRange, normalizedValue);
        }

        public float AngleToValue(float angle)
        {
            var normalizedAngle = (m_AngularRange - angle) / (m_AngularRange * 2);
            return FocusDistanceUtility.Denormalize(m_MinValue, m_MaxValue, normalizedAngle);
        }

        public bool isDirty => false;

        public void MarkClean() {}
    }
}
