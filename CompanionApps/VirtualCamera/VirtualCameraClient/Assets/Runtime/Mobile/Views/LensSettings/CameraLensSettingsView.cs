using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using TMPro;

namespace Unity.CompanionApps.VirtualCamera
{
    class CameraLensSettingsView : DialogView, ICameraLensSettingsView
    {
        public event Action<Toggle, LensSettingsViewId> Toggled = delegate {};
        public event Action<float> FocusDistanceValueChanged = delegate {};
        public event Action<float> ApertureValueChanged = delegate {};
        public event Action CancelClicked = delegate {};
        public event Action<float> FocalLengthValueChanged = delegate {};

        [SerializeField]
        Toggle m_ZoomToggle;
        [SerializeField]
        Toggle m_ApertureToggle;
        [SerializeField]
        Toggle m_FocusDistanceToggle;
        [SerializeField]
        Toggle m_FocusModeToggle;

        // App flow
        [SerializeField]
        Button m_CancelButton;

        //Focal length
        [SerializeField]
        TMP_Text m_ZoomTitle;
        [SerializeField]
        TMP_Text m_ZoomValue;
        [SerializeField]
        Slider m_ZoomSlider;

        // Aperture
        [SerializeField]
        TMP_Text m_ApertureTitle;
        [SerializeField]
        TMP_Text m_ApertureValue;
        [SerializeField]
        Slider m_ApertureSlider;

        // Focus distance
        [SerializeField]
        TMP_Text m_FocusDistanceTitle;
        [SerializeField]
        TMP_Text m_FocusDistanceValue;
        [SerializeField]
        Slider m_FocusDistanceSlider;

        // Focus mode
        [SerializeField]
        TMP_Text m_FocusModeTitle;
        [SerializeField]
        TMP_Text m_FocusModeValue;
        [SerializeField]
        GameObject m_FocusModeDialog;
        // TODO this is an intermediary refactoring step
        [SerializeField]
        FocusModeView m_FocusModeView;

        bool m_HelpMode;

        FocusDistanceSliderAdapter m_FocusDistanceSliderAdapter;

        public void Awake()
        {
            PrepareToggle(m_ZoomToggle, LensSettingsViewId.FocalLength);
            PrepareToggle(m_FocusDistanceToggle, LensSettingsViewId.FocusDistance);
            PrepareToggle(m_ApertureToggle, LensSettingsViewId.Aperture);
            PrepareToggle(m_FocusModeToggle, LensSettingsViewId.FocusMode);

            m_CancelButton.onClick.AddListener(OnCancelClicked);
            m_ZoomSlider.onValueChanged.AddListener(OnFocalLengthValueChanged);
            m_ApertureSlider.onValueChanged.AddListener(OnApertureValueChanged);
            m_FocusDistanceSliderAdapter = new FocusDistanceSliderAdapter(m_FocusDistanceSlider);
            m_FocusDistanceSliderAdapter.onValueChanged.AddListener(OnFocusDistanceValueChanged);
        }

        public void SetFocalLength(float focalLength, Vector2 range)
        {
            m_ZoomValue.text = $"{(int)focalLength}mm";
            m_ZoomSlider.minValue = range.x;
            m_ZoomSlider.maxValue = range.y;
            m_ZoomSlider.SetValueWithoutNotify(focalLength);
        }

        public void SetFocusDistance(float focusDistance, Vector2 range)
        {
            var roundedFocusDistance = (float)Math.Round(focusDistance, 1);
            m_FocusDistanceValue.text = FocusDistanceUtility.AsString(roundedFocusDistance, range.x, "m");
            m_FocusDistanceSliderAdapter.SetMinValue(range.x, false);
            m_FocusDistanceSliderAdapter.SetMaxValue(range.y, false);
            m_FocusDistanceSliderAdapter.SetValue(focusDistance, false);
        }

        public void SetAperture(float aperture, Vector2 range)
        {
            var roundedAperture = (float)Math.Round(aperture, 1);
            m_ApertureValue.text = "f/" + roundedAperture.ToString();
            m_ApertureSlider.minValue = range.x;
            m_ApertureSlider.maxValue = range.y;
            m_ApertureSlider.SetValueWithoutNotify(aperture);
        }

        public void SetFocusMode(FocusMode focusMode)
        {
            m_FocusModeValue.text = focusMode.GetDescription();
            m_FocusModeView.SetFocusMode(focusMode);
        }

        public void SetHelpMode(bool value)
        {
            m_HelpMode = value;

            ToggleOff();
            UpdateControls();
        }

        public void ToggleOff()
        {
            m_ZoomToggle.SetIsOnWithoutNotify(false);
            m_FocusDistanceToggle.SetIsOnWithoutNotify(false);
            m_ApertureToggle.SetIsOnWithoutNotify(false);
            m_FocusModeToggle.SetIsOnWithoutNotify(false);

            HideControls();
        }

        void HideControls()
        {
            m_ZoomSlider.gameObject.SetActive(false);
            m_FocusDistanceSlider.gameObject.SetActive(false);
            m_ApertureSlider.gameObject.SetActive(false);
            m_FocusModeDialog.SetActive(false);
        }

        void UpdateControls()
        {
            if (!m_HelpMode)
            {
                m_ZoomSlider.gameObject.SetActive(m_ZoomToggle.isOn);
                m_FocusDistanceSlider.gameObject.SetActive(m_FocusDistanceToggle.isOn);
                m_ApertureSlider.gameObject.SetActive(m_ApertureToggle.isOn);
                m_FocusModeDialog.SetActive(m_FocusModeToggle.isOn);
            }
        }

        void PrepareToggle(Toggle toggle, LensSettingsViewId id)
        {
            toggle.onValueChanged.AddListener(v =>
            {
                UpdateControls();
                Toggled.Invoke(toggle, id);
            });
        }

        void OnCancelClicked()
        {
            CancelClicked.Invoke();
        }

        void OnFocalLengthValueChanged(float value)
        {
            var roundedValue = (float)Math.Round(value, 1);
            FocalLengthValueChanged.Invoke(roundedValue);
        }

        void OnApertureValueChanged(float value)
        {
            var roundedValue = (float)Math.Round(value, 1);
            ApertureValueChanged.Invoke(roundedValue);
        }

        void OnFocusDistanceValueChanged(float value)
        {
            var roundedValue = (float)Math.Round(value, 1);
            FocusDistanceValueChanged.Invoke(roundedValue);
        }
    }
}
