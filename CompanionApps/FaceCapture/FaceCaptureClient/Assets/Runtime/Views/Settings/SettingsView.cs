using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;
using TMPro;

namespace Unity.CompanionApps.FaceCapture
{
    class SettingsView : DialogView, ISettingsView
    {
        public event Action DoneButtonClicked = delegate {};
        public event Action ResetButtonClicked = delegate {};
        public event Action PrivacyPolicyClicked = delegate {};
        public event Action DocumentationClicked = delegate {};
        public event Action SupportClicked = delegate {};
        public event Action<bool> AutoHideUIChanged = delegate {};
        public event Action<bool> CameraPassthroughChanged = delegate {};
        public event Action<bool> DimScreenChanged = delegate {};
        public event Action<bool> FlipHorizontallyChanged = delegate {};
        public event Action<bool> RecordAudioChanged = delegate {};
        public event Action<bool> RecordVideoChanged = delegate {};
        public event Action<bool> RecordingCountdownChanged = delegate {};
        public event Action<bool> ShowFaceChanged = delegate {};
        public event Action<bool> ShowRecordButtonChanged = delegate {};
        public event Action<int> RecordingCountdownValueChanged = delegate {};
        public event Action<string> TimecodeSourceChanged = delegate {};

        [SerializeField]
        SimpleButton m_DoneButton;

        [SerializeField]
        TMP_Dropdown m_TimecodeSourceDropdown;

        [SerializeField]
        SlideToggle m_FlipHorizontally;

        [SerializeField]
        SlideToggle m_ShowFaceToggle;

        [SerializeField]
        SlideToggle m_ShowRecordButtonToggle;

        [SerializeField]
        SlideToggle m_DimScreenToggle;

        [SerializeField]
        SlideToggle m_RecordingCountdownToggle;

        [SerializeField]
        SlideToggle m_CameraPassthroughToggle;

        [SerializeField]
        SlideToggle m_AutoHideUIToggle;

        [SerializeField]
        TMP_InputField m_RecordingCountdownTimeInputField;

        [SerializeField]
        GameObject m_CountdownTimeRow;

        [SerializeField]
        Button m_ResetButton;

        [SerializeField]
        SlideToggle m_RecordVideoUIToggle;

        [SerializeField]
        SlideToggle m_RecordAudioUIToggle;

        [SerializeField]
        Button m_PrivacyPolicy;

        [SerializeField]
        Button m_Documentation;

        [SerializeField]
        Button m_Support;

        IReadOnlyList<ITimecodeSource> m_TimecodeSources;
        string m_SelectedTimecodeId;

        void Awake()
        {
            m_DoneButton.onClick += OnDoneButtonClick;
            m_ResetButton.onClick.AddListener(OnResetButtonClick);
            m_PrivacyPolicy.onClick.AddListener(OnPrivacyPolicyClick);
            m_Documentation.onClick.AddListener(OnDocumentationClick);
            m_Support.onClick.AddListener(OnSupportClick);
            m_AutoHideUIToggle.onValueChanged.AddListener(OnAutoHideUIChanged);
            m_CameraPassthroughToggle.onValueChanged.AddListener(OnCameraPassthroughChanged);
            m_DimScreenToggle.onValueChanged.AddListener(OnDimScreenChanged);
            m_FlipHorizontally.onValueChanged.AddListener(OnFlipHorizontallyChanged);
            m_RecordAudioUIToggle.onValueChanged.AddListener(OnRecordAudioChanged);
            m_RecordVideoUIToggle.onValueChanged.AddListener(OnRecordVideoChanged);
            m_RecordingCountdownToggle.onValueChanged.AddListener(OnRecordingCountdownChanged);
            m_RecordingCountdownTimeInputField.onEndEdit.AddListener(OnRecordingCountdownValueChanged);
            m_ShowFaceToggle.onValueChanged.AddListener(OnShowFaceChanged);
            m_ShowRecordButtonToggle.onValueChanged.AddListener(OnShowRecordButtonChanged);
            m_TimecodeSourceDropdown.onValueChanged.AddListener(OnTimecodeSourceChanged);

            TimecodeSourceManager.Instance.Added += OnTimecodeManagerChanged;
            TimecodeSourceManager.Instance.Removed += OnTimecodeManagerChanged;

            PrepareTimecodeDropdown();
        }

