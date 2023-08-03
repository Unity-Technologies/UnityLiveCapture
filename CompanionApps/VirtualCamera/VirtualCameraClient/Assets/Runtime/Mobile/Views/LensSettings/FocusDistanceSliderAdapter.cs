using System;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusDistanceSliderAdapter : IDisposable
    {
        Slider m_Slider;
        Slider.SliderEvent m_OnValueChanged = new Slider.SliderEvent();
        float m_MinValue = 1;
        float m_MaxValue = 10;
        float m_Value = 1;

        public Slider.SliderEvent onValueChanged => m_OnValueChanged;

        public void SetMinValue(float min, bool sendCallback = true)
        {
            m_MinValue = min;
            UpdateSlider(sendCallback);
        }

        public void SetMaxValue(float max, bool sendCallback = true)
        {
            m_MaxValue = max;
            UpdateSlider(sendCallback);
        }

        public void SetValue(float newValue, bool sendCallback = true)
        {
            m_Value = newValue;
            UpdateSlider(sendCallback);
        }

        // We do not modify the values we are passed, but we only operate when those are valid.
        public bool IsValid() => m_MinValue > 0 && m_MaxValue > m_MinValue && m_MinValue <= m_Value && m_Value <= m_MaxValue;

        public FocusDistanceSliderAdapter(Slider slider)
        {
            m_Slider = slider;
            m_Slider.minValue = 0;
            m_Slider.maxValue = 1;
            m_Slider.onValueChanged.AddListener(OnValueChanged);
        }

        public void Dispose()
        {
            m_Slider.onValueChanged.RemoveListener(OnValueChanged);
            m_OnValueChanged.RemoveAllListeners();
        }

        void UpdateSlider(bool sendCallback)
        {
            if (IsValid())
            {
                var normalized = FocusDistanceUtility.Normalize(m_MinValue, m_MaxValue, m_Value);
                if (sendCallback)
                {
                    m_Slider.normalizedValue = normalized;
                }
                else
                {
                    var denormalized = UnityEngine.Mathf.Lerp(m_Slider.minValue, m_Slider.maxValue, normalized);
                    m_Slider.SetValueWithoutNotify(denormalized);
                }
            }
        }

        void OnValueChanged(float _)
        {
            if (IsValid())
            {
                var normalized = m_Slider.normalizedValue;
                var denormalized = FocusDistanceUtility.Denormalize(m_MinValue, m_MaxValue, normalized);
                m_OnValueChanged.Invoke(denormalized);
            }
        }
    }
}
