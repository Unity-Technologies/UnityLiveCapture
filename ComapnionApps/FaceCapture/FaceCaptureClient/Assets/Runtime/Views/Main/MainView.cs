using System;
using UnityEngine;
using Unity.CompanionAppCommon;
using UnityEngine.UI;
using Unity.LiveCapture;
using TMPro;

namespace Unity.CompanionApps.FaceCapture
{
    [RequireComponent(typeof(CanvasGroup))]
    class MainView : DialogView
        , IMainView
        , ITimeListener
        , ISettingsPropertyListener
        , ISlateTakeNumberListener
    {
        public event Action<bool> VisibilityChanged = delegate {};
        public event Action SettingsButtonClicked = delegate {};
        public event Action TakesButtonClicked = delegate {};
        public event Action TrackingButtonClicked = delegate {};
        public event Action RecordButtonClicked = delegate {};
        public event Action<bool> HelpToggleChanged = delegate {};

        [SerializeField]
        Button m_SettingsButton;

        [SerializeField]
        Button m_RecordButton;

        [SerializeField]
        Toggle m_HelpToggle;

        [SerializeField]
        TMP_Text m_TakeNumberLabel;

        [SerializeField]
        TimecodeView m_TimecodeView;

        [SerializeField]
        Image m_TrackingIndicator;

        [SerializeField]
        Button m_TrackingButton;

        [SerializeField]
        Button m_TakesButton;

        [SerializeField]
        Color m_RecordingColor = new Color(0.93f, 0.46f, 0.3f, 1f);

        [SerializeField]
        Sprite m_StartRecordingSprite;

        [SerializeField]
        Sprite m_StopRecordingSprite;

        [SerializeField]
        CanvasGroup m_RecordButtonCanvasgroup;

        [SerializeField]
        Color m_TrackingActiveColor = new Color(0f, 0f, 0f, 0f);

        [SerializeField]
        Color m_TrackingInactiveColor = new Color(0.93f, 0.46f, 0.3f, 1f);

        [SerializeField]
        Sprite m_TrackingActiveSprite;

        [SerializeField]
        Sprite m_TrachingInactiveSprite;

        Image m_RecordButtonIcon;
        Image m_TimecodeBackground;
        Image m_TrackingIndicatorIcon;
        bool m_IsRecording;
        double m_Time;
        double m_Duration;
        FrameRate m_FrameRate;
        CanvasGroup m_CanvasGroup;
        bool m_ShowRecordButtonSetting;

        public float Alpha
        {
            get => m_CanvasGroup.alpha;
            set => m_CanvasGroup.alpha = value;
        }

        public bool Interactable
        {
            get => m_CanvasGroup.interactable;
            set => m_CanvasGroup.interactable = value;
        }

        public float RecordButtonOpacity
        {
            set => m_RecordButtonCanvasgroup.alpha = value;
        }

        public bool RecordButtonActive
        {
            set => m_RecordButton.gameObject.SetActive(value && m_ShowRecordButtonSetting);
        }

        public bool BlocksRaycasts
        {
            get => m_CanvasGroup.blocksRaycasts;
            set => m_CanvasGroup.blocksRaycasts = value;
        }

        void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_SettingsButton.onClick.AddListener(OnSettingsButtonClick);
            m_RecordButton.onClick.AddListener(OnRecordButtonClick);
            m_HelpToggle.onValueChanged.AddListener(OnHelpToggleChanged);
            m_TrackingButton.onClick.AddListener(OnTrackingClicked);
            m_TakesButton.onClick.AddListener(OnTakesClicked);

            m_RecordButtonIcon = m_RecordButton.GetComponentInChildren<Image>();
            m_TimecodeBackground = m_TimecodeView.GetComponent<Image>();
            m_TrackingIndicatorIcon = m_TrackingIndicator.transform.Find("Icon").GetComponent<Image>();

            UpdateTimecodeView();
            UpdateRecordButonIcon();
        }

        void OnDestroy()
        {
            m_SettingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            m_RecordButton.onClick.RemoveListener(OnRecordButtonClick);
            m_HelpToggle.onValueChanged.RemoveListener(OnHelpToggleChanged);
            m_TrackingButton.onClick.RemoveListener(OnTrackingClicked);
            m_TakesButton.onClick.RemoveListener(OnTakesClicked);
        }

        void OnRecordButtonClick()
        {
            RecordButtonClicked.Invoke();
        }

        void OnSettingsButtonClick()
        {
            SettingsButtonClicked.Invoke();
        }

        void OnHelpToggleChanged(bool value)
        {
            HelpToggleChanged.Invoke(value);
        }

        void OnTrackingClicked()
        {
            TrackingButtonClicked.Invoke();
        }

        void OnTakesClicked()
        {
            TakesButtonClicked.Invoke();
        }

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            m_Time = time;
            m_Duration = duration;
            m_FrameRate = frameRate;

            UpdateTimecodeView();
        }

        public void SetRecordingState(bool isRecording)
        {
            m_IsRecording = isRecording;

            UpdateTimecodeView();
            UpdateRecordButonIcon();
        }

        public void SetTrackingState(bool value)
        {
            var sprite = m_TrachingInactiveSprite;
            var color = m_TrackingInactiveColor;

            if (value)
            {
                sprite = m_TrackingActiveSprite;
                color = m_TrackingActiveColor;
            }

            m_TrackingIndicatorIcon.sprite = sprite;
            m_TrackingIndicator.color = color;
        }

        public void SettingsPropertyChanged(SettingsProperty property, ISettings settings)
        {
            if (property == SettingsProperty.ShowRecordButton)
            {
                m_ShowRecordButtonSetting = settings.ShowRecordButton;
                m_RecordButton.gameObject.SetActive(m_ShowRecordButtonSetting);
            }
        }

        public void SetSlateTakeNumber(int takeNumber)
        {
            m_TakeNumberLabel.text = takeNumber.ToString();
        }

        void UpdateTimecodeView()
        {
            var time = m_Time;
            var color = m_RecordingColor;

            if (!m_IsRecording)
            {
                time = 0d;
                color = Color.black;
            }

            m_TimecodeView.SetTime(m_Time, m_Duration, m_FrameRate);
            m_TimecodeBackground.color = color;
        }

        void UpdateRecordButonIcon()
        {
            m_RecordButtonIcon.sprite = m_IsRecording ? m_StopRecordingSprite : m_StartRecordingSprite;
        }

        protected override void OnShowChanged()
        {
            VisibilityChanged.Invoke(IsShown);
        }
    }
}
