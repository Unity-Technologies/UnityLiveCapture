using System;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    public class MainViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IMainViewTablet m_MainView;
        [Inject]
        IFocalLengthView m_FocalLengthView;
        [Inject]
        IFocusDistanceView m_FocusDistanceView;
        [Inject]
        IFocusModeView m_FocusModeView;
        [Inject]
        IResetView m_ResetView;
        [Inject]
        Gamepad.IGamepadDriver m_Gamepad;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_MainView.Toggled += OnToggle;
            m_MainView.ToggledHelpMode += OnToggledHelpMode;
            m_MainView.RecordClicked += OnRecordClicked;
            m_MainView.deviceModeToggled += OnDeviceModeToggled;
            m_MainView.ShowTakesView += OnShowTakesView;
            m_FocalLengthView.ValueChanged += OnFocalLengthChanged;
            m_FocalLengthView.Closed += OnFocalLengthViewClosed;
            m_FocusDistanceView.Closed += OnFocusViewClosed;
            m_FocusModeView.FocusModeChanged += OnFocusModeChanged;
            m_ResetView.onResetLensClicked += OnResetOptionClicked;
            m_ResetView.onResetPoseClicked += OnResetOptionClicked;
            m_ResetView.onRebaseToggled += OnResetOptionClicked;
            m_Gamepad.OnHasDeviceChanged += OnHasDeviceChanged;
        }

        public void Dispose()
        {
            m_MainView.Toggled -= OnToggle;
            m_MainView.ToggledHelpMode -= OnToggledHelpMode;
            m_MainView.RecordClicked -= OnRecordClicked;
            m_MainView.deviceModeToggled -= OnDeviceModeToggled;
            m_MainView.ShowTakesView -= OnShowTakesView;
            m_FocalLengthView.ValueChanged -= OnFocalLengthChanged;
            m_FocalLengthView.Closed -= OnFocalLengthViewClosed;
            m_FocusDistanceView.Closed -= OnFocusViewClosed;
            m_FocusModeView.FocusModeChanged -= OnFocusModeChanged;
            m_ResetView.onResetLensClicked -= OnResetOptionClicked;
            m_ResetView.onResetPoseClicked -= OnResetOptionClicked;
            m_ResetView.onRebaseToggled -= OnResetOptionClicked;
            m_Gamepad.OnHasDeviceChanged -= OnHasDeviceChanged;
        }

        void OnShowTakesView(bool value)
        {
            m_SignalBus.Fire(new ShowTakeLibrarySignal() { value = value });
        }

        void OnToggle(MainViewId label, bool value)
        {
            if (value)
            {
                if (label == MainViewId.JoystickSettings && m_Gamepad.HasDevice)
                {
                    label = MainViewId.GamepadSettings;
                }

                m_SignalBus.Fire(new ShowMainViewSignal() { value = label });
            }
            else
            {
                m_SignalBus.Fire(new HideMainViewSignal());
            }
        }

        void OnDeviceModeToggled(DeviceMode deviceMode)
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleDeviceMode());
        }

        void OnToggledHelpMode(bool value)
        {
            m_SignalBus.Fire(new HelpModeSignals.Toggle {value = value});
            m_SignalBus.Fire(new ShowMainViewSignal(){value = MainViewId.None});
        }

        void OnFocalLengthChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocalLength,
                FloatValue = value
            });
        }

        void OnFocalLengthViewClosed()
        {
            m_SignalBus.Fire(new HideMainViewSignal());
        }

        void OnFocusViewClosed()
        {
            m_SignalBus.Fire(new HideMainViewSignal());
        }

        void OnFocusModeChanged(FocusMode focusMode)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocusMode,
                FocusModeValue = focusMode
            });
        }

        void OnResetOptionClicked()
        {
            m_SignalBus.Fire(new HideMainViewSignal());
        }

        void OnRecordClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleRecording());
        }

        void OnHasDeviceChanged(bool hasDevice)
        {
            m_MainView.SetGamepadState(hasDevice);

            if (m_MainView.ActiveToggle == MainViewId.JoystickSettings ||
                m_MainView.ActiveToggle == MainViewId.GamepadSettings)
            {
                OnToggle(hasDevice ? MainViewId.GamepadSettings : MainViewId.JoystickSettings, true);
            }
        }
    }
}
