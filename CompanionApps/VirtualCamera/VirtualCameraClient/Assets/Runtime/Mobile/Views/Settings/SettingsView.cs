using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;
using TMPro;

namespace Unity.CompanionApps.VirtualCamera
{
    class SettingsView : DialogView, ISettingsView
    {
        public event Action DoneClicked = delegate {};

        public event Action PrivacyPolicyClicked = delegate {};
        public event Action DocumentationClicked = delegate {};
        public event Action SupportClicked = delegate {};

        // Timecode
        public event Action<string> TimecodeSourceChanged = delegate {};

        // Main View.
        public event Action<bool> JoysticksToggled = delegate {};
        public event Action<bool> CameraSettingsToggled = delegate {};
        public event Action<bool> LensSettingsToggled = delegate {};
        public event Action<bool> InformationBarToggled = delegate {};

        // Camera Options.
        public event Action<bool> GateMaskToggled = delegate {};
        public event Action<bool> FrameLinesToggled = delegate {};
        public event Action<bool> CenterMarkerToggled = delegate {};
        public event Action<bool> FocusPlaneToggled = delegate {};

        // Ergonomic Tilt.
        public event Action SetToCurrentTiltClicked = delegate {};
        public event Action ResetTiltClicked = delegate {};
        public event Action<float> TiltChanged = delegate {};

        // Lens Damping.
        public event Action<float> FocusDistanceDampingChanged = delegate {};
        public event Action<float> FocalLengthDampingChanged = delegate {};
        public event Action<float> ApertureDampingChanged = delegate {};

        // Recording Countdown.
        public event Action<bool> CountdownEnableToggled = delegate {};
        public event Action<int> CountdownTimeChanged = delegate {};

        [SerializeField]
        SimpleButton m_DoneButton;

        // TimeCode
        [SerializeField]
        TMP_Dropdown m_TimecodeSourceDropdown;

        // Main View.
        [SerializeField]
        SlideToggle m_JoysticksToggle;
        [SerializeField]
        SlideToggle m_CameraSettingsToggle;
        [SerializeField]
        SlideToggle m_LensSettingsToggle;
        [SerializeField]
        SlideToggle m_InformationBarToggle;

        // Camera Options.
        [SerializeField]
        SlideToggle m_GateMaskToggle;
        [SerializeField]
        SlideToggle m_FrameLinesToggle;
        [SerializeField]
        SlideToggle m_CenterMarkerToggle;
        [SerializeField]
        SlideToggle m_FocusPlaneToggle;

        // Ergonomic Tilt.
        [SerializeField]
        SimpleButton m_SetToCurrentTiltButton;
        [SerializeField]
        SimpleButton m_ResetTiltButton;
        [SerializeField]
        TMP_InputField m_Tilt;

        // Lens Damping.
        [SerializeField]
        TMP_InputField m_FocusDistanceDamping;
        [SerializeField]
        TMP_InputField m_FocalLengthDamping;
        [SerializeField]
        TMP_InputField m_ApertureDamping;

        // Recording Countdown.
        [SerializeField]
        SlideToggle m_RecordingCountdownToggle;
        [SerializeField]
        TMP_InputField m_RecordingCountdownDuration;


        [SerializeField]
        Button m_PrivacyPolicy;

        [SerializeField]
        Button m_Documentation;

        [SerializeField]
        Button m_Support;

        IntInputWrapper m_RecordingCountDownDurationWrapper;
        FloatInputWrapper m_TiltWrapper;
        FloatInputWrapper m_FocusDistanceDampingWrapper;
        FloatInputWrapper m_FocalLengthDampingWrapper;
        FloatInputWrapper m_ApertureDampingWrapper;
        IReadOnlyList<ITimecodeSource> m_TimecodeSources;
        string m_SelectedTimecodeId;

