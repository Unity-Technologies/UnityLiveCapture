using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class TimeControlView : DialogView, ITimeControlView
    {
        [SerializeField]
        CanvasGroup m_Group;
        [SerializeField]
        Button m_PlayButton;
        [SerializeField]
        Slider2 m_Slider;
        [SerializeField]
        Button m_PrevFrameButton;
        [SerializeField]
        Button m_PrevTenFramesButton;
        [SerializeField]
        Button m_NextFrameButton;
        [SerializeField]
        Button m_NextTenFramesButton;
        [SerializeField]
        TimecodeView m_TimecodeView;
        [SerializeField]
        TimecodeView m_TimecodePopupView;
        [SerializeField]
        Sprite m_PlaySprite;
        [SerializeField]
        Sprite m_PauseSprite;
        Image m_PlayButtonImage;
        FrameRate m_FrameRate;

        public event Action<float> ValueChanged = delegate {};
        public event Action PlayButtonClicked = delegate {};
        public event Action<int> FrameSkipButtonClicked = delegate {};

        void Awake()
        {
            m_PlayButton.onClick.AddListener(OnPlayButtonClick);
            m_Slider.onValueChanged.AddListener(OnSliderValueChanged);
            m_Slider.onPointerDown += OnSliderPointerDown;
            m_Slider.onPointerUp += OnSliderPointerUp;
            m_PrevFrameButton.onClick.AddListener(OnPrevFrameButtonClick);
            m_PrevTenFramesButton.onClick.AddListener(OnPrevTenFramesButtonClick);
            m_NextFrameButton.onClick.AddListener(OnNextButtonClick);
            m_NextTenFramesButton.onClick.AddListener(OnNextTenFramesButtonClick);

            m_PlayButtonImage = m_PlayButton.transform.Find("Icon").GetComponent<Image>();

            ShowPopup(false);
        }

        protected override void OnInteractableChanged()
        {
            m_Group.alpha = IsInteractable ? 1.0f : 0.5f;
        }

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            Debug.Assert(m_Slider != null);

            m_Slider.minValue = 0f;
            m_Slider.maxValue = (float)duration;
            m_Slider.SetValueWithoutNotify((float)time);

            m_TimecodeView.SetTime(duration, duration, frameRate);
            m_TimecodePopupView.SetTime(time, duration, frameRate);

            m_FrameRate = frameRate;
        }

        void OnPlayButtonClick()
        {
            PlayButtonClicked.Invoke();
        }

        void OnSliderValueChanged(float value)
        {
            ValueChanged.Invoke(value);

            m_TimecodePopupView.SetTime(value, m_Slider.maxValue, m_FrameRate);
        }

        void OnSliderPointerDown()
        {
            ShowPopup(true);
        }

        void OnSliderPointerUp()
        {
            ShowPopup(false);
        }

        void OnPrevFrameButtonClick()
        {
            FrameSkipButtonClicked.Invoke(-1);
        }

        void OnPrevTenFramesButtonClick()
        {
            FrameSkipButtonClicked.Invoke(-10);
        }

        void OnNextButtonClick()
        {
            FrameSkipButtonClicked.Invoke(1);
        }

        void OnNextTenFramesButtonClick()
        {
            FrameSkipButtonClicked.Invoke(10);
        }

        public void SetPlayState(bool value)
        {
            m_PlayButtonImage.sprite = value ? m_PauseSprite : m_PlaySprite;
        }

        void ShowPopup(bool value)
        {
            m_TimecodePopupView.transform.parent.gameObject.SetActive(value);
        }
    }
}
