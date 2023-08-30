using System;
using System.Linq;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    public enum MainViewId : int
    {
        None = 0,
        Reset,
        RigSettings,
        FocalLength,
        Focus,
        FocusMode,
        Settings,
        TakeIteration,
        JoystickSettings,
        GamepadSettings
    }

    interface IMainView : IDialogView,
        ITimeListener,
        IRecordingStateListener
    {
        event Action RecordClicked;
        event Action<bool> ShowTakesView;

        bool TryGetPosition(MainViewId label, out Vector3 position, out Vector2 pivot);
        bool TryGetSize(MainViewId label, out Vector2 size);

        void SetOptions(MainViewOptions options);
    }

    interface IMainViewTablet : IMainView,
        ISensorSizeListener,
        IFocalLengthListener,
        IApertureListener,
        IFocusDistanceListener,
        IFocusModeListener,
        ILensIntrinsicsListener,
        ISlateShotNameListener,
        ISlateTakeNumberListener,
        IChannelFlagsListener
    {
        event Action<MainViewId, bool> Toggled;

        event Action<DeviceMode> deviceModeToggled;

        event Action<bool> ToggledHelpMode;

        MainViewId ActiveToggle { get; set; }

        void SetGamepadState(bool connected);
    }

    class MainView : DialogView, IMainViewTablet, IDeviceModeListener
    {
        public event Action<MainViewId, bool> Toggled = delegate {};
        public event Action<bool> ShowTakesView = delegate {};
        public event Action<bool> ToggledHelpMode = delegate {};
        public event Action RecordClicked = delegate {};
        public event Action<DeviceMode> deviceModeToggled = delegate {};

        [SerializeField]
        Button m_RecordButton;
        [SerializeField]
        Toggle m_ResetToggle;
        [SerializeField]
        Toggle m_RigSettingsToggle;
        [SerializeField]
        Toggle m_JoystickSettingsToggle;
        [SerializeField]
        Toggle m_HelpToggle;
        [SerializeField]
        Toggle m_FocalDialToggle;
        [SerializeField]
        Toggle m_FocusApertureToggle;
        [SerializeField]
        Toggle m_FocusModeToggle;
        [SerializeField]
        Toggle m_SettingsToggle;
        [SerializeField]
        SpriteSwapToggle m_PlaybackToggle;
        [SerializeField]
        TakeIterationToggle m_TakeIterationToggle;
        [SerializeField]
        RectTransform m_RigSettingsContainer;
        [SerializeField]
        RectTransform m_JoystickSettingsContainer;
        [SerializeField]
        RectTransform m_GamepadSettingsContainer;
        [SerializeField]
        RectTransform m_SettingsContainer;
        [SerializeField]
        RectTransform m_TakeIterationContainer;
        [SerializeField]
        HudView m_HudView;
        [SerializeField]
        TimeControlView m_TimeControlView;
        [SerializeField]
        JoysticksView m_JoysticksView;
        [SerializeField]
        RectTransform m_CameraSettingsTransform;
        [SerializeField]
        RectTransform m_LensSettingsTransform;
        [SerializeField]
        Sprite m_StartRecordingSprite;
        [SerializeField]
        Sprite m_StopRecordingSprite;
        [SerializeField]
        ColorTintToggle m_ShowTakesToggle;
        [SerializeField]
        Image m_FocusModeIcon;
        [SerializeField]
        Sprite m_FocusModeClearIcon;
        [SerializeField]
        Sprite m_FocusModeManualIcon;
        [SerializeField]
        Sprite m_FocusModeReticleAFIcon;
        [SerializeField]
        Sprite m_FocusModeTrackingAFIcon;
        [SerializeField]
        Sprite m_JoystickSprite;
        [SerializeField]
        Sprite m_GamepadSprite;
        [SerializeField]
        Image m_InputButtonImage;

        Image m_RecordButtonImage;
        Toggle[] m_ToggleGroup;
        bool m_IsRecording;
        bool m_IsPlayback;
        MainViewOptions m_CachedViewOptions = MainViewOptions.All;

        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        public void SetOptions(MainViewOptions options)
        {
            m_CachedViewOptions = options;
            m_HudView.gameObject.SetActive(options.HasFlag(MainViewOptions.InformationBar));
            m_CameraSettingsTransform.gameObject.SetActive(!m_IsPlayback && options.HasFlag(MainViewOptions.CameraSettings));
            m_LensSettingsTransform.gameObject.SetActive(!m_IsPlayback && options.HasFlag(MainViewOptions.LensSettings));

            if (!m_IsPlayback && options.HasFlag(MainViewOptions.Joysticks))
                m_JoysticksView.Show();
            else
                m_JoysticksView.Hide();
        }

        public MainViewId ActiveToggle
        {
            get
            {
                return GetLabelFromToggle(GetActiveToggle());
            }
            set
            {
                SetAllTogglesOffWithoutNotify();

                var toggle = GetToggleFromLabel(value);

                if (toggle != null)
                {
                    toggle.SetIsOnWithoutNotify(true);
                }
            }
        }

        void Awake()
        {
            m_RecordButtonImage = m_RecordButton.transform.Find("Icon").GetComponent<Image>();

            PrepareToggles();

            m_HelpToggle.onValueChanged.AddListener(OnHelpModeToggled);
            m_RecordButton.onClick.AddListener(OnRecordClicked);
            m_ShowTakesToggle.onValueChanged.AddListener(OnShowTakesToggle);
            m_PlaybackToggle.onValueChanged.AddListener(OnDeviceModeToggle);
        }

        void OnDestroy()
        {
            // TODO dispose toggles

            m_HelpToggle.onValueChanged.RemoveListener(OnHelpModeToggled);
            m_RecordButton.onClick.RemoveListener(OnRecordClicked);
            m_ShowTakesToggle.onValueChanged.RemoveListener(OnShowTakesToggle);
            m_PlaybackToggle.onValueChanged.RemoveListener(OnDeviceModeToggle);
        }

        void OnHelpModeToggled(bool value) => ToggledHelpMode.Invoke(value);

        void OnDeviceModeToggle(bool value) => deviceModeToggled.Invoke(value ? DeviceMode.Playback : DeviceMode.LiveStream);

        void OnShowTakesToggle(bool value) => ShowTakesView.Invoke(value);

        void OnRecordClicked() => RecordClicked.Invoke();

        void PrepareToggles()
        {
            var go = new GameObject("Toggle Group", typeof(ToggleGroup));
            go.transform.SetParent(transform);

            PrepareToggle(m_ResetToggle, MainViewId.Reset);
            PrepareToggle(m_RigSettingsToggle, MainViewId.RigSettings);
            PrepareToggle(m_JoystickSettingsToggle, MainViewId.JoystickSettings);
            PrepareToggle(m_FocalDialToggle, MainViewId.FocalLength);
            PrepareToggle(m_FocusApertureToggle, MainViewId.Focus);
            PrepareToggle(m_FocusModeToggle, MainViewId.FocusMode);
            PrepareToggle(m_SettingsToggle, MainViewId.Settings);
            PrepareToggle(m_TakeIterationToggle, MainViewId.TakeIteration);

            m_ToggleGroup = new[]
            {
                m_ResetToggle,
                m_RigSettingsToggle,
                m_JoystickSettingsToggle,
                m_FocalDialToggle,
                m_FocusApertureToggle,
                m_FocusModeToggle,
                m_SettingsToggle,
                m_TakeIterationToggle
            };
        }

        void PrepareToggle(Toggle toggle, MainViewId label)
        {
            if (toggle == null)
            {
                Debug.LogError($"Toggle {label} reference is not set");
                return;
            }

            PrepareToggle(toggle, (value) =>
            {
                Toggled.Invoke(label, value);
            });
        }

        void PrepareToggle(Toggle toggle, UnityAction<bool> action)
        {
            Debug.Assert(toggle != null);

            toggle.isOn = false;
            toggle.onValueChanged.AddListener(action);
        }

        Toggle GetActiveToggle()
        {
            return m_ToggleGroup.FirstOrDefault(t => t.isOn);
        }

        void SetAllTogglesOffWithoutNotify()
        {
            foreach (var toggle in m_ToggleGroup)
            {
                toggle.SetIsOnWithoutNotify(false);
            }
        }

        public void SetTime(double time, double duration, FrameRate frameRate)
        {
            m_HudView.SetTime(time, duration, frameRate);

            if (duration > 0f)
            {
                m_TimeControlView.Show();
            }
            else
            {
                m_TimeControlView.Hide();
            }
        }

        public void SetSensorSize(Vector2 sensorSize)
        {
            m_HudView.SetSensorSize(sensorSize);
        }

        public void SetFocalLength(float value, Vector2 range)
        {
            m_HudView.SetFocalLength(value, range);
        }

        public void SetAperture(float aperture, Vector2 range)
        {
            m_HudView.SetAperture(aperture, range);
        }

        public void SetFocusDistance(float value, Vector2 range)
        {
            m_HudView.SetFocusDistance(value, range);
        }

        public void SetFocusMode(FocusMode value)
        {
            m_HudView.SetFocusMode(value);

            switch (value)
            {
                case FocusMode.Clear:
                    m_FocusModeIcon.sprite = m_FocusModeClearIcon;
                    break;
                case FocusMode.Manual:
                    m_FocusModeIcon.sprite = m_FocusModeManualIcon;
                    break;
                case FocusMode.ReticleAF:
                    m_FocusModeIcon.sprite = m_FocusModeReticleAFIcon;
                    break;
                case FocusMode.TrackingAF:
                    m_FocusModeIcon.sprite = m_FocusModeTrackingAFIcon;
                    break;
            }
        }

        public void SetLensIntrinsics(LensIntrinsics value)
        {
            m_HudView.SetLensIntrinsics(value);
        }

        public void SetSlateShotName(string shotName)
        {
            m_HudView.SetSlateShotName(shotName);
        }

        public void SetSlateTakeNumber(int takeNumber)
        {
            m_HudView.SetSlateTakeNumber(takeNumber);
        }

        public void SetChannelFlags(VirtualCameraChannelFlags channelFlags)
        {
            m_TakeIterationToggle.IsLocked = !channelFlags.AreAllChannelsActive();
        }

        public void SetRecordingState(bool value)
        {
            m_IsRecording = value;
            m_RecordButtonImage.sprite = m_IsRecording ? m_StopRecordingSprite : m_StartRecordingSprite;
            m_PlaybackToggle.gameObject.SetActive(!value);
            m_TimeControlView.SetInteractable(!value);
            UpdateTakeIterationToggle();
        }

        public void SetDeviceMode(DeviceMode mode)
        {
            m_IsPlayback = mode == DeviceMode.Playback;
            m_PlaybackToggle.SetValueWithoutNotify(m_IsPlayback);
            m_RecordButton.gameObject.SetActive(!m_IsPlayback);
            m_ShowTakesToggle.gameObject.SetActive(m_IsPlayback);

            UpdateTakeIterationToggle();

            SetOptions(m_CachedViewOptions);
            ShowTakesView.Invoke(m_IsPlayback && m_ShowTakesToggle.IsOn);
        }

        public void SetGamepadState(bool connected)
        {
            m_InputButtonImage.sprite = connected ? m_GamepadSprite : m_JoystickSprite;
        }

        void UpdateTakeIterationToggle()
        {
            m_TakeIterationToggle.gameObject.SetActive(!m_IsPlayback && (!m_IsRecording || m_TakeIterationToggle.IsLocked));
            m_TakeIterationToggle.Interactable = !m_IsRecording;
        }

        public bool TryGetPosition(MainViewId label, out Vector3 position, out Vector2 pivot)
        {
            var toggle = GetToggleFromLabel(label);
            var offset = Vector2.zero;

            switch (label)
            {
                case MainViewId.Reset:
                    offset = new Vector3(44f, 0f); // 48 / 2 + 20
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case MainViewId.RigSettings:
                    offset = new Vector3(44f, 0f); // 48 / 2 + 20
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case MainViewId.JoystickSettings:
                    offset = new Vector3(44f, 48f); // 48 / 2 + 20
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case MainViewId.GamepadSettings:
                    offset = new Vector3(44f, 48f); // 48 / 2 + 20
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case MainViewId.FocusMode:
                    offset = new Vector3(-36f, 0f);
                    pivot = new Vector2(1f, 0.5f);
                    break;
                case MainViewId.Settings:
                    pivot = new Vector2(0f, 1f);
                    position = m_SettingsContainer.position;
                    return true;
                case MainViewId.TakeIteration:
                    pivot = m_TakeIterationContainer.pivot;
                    position = m_TakeIterationContainer.position;
                    return true;
                default:
                    pivot = Vector2.one * 0.5f;
                    break;
            }

            position = toggle.transform.TransformPoint(offset);

            return true;
        }

        public bool TryGetSize(MainViewId label, out Vector2 size)
        {
            size = Vector2.one * 100f;

            switch (label)
            {
                case MainViewId.Reset:
                    size = new Vector2(173f, 80f);
                    break;
                case MainViewId.RigSettings:
                    size = m_RigSettingsContainer.rect.size;
                    return true;
                case MainViewId.JoystickSettings:
                    size = m_JoystickSettingsContainer.rect.size;
                    return true;
                case MainViewId.GamepadSettings:
                    size = m_GamepadSettingsContainer.rect.size;
                    return true;
                case MainViewId.Settings:
                    size = m_SettingsContainer.rect.size;
                    return true;
                default:
                    size = Vector3.zero;
                    break;
            }

            return false;
        }

        Toggle GetToggleFromLabel(MainViewId label)
        {
            switch (label)
            {
                case MainViewId.Reset:
                    return m_ResetToggle;
                case MainViewId.RigSettings:
                    return m_RigSettingsToggle;
                case MainViewId.JoystickSettings:
                    return m_JoystickSettingsToggle;
                case MainViewId.GamepadSettings:
                    return m_JoystickSettingsToggle;
                case MainViewId.FocalLength:
                    return m_FocalDialToggle;
                case MainViewId.Focus:
                    return m_FocusApertureToggle;
                case MainViewId.FocusMode:
                    return m_FocusModeToggle;
                case MainViewId.Settings:
                    return m_SettingsToggle;
                case MainViewId.TakeIteration:
                    return m_TakeIterationToggle;
            }

            return null;
        }

        MainViewId GetLabelFromToggle(Toggle toggle)
        {
            if (toggle == m_ResetToggle)
            {
                return MainViewId.Reset;
            }
            if (toggle == m_RigSettingsToggle)
            {
                return MainViewId.RigSettings;
            }
            if (toggle == m_JoystickSettingsToggle)
            {
                return MainViewId.JoystickSettings;
            }
            if (toggle == m_FocalDialToggle)
            {
                return MainViewId.FocalLength;
            }
            if (toggle == m_FocusApertureToggle)
            {
                return MainViewId.Focus;
            }
            if (toggle == m_FocusModeToggle)
            {
                return MainViewId.FocusMode;
            }
            if (toggle == m_TakeIterationToggle)
            {
                return MainViewId.TakeIteration;
            }

            return MainViewId.None;
        }
    }
}
