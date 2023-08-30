using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;

namespace Unity.CompanionApps.VirtualCamera
{
    class MainViewMobile : DialogView, IMainViewMobile
    {
        const int k_RightMargin = 12;

        public event Action LensSettingsClicked = delegate {};
        public event Action RigSettingsClicked = delegate {};
        public event Action JoystickSettingsClicked = delegate {};
        public event Action RecordClicked = delegate {};
        public event Action<bool> TakeIterationToggled = delegate {};
        public event Action SettingsClicked = delegate {};
        public event Action<bool> DeviceModeToggled = delegate {};
        public event Action<bool> HelpToggled = delegate {};
        public event Action<bool> ResetMenuToggled = delegate {};
        public event Action<bool> ShowTakesView = delegate {};

        [SerializeField]
        Button m_CameraLensSettings;
        [SerializeField]
        Button m_CameraLensSettingsRecording;
        [SerializeField]
        Button m_CameraRigSettings;
        [SerializeField]
        Button m_JoystickSettings;
        [SerializeField]
        SpriteSwapToggle m_DeviceModeToggle;
        [SerializeField]
        GameObject m_TopToolBar;
        [SerializeField]
        GameObject m_RightToolBar;
        [SerializeField]
        LayoutGroup m_FooterLayout;
        [SerializeField]
        RectTransform m_LensViewContainer;
        [SerializeField]
        Button m_Recordbutton;
        [SerializeField]
        Image m_RecordImage;
        [SerializeField]
        Button m_Settings;
        [SerializeField]
        Toggle m_HelpToggle;
        [SerializeField]
        Toggle m_ResetPoseMenuToggle;
        [SerializeField]
        TakeIterationToggle m_TakeIterationToggle;
        [SerializeField]
        TimecodeView m_TimeHudView;
        [SerializeField]
        JoysticksView m_JoysticksView;
        [SerializeField]
        TimeControlView m_TimeControlView;
        [SerializeField]
        Sprite m_StopRecordingSprite;
        [SerializeField]
        Sprite m_StartRecordingSprite;
        [SerializeField]
        Button m_ShowTakesButton;
        [SerializeField]
        Sprite m_JoystickSprite;
        [SerializeField]
        Sprite m_GamepadSprite;
        [SerializeField]
        Image m_InputButtonImage;

        DeviceMode m_DeviceMode;
        bool m_IsRecording;
        bool m_LensViewIsOpen;
        MainViewOptions m_Options;

        // TODO unsubscribe in Destroy
        void Awake()
        {
            m_CameraLensSettings.onClick.AddListener(OnCameraLensSettingsClicked);
            m_HelpToggle.onValueChanged.AddListener(OnHelpValueChanged);
            m_CameraRigSettings.onClick.AddListener(OnCameraRigSettingsClicked);
            m_JoystickSettings.onClick.AddListener(OnJoystickSettingsClicked);
            m_ResetPoseMenuToggle.onValueChanged.AddListener(OnResetMenuToggled);
            m_Recordbutton.onClick.AddListener(OnRecordClicked);
            m_CameraLensSettingsRecording.onClick.AddListener(OnCameraLensSettingsClicked);
            m_Settings.onClick.AddListener(OnSettingsClicked);
            m_DeviceModeToggle.onValueChanged.AddListener(OnDeviceModeChanged);
            m_ShowTakesButton.onClick.AddListener(OnShowTakesClicked);
            m_TakeIterationToggle.onValueChanged.AddListener(OnTakeIterationToggled);

            SetHelpMode(false);
            UpdateLayout();
        }

        public bool TryGetPosition(MainViewId label, out Vector3 position, out Vector2 pivot)
        {
            position = Vector2.zero;
            pivot = Vector2.one * 0.5f;

            switch (label)
            {
                case MainViewId.Reset:
                    pivot = new Vector2(1f, 0.5f);
                    position = m_ResetPoseMenuToggle.transform.TransformPoint(new Vector3(-36f, 0f));
                    break;
            }

            return false;
        }

        public bool TryGetSize(MainViewId label, out Vector2 size)
        {
            size = Vector2.one * 100;

            switch (label)
            {
                case MainViewId.Reset:
                    size = new Vector2(173, 80f);
                    break;
            }

            return false;
        }

        public void SetChannelFlags(VirtualCameraChannelFlags channelFlags)
        {
            m_TakeIterationToggle.IsLocked = !channelFlags.AreAllChannelsActive();
        }

