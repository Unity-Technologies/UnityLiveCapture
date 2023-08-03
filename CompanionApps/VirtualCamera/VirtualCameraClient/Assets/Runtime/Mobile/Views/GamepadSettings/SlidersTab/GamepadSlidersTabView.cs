using System;
using TMPro;
using Unity.CompanionApps.VirtualCamera.Gamepad;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadSlidersTabView : MonoBehaviour, IGamepadSlidersTabView
    {
        public event Action<AxisID, float> onSensitivityChanged = delegate {};

        [SerializeField]
        TMP_InputField m_FocalLengthInput;
        [SerializeField]
        TMP_InputField m_ApertureInput;
        [SerializeField]
        TMP_InputField m_FocusDistanceInput;
        [SerializeField]
        TMP_InputField m_TimeInput;

        FloatInputWrapper m_FocalLengthInputWrapper;
        FloatInputWrapper m_ApertureInputWrapper;
        FloatInputWrapper m_FocusDistanceInputWrapper;
        FloatInputWrapper m_TimeInputWrapper;

        public void Awake()
        {
            m_FocalLengthInputWrapper = new FloatInputWrapper(m_FocalLengthInput);
            m_ApertureInputWrapper = new FloatInputWrapper(m_ApertureInput);
            m_FocusDistanceInputWrapper = new FloatInputWrapper(m_FocusDistanceInput);
            m_TimeInputWrapper = new FloatInputWrapper(m_TimeInput);
        }

        void OnDestroy()
        {
            m_FocalLengthInputWrapper.Dispose();
            m_ApertureInputWrapper.Dispose();
            m_FocusDistanceInputWrapper.Dispose();
            m_TimeInputWrapper.Dispose();
        }

        void OnEnable()
        {
            m_FocalLengthInputWrapper.onValueChanged += OnFocalLengthValueChanged;
            m_ApertureInputWrapper.onValueChanged += OnApertureValueChanged;
            m_FocusDistanceInputWrapper.onValueChanged += OnFocusDistanceValueChanged;
            m_TimeInputWrapper.onValueChanged += OnTimeValueChanged;
        }

        void OnDisable()
        {
            m_FocalLengthInputWrapper.onValueChanged -= OnFocalLengthValueChanged;
            m_ApertureInputWrapper.onValueChanged -= OnApertureValueChanged;
            m_FocusDistanceInputWrapper.onValueChanged -= OnFocusDistanceValueChanged;
            m_TimeInputWrapper.onValueChanged -= OnTimeValueChanged;
        }

        public void SetSensitivity(AxisID axis, float sensitivity)
        {
            switch (axis)
            {
                case AxisID.Zoom:
                    m_FocalLengthInputWrapper.SetValue(sensitivity);
                    break;
                case AxisID.FStop:
                    m_ApertureInputWrapper.SetValue(sensitivity);
                    break;
                case AxisID.FocusDistance:
                    m_FocusDistanceInputWrapper.SetValue(sensitivity);
                    break;
                case AxisID.Time:
                    m_TimeInputWrapper.SetValue(sensitivity);
                    break;
            }
        }

        void OnFocalLengthValueChanged(float value)
        {
            onSensitivityChanged.Invoke(AxisID.Zoom, value);
        }

        void OnApertureValueChanged(float value)
        {
            onSensitivityChanged.Invoke(AxisID.FStop, value);
        }

        void OnFocusDistanceValueChanged(float value)
        {
            onSensitivityChanged.Invoke(AxisID.FocusDistance, value);
        }

        void OnTimeValueChanged(float value)
        {
            onSensitivityChanged.Invoke(AxisID.Time, value);
        }
    }
}