        void OnDestroy()
        {
            m_DoneButton.onClick -= OnDoneButtonClick;
            m_PrivacyPolicy.onClick.RemoveListener(OnPrivacyPolicyClick);
            m_Documentation.onClick.RemoveListener(OnDocumentationClick);
            m_Support.onClick.RemoveListener(OnSupportClick);
            m_ResetButton.onClick.RemoveListener(OnResetButtonClick);
            m_AutoHideUIToggle.onValueChanged.RemoveListener(OnAutoHideUIChanged);
            m_CameraPassthroughToggle.onValueChanged.RemoveListener(OnCameraPassthroughChanged);
            m_DimScreenToggle.onValueChanged.RemoveListener(OnDimScreenChanged);
            m_FlipHorizontally.onValueChanged.RemoveListener(OnFlipHorizontallyChanged);
            m_RecordAudioUIToggle.onValueChanged.RemoveListener(OnRecordAudioChanged);
            m_RecordVideoUIToggle.onValueChanged.RemoveListener(OnRecordVideoChanged);
            m_RecordingCountdownToggle.onValueChanged.RemoveListener(OnRecordingCountdownChanged);
            m_RecordingCountdownTimeInputField.onEndEdit.RemoveListener(OnRecordingCountdownValueChanged);
            m_ShowFaceToggle.onValueChanged.RemoveListener(OnShowFaceChanged);
            m_ShowRecordButtonToggle.onValueChanged.RemoveListener(OnShowRecordButtonChanged);
            m_TimecodeSourceDropdown.onValueChanged.RemoveListener(OnTimecodeSourceChanged);

            TimecodeSourceManager.Instance.Added -= OnTimecodeManagerChanged;
            TimecodeSourceManager.Instance.Removed -= OnTimecodeManagerChanged;
        }

        public void SettingsPropertyChanged(SettingsProperty property, ISettings settings)
        {
            m_AutoHideUIToggle.on = settings.AutoHideUI;
            m_CameraPassthroughToggle.on = settings.CameraPassthrough;
            m_DimScreenToggle.on = settings.DimScreen;
            m_FlipHorizontally.on = settings.FlipHorizontally;
            m_RecordAudioUIToggle.on = settings.RecordAudio;
            m_RecordVideoUIToggle.on = settings.RecordVideo;
            m_RecordingCountdownToggle.on = settings.CountdownEnabled;
            m_ShowFaceToggle.on = settings.FaceWireframe;
            m_ShowRecordButtonToggle.on = settings.ShowRecordButton;
            m_RecordingCountdownTimeInputField.SetTextWithoutNotify(settings.CountdownTime.ToString());
            m_CountdownTimeRow.SetActive(settings.CountdownEnabled);
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

        void OnDoneButtonClick()
        {
            DoneButtonClicked.Invoke();
        }

        void OnResetButtonClick()
        {
            ResetButtonClicked.Invoke();
        }

        void OnAutoHideUIChanged(bool value)
        {
            AutoHideUIChanged.Invoke(value);
        }

        void OnCameraPassthroughChanged(bool value)
        {
            CameraPassthroughChanged.Invoke(value);
        }

        void OnDimScreenChanged(bool value)
        {
            DimScreenChanged.Invoke(value);
        }

        void OnFlipHorizontallyChanged(bool value)
        {
            FlipHorizontallyChanged.Invoke(value);
        }

        void OnRecordAudioChanged(bool value)
        {
            RecordAudioChanged.Invoke(value);
        }

        void OnRecordVideoChanged(bool value)
        {
            RecordVideoChanged.Invoke(value);
        }

        void OnRecordingCountdownChanged(bool value)
        {
            RecordingCountdownChanged.Invoke(value);
        }

        void OnRecordingCountdownValueChanged(string value)
        {
            if (int.TryParse(value, out var intValue))
            {
                RecordingCountdownValueChanged.Invoke(intValue);
            }
        }

        void OnShowFaceChanged(bool value)
        {
            ShowFaceChanged.Invoke(value);
        }

        void OnShowRecordButtonChanged(bool value)
        {
            ShowRecordButtonChanged.Invoke(value);
        }

        void OnTimecodeSourceChanged(int value)
        {
            m_SelectedTimecodeId = value > 0 ? m_TimecodeSources[value - 1].Id : string.Empty;

            TimecodeSourceChanged.Invoke(m_SelectedTimecodeId);
        }
    }
}
