using System;
using UnityEngine;
using Unity.CompanionAppCommon;
using UnityEngine.UI;
using TMPro;
using Unity.TouchFramework;

namespace Unity.CompanionApps.FaceCapture
{
    [RequireComponent(typeof(CanvasGroup))]
    class CalibrationView : DialogView, ICalibrationView
    {
        public event Action<ICalibrationView.State> StateChanged = delegate {};
        public event Action CalibrateButtonClicked = delegate {};
        public event Action RecordButtonClicked = delegate {};
        public event Action CalibrateMenuButtonClicked = delegate {};
        public event Action ClearMenuButtonClicked = delegate {};
        public event Action CancelButtonClicked = delegate {};
        public event Action DoneButtonClicked = delegate {};

        [SerializeField]
        Button m_CalibrateButton;

        [SerializeField]
        Button m_RecordButton;

        [SerializeField]
        SimpleButton m_CancelButton;

        [SerializeField]
        SimpleButton m_DoneButton;

        [SerializeField]
        Notification m_Notification;

        [SerializeField]
        CanvasGroup m_MenuGroup;

        [SerializeField]
        Button m_CalibrateMenuButton;

        [SerializeField]
        Button m_ClearMenuButton;

        [SerializeField]
        Sprite m_CalibrateSprite;

        [SerializeField]
        Sprite m_CancelSprite;

        [SerializeField]
        CanvasGroup m_ReviewGroup;

        CanvasGroup m_CanvasGroup;
        Image m_CalibrateButtonIcon;
        TMP_Text m_CalibrateMenuButtonLabel;
        bool m_IsRecording;
        string m_NotificationText;

        ICalibrationView.State m_State = ICalibrationView.State.MenuClosed;

        public ICalibrationView.State ViewState
        {
            get => m_State;
            set
            {
                if (m_State != value)
                {
                    m_State = value;

                    switch (m_State)
                    {
                        case ICalibrationView.State.MenuClosed:
                            SetMenuClosedLayout();
                            break;

                        case ICalibrationView.State.MenuOpen:
                            SetMenuOpenLayout();
                            break;

                        case ICalibrationView.State.Calibrating:
                            SetCalibratingLayout();
                            break;

                        case ICalibrationView.State.Reviewing:
                            SetReviewingLayout();
                            break;
                    }

                    StateChanged.Invoke(m_State);
                }
            }
        }

        bool m_IsCalibrated;

        public bool IsCalibrated
        {
            set
            {
                if (value != m_IsCalibrated)
                {
                    m_IsCalibrated = value;
                    UpdateMenu();
                }
            }
        }

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

        public bool BlocksRaycasts
        {
            get => m_CanvasGroup.blocksRaycasts;
            set => m_CanvasGroup.blocksRaycasts = value;
        }

        void Awake()
        {
            m_CalibrateButton.onClick.AddListener(OnCalibrateButtonClick);
            m_RecordButton.onClick.AddListener(OnRecordButtonClick);

            m_CalibrateMenuButton.onClick.AddListener(OnCalibrateMenuButtonClick);
            m_ClearMenuButton.onClick.AddListener(OnClearMenuButtonClick);

            m_CancelButton.onClick += OnCancelButtonClicked;
            m_DoneButton.onClick += OnDoneButtonClicked;

            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_CalibrateButtonIcon = m_CalibrateButton.GetComponentInChildren<Image>();
            m_CalibrateMenuButtonLabel = m_CalibrateMenuButton.GetComponentInChildren<TMP_Text>();

            m_NotificationText = m_Notification.GetComponentInChildren<TMP_Text>().text;

            UpdateMenu();
            SetMenuClosedLayout();
        }

        void OnDestroy()
        {
            m_CalibrateButton.onClick.RemoveListener(OnCalibrateButtonClick);
            m_RecordButton.onClick.RemoveListener(OnRecordButtonClick);

            m_CalibrateMenuButton.onClick.RemoveListener(OnCalibrateMenuButtonClick);
            m_ClearMenuButton.onClick.RemoveListener(OnClearMenuButtonClick);

            m_CancelButton.onClick -= OnCancelButtonClicked;
            m_DoneButton.onClick -= OnDoneButtonClicked;
        }

        void OnCalibrateButtonClick()
        {
            CalibrateButtonClicked.Invoke();
        }

        void OnRecordButtonClick()
        {
            RecordButtonClicked.Invoke();
        }

        void OnCalibrateMenuButtonClick()
        {
            CalibrateMenuButtonClicked.Invoke();
        }

        void OnClearMenuButtonClick()
        {
            ClearMenuButtonClicked.Invoke();
        }

        void OnCancelButtonClicked()
        {
            CancelButtonClicked.Invoke();
        }

        void OnDoneButtonClicked()
        {
            DoneButtonClicked.Invoke();
        }

        public void SetRecordingState(bool isRecording)
        {
            m_IsRecording = isRecording;

            if (m_IsRecording)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        void SetMenuClosedLayout()
        {
            m_RecordButton.gameObject.SetActive(false);
            m_CalibrateButton.gameObject.SetActive(true);

            m_Notification.HideImmediate();

            m_CalibrateButtonIcon.sprite = m_CalibrateSprite;

            m_MenuGroup.alpha = 0f;
            m_MenuGroup.interactable = m_MenuGroup.blocksRaycasts = false;

            m_ReviewGroup.alpha = 0f;
            m_ReviewGroup.interactable = m_ReviewGroup.blocksRaycasts = false;
        }

        void SetMenuOpenLayout()
        {
            SetMenuClosedLayout();

            m_MenuGroup.alpha = 1f;
            m_MenuGroup.interactable = m_MenuGroup.blocksRaycasts = true;
        }

        void SetCalibratingLayout()
        {
            m_RecordButton.gameObject.SetActive(true);
            m_CalibrateButton.gameObject.SetActive(true);

            var displayData = m_Notification.DefaultData();
            displayData.text = m_NotificationText;
            displayData.displayDuration = null;
            m_Notification.Display(displayData);

            m_CalibrateButtonIcon.sprite = m_CancelSprite;

            m_MenuGroup.alpha = 0f;
            m_MenuGroup.interactable = m_MenuGroup.blocksRaycasts = false;

            m_ReviewGroup.alpha = 0f;
            m_ReviewGroup.interactable = m_ReviewGroup.blocksRaycasts = false;
        }

        void SetReviewingLayout()
        {
            SetMenuClosedLayout();

            m_CalibrateButton.gameObject.SetActive(false);

            m_ReviewGroup.alpha = 1f;
            m_ReviewGroup.interactable = m_ReviewGroup.blocksRaycasts = true;
        }

        void UpdateMenu()
        {
            m_CalibrateMenuButtonLabel.text = m_IsCalibrated
                ? "Recalibrate"
                : "Calibrate";

            m_ClearMenuButton.interactable = m_IsCalibrated;
        }
    }
}