        void Awake()
        {
            m_DoneButton.onClick += OnDoneClicked;

            // Timecode
            m_TimecodeSourceDropdown.onValueChanged.AddListener(OnTimecodeSourceChanged);

            // Main View.
            m_JoysticksToggle.onValueChanged.AddListener(OnJoysticksToggle);
            m_CameraSettingsToggle.onValueChanged.AddListener(OnCameraSettingsToggle);
            m_LensSettingsToggle.onValueChanged.AddListener(OnLensSettingsToggle);
            m_InformationBarToggle.onValueChanged.AddListener(OnInformationBarToggle);

            // Camera Options.
            m_GateMaskToggle.onValueChanged.AddListener(OnGateMaskToggle);
            m_FrameLinesToggle.onValueChanged.AddListener(OnFrameLinesToggle);
            m_CenterMarkerToggle.onValueChanged.AddListener(OnCenterMarkerToggle);
            m_FocusPlaneToggle.onValueChanged.AddListener(OnFocusPlaneToggle);

            // Ergonomic Tilt.
            m_SetToCurrentTiltButton.onClick += OnSetToCurrentTiltClicked;
            m_ResetTiltButton.onClick += OnResetTiltClicked;
            m_TiltWrapper = new FloatInputWrapper(m_Tilt);
            m_TiltWrapper.onValueChanged += OnTiltValueChanged;

            // Lens Damping.
            m_FocusDistanceDampingWrapper = new FloatInputWrapper(m_FocusDistanceDamping);
            m_FocalLengthDampingWrapper = new FloatInputWrapper(m_FocalLengthDamping);
            m_ApertureDampingWrapper = new FloatInputWrapper(m_ApertureDamping);

            m_FocusDistanceDampingWrapper.onValueChanged += OnFocusDistanceDampingChanged;
            m_FocalLengthDampingWrapper.onValueChanged += OnFocalLengthDampingChanged;
            m_ApertureDampingWrapper.onValueChanged += OnApertureDampingChanged;

            // Recording Countdown.
            m_RecordingCountdownToggle.onValueChanged.AddListener(OnRecordingEnabled);
            m_RecordingCountDownDurationWrapper = new IntInputWrapper(m_RecordingCountdownDuration);
            m_RecordingCountDownDurationWrapper.onValueChanged += OnRecordingTimeChanged;

            m_PrivacyPolicy.onClick.AddListener(OnPrivacyPolicyClick);
            m_Documentation.onClick.AddListener(OnDocumentationClick);
            m_Support.onClick.AddListener(OnSupportClick);

            TimecodeSourceManager.Instance.Added += OnTimecodeManagerChanged;
            TimecodeSourceManager.Instance.Removed += OnTimecodeManagerChanged;

            PrepareTimecodeDropdown();
        }

        void OnDestroy()
        {
            m_DoneButton.onClick -= OnDoneClicked;

            // Timecode
            m_TimecodeSourceDropdown.onValueChanged.RemoveListener(OnTimecodeSourceChanged);

            // Main View.
            m_JoysticksToggle.onValueChanged.RemoveListener(OnJoysticksToggle);
            m_CameraSettingsToggle.onValueChanged.RemoveListener(OnCameraSettingsToggle);
            m_LensSettingsToggle.onValueChanged.RemoveListener(OnLensSettingsToggle);
            m_InformationBarToggle.onValueChanged.RemoveListener(OnInformationBarToggle);

            // Camera Options.
            m_GateMaskToggle.onValueChanged.RemoveListener(OnGateMaskToggle);
            m_FrameLinesToggle.onValueChanged.RemoveListener(OnFrameLinesToggle);
            m_CenterMarkerToggle.onValueChanged.RemoveListener(OnCenterMarkerToggle);
            m_FocusPlaneToggle.onValueChanged.RemoveListener(OnFocusPlaneToggle);

            // Ergonomic Tilt.
            m_SetToCurrentTiltButton.onClick -= OnSetToCurrentTiltClicked;
            m_ResetTiltButton.onClick -= OnResetTiltClicked;
            m_TiltWrapper.onValueChanged -= OnTiltValueChanged;
            m_TiltWrapper.Dispose();

            // Lens Damping.
            m_FocusDistanceDampingWrapper.onValueChanged -= OnFocusDistanceDampingChanged;
            m_FocalLengthDampingWrapper.onValueChanged -= OnFocalLengthDampingChanged;
            m_ApertureDampingWrapper.onValueChanged -= OnApertureDampingChanged;
            m_FocusDistanceDampingWrapper.Dispose();
            m_FocalLengthDampingWrapper.Dispose();
            m_ApertureDampingWrapper.Dispose();

            // Recording Countdown.
            m_RecordingCountdownToggle.onValueChanged.RemoveAllListeners();
            m_RecordingCountDownDurationWrapper.onValueChanged -= OnRecordingTimeChanged;
            m_RecordingCountDownDurationWrapper.Dispose();

            m_PrivacyPolicy.onClick.RemoveListener(OnPrivacyPolicyClick);
            m_Documentation.onClick.RemoveListener(OnDocumentationClick);
            m_Support.onClick.RemoveListener(OnSupportClick);

            TimecodeSourceManager.Instance.Added -= OnTimecodeManagerChanged;
            TimecodeSourceManager.Instance.Removed -= OnTimecodeManagerChanged;
        }