        public void SetOptions(MainViewOptions options)
        {
            m_Options = options;

            UpdateLayout();
        }

        public void SetDeviceMode(DeviceMode mode)
        {
            m_DeviceMode = mode;

            UpdateLayout();
        }

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            m_TimeHudView.SetTime(time, duration, frameRate);

            if (duration > 0f)
            {
                m_TimeControlView.Show();
            }
            else
            {
                m_TimeControlView.Hide();
            }
        }

        public void SetRecordingState(bool isRecording)
        {
            m_IsRecording = isRecording;

            UpdateLayout();
        }

        public void SetGamepadState(bool connected)
        {
            m_InputButtonImage.sprite = connected ? m_GamepadSprite : m_JoystickSprite;
        }

        public void SetLensViewOpenState(bool isOpen)
        {
            m_LensViewIsOpen = isOpen;

            UpdateLayout();
        }

        public void SetHelpMode(bool value)
        {
            SetMenuTogglesOffWithoutNotify();

            m_HelpToggle.SetIsOnWithoutNotify(value);
        }

        public void SetMenuTogglesOffWithoutNotify()
        {
            m_ResetPoseMenuToggle.SetIsOnWithoutNotify(false);
        }

        void OnSettingsClicked() => SettingsClicked.Invoke();

        void OnDeviceModeChanged(bool value) => DeviceModeToggled.Invoke(value);

        void OnHelpValueChanged(bool value) => HelpToggled.Invoke(value);

        void OnCameraLensSettingsClicked() => LensSettingsClicked.Invoke();

        void OnCameraRigSettingsClicked() => RigSettingsClicked.Invoke();

        void OnJoystickSettingsClicked() => JoystickSettingsClicked.Invoke();

        void OnResetMenuToggled(bool value) => ResetMenuToggled.Invoke(value);

        void OnTakeIterationToggled(bool value) => TakeIterationToggled.Invoke(value);

        void OnRecordClicked() => RecordClicked.Invoke();

        void OnShowTakesClicked() => ShowTakesView.Invoke(true);

        void UpdateLayout()
        {
            var isLive = m_DeviceMode == DeviceMode.LiveStream;

            m_ShowTakesButton.gameObject.SetActive(m_DeviceMode == DeviceMode.Playback);
            m_TimeHudView.gameObject.SetActive(m_IsRecording);
            m_TimeControlView.SetInteractable(!m_IsRecording);
            m_TopToolBar.SetActive(isLive && !m_IsRecording && !m_LensViewIsOpen);
            m_RightToolBar.SetActive(isLive && !m_IsRecording && !m_LensViewIsOpen);
            m_DeviceModeToggle.gameObject.SetActive(!m_LensViewIsOpen && !m_IsRecording);
            m_DeviceModeToggle.SetValueWithoutNotify(!isLive);
            m_CameraLensSettingsRecording.gameObject.SetActive(m_IsRecording && !m_LensViewIsOpen);
            m_Recordbutton.gameObject.SetActive(isLive);

            if (isLive && m_Options.HasFlag(MainViewOptions.Joysticks))
                m_JoysticksView.Show();
            else
                m_JoysticksView.Hide();

            m_TakeIterationToggle.gameObject.SetActive(isLive && (!m_IsRecording || m_TakeIterationToggle.IsLocked));
            m_TakeIterationToggle.Interactable = !m_IsRecording;

            AdjustFooterPadding();

            var sprite = m_StartRecordingSprite;

            if (m_IsRecording)
            {
                sprite = m_StopRecordingSprite;
            }

            m_RecordImage.sprite = sprite;

            LayoutRebuilder.MarkLayoutForRebuild(m_LensViewContainer.GetComponent<RectTransform>());
        }

        void AdjustFooterPadding()
        {
            var footerRectTransform = m_FooterLayout.transform as RectTransform;
            var footerParentRectTransform = footerRectTransform.parent as RectTransform;
            var rectMin = m_LensViewContainer.rect.min;

            rectMin = m_LensViewContainer.TransformPoint(rectMin);
            rectMin = footerRectTransform.parent.InverseTransformPoint(rectMin);

            var width = footerParentRectTransform.rect.xMax - rectMin.x;
            var padding = m_FooterLayout.padding;

            padding.right = m_LensViewIsOpen ? (int)width : k_RightMargin;

            m_FooterLayout.padding = padding;
        }
    }
}