        public void CollapseAllSections()
        {
            // Rarely called, so we don't cache the components collection.
            foreach (var container in GetComponentsInChildren<ExpandableContainer>())
            {
                container.Expanded = false;
            }
        }

        public void UpdateDeviceData(DeviceData deviceData)
        {
            SetOptions(deviceData.mainViewOptions);
            m_RecordingCountdownToggle.on = deviceData.isCountdownEnabled;
            m_RecordingCountDownDurationWrapper.SetValue(deviceData.countdownDuration);
        }

        public void SetOptions(MainViewOptions options)
        {
            m_JoysticksToggle.on = options.HasFlag(MainViewOptions.Joysticks);
            m_CameraSettingsToggle.on = options.HasFlag(MainViewOptions.CameraSettings);
            m_LensSettingsToggle.on = options.HasFlag(MainViewOptions.LensSettings);
            m_InformationBarToggle.on = options.HasFlag(MainViewOptions.InformationBar);
        }

        void OnTimecodeSourceChanged(int value)
        {
            m_SelectedTimecodeId = value > 0 ? m_TimecodeSources[value - 1].Id : string.Empty;

            TimecodeSourceChanged.Invoke(m_SelectedTimecodeId);
        }

        void OnTimecodeManagerChanged(ITimecodeSource source)
        {
            PrepareTimecodeDropdown();
        }

        void PrepareTimecodeDropdown()
        {
            m_TimecodeSources = TimecodeSourceManager.Instance.Entries;

            m_TimecodeSourceDropdown.options.Clear();
            m_TimecodeSourceDropdown.options.Add(new TMP_Dropdown.OptionData("None"));
            m_TimecodeSourceDropdown.options.AddRange(m_TimecodeSources
                .Select(s => new TMP_Dropdown.OptionData(s.FriendlyName)));

            var index = m_TimecodeSources.FindIndex(s => s.Id == m_SelectedTimecodeId);

            m_TimecodeSourceDropdown.value = index + 1;
        }

        void OnPrivacyPolicyClick()
        {
            PrivacyPolicyClicked.Invoke();
        }

        void OnDocumentationClick()
        {
            DocumentationClicked.Invoke();
        }

        void OnSupportClick()
        {
            SupportClicked.Invoke();
        }

        void OnDoneClicked() => DoneClicked.Invoke();

        void OnJoysticksToggle(bool on) => JoysticksToggled.Invoke(on);

        void OnCameraSettingsToggle(bool on) => CameraSettingsToggled.Invoke(on);

        void OnLensSettingsToggle(bool on) => LensSettingsToggled.Invoke(on);

        void OnInformationBarToggle(bool on) => InformationBarToggled.Invoke(on);

        void OnGateMaskToggle(bool on) => GateMaskToggled.Invoke(on);

        void OnFrameLinesToggle(bool on) => FrameLinesToggled.Invoke(on);

        void OnCenterMarkerToggle(bool on) => CenterMarkerToggled.Invoke(on);

        void OnFocusPlaneToggle(bool on) => FocusPlaneToggled.Invoke(on);

        void OnSetToCurrentTiltClicked() => SetToCurrentTiltClicked.Invoke();

        void OnResetTiltClicked() => ResetTiltClicked.Invoke();

        void OnTiltValueChanged(float value) => TiltChanged.Invoke(value);

        void OnFocusDistanceDampingChanged(float value) => FocusDistanceDampingChanged.Invoke(value);

        void OnFocalLengthDampingChanged(float value) => FocalLengthDampingChanged.Invoke(value);

        void OnApertureDampingChanged(float value) => ApertureDampingChanged.Invoke(value);

        void OnRecordingEnabled(bool on) => CountdownEnableToggled.Invoke(on);

        void OnRecordingTimeChanged(int value) => CountdownTimeChanged.Invoke(value);

        public void SetTilt(float value) => m_TiltWrapper.SetValue(value);

        public void SetFocusDistanceDamping(float value) => m_FocusDistanceDampingWrapper.SetValue(value);

        public void SetFocalLengthDamping(float value) => m_FocalLengthDampingWrapper.SetValue(value);

        public void SetApertureDamping(float value) => m_ApertureDampingWrapper.SetValue(value);

        public void SetGateMaskEnabled(bool on) => m_GateMaskToggle.on = on;

        public void SetFrameLinesEnabled(bool on) => m_FrameLinesToggle.on = on;

        public void SetCenterMarkerEnabled(bool on) => m_CenterMarkerToggle.on = on;

        public void SetFocusPlaneEnabled(bool on) => m_FocusPlaneToggle.on = on;
    }
}
