using System;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class MainViewMobileMediator : IInitializable, IDisposable
    {
        [Inject]
        StateModel m_State;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        SettingsModel m_Settings;
        [Inject]
        Gamepad.IGamepadDriver m_Gamepad;
        [Inject]
        IMainViewMobile m_VirtualCameraView;
        [Inject]
        IResetView m_ResetView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_Gamepad.OnHasDeviceChanged += OnHasDeviceChanged;
            m_VirtualCameraView.LensSettingsClicked += OnLensSettingsClicked;
            m_VirtualCameraView.RigSettingsClicked += OnRigSettingsClicked;
            m_VirtualCameraView.JoystickSettingsClicked += OnJoystickSettingsClicked;
            m_VirtualCameraView.RecordClicked += OnRecordClicked;
            m_VirtualCameraView.SettingsClicked += OnSettingsClicked;
            m_VirtualCameraView.DeviceModeToggled += OnDeviceModeToggled;
            m_VirtualCameraView.HelpToggled += OnHelpToggled;
            m_VirtualCameraView.TakeIterationToggled += OnTakeIterationToggled;
            m_VirtualCameraView.ResetMenuToggled += OnResetMenuToggled;
            m_VirtualCameraView.ShowTakesView += OnShowTakesView;
            m_ResetView.onResetLensClicked += OnResetViewOptionClicked;
            m_ResetView.onResetPoseClicked += OnResetViewOptionClicked;
            m_ResetView.onRebaseToggled += OnResetViewOptionClicked;
        }

        public void Dispose()
        {
            m_Gamepad.OnHasDeviceChanged -= OnHasDeviceChanged;
            m_VirtualCameraView.LensSettingsClicked -= OnLensSettingsClicked;
            m_VirtualCameraView.RigSettingsClicked -= OnRigSettingsClicked;
            m_VirtualCameraView.JoystickSettingsClicked -= OnJoystickSettingsClicked;
            m_VirtualCameraView.RecordClicked -= OnRecordClicked;
            m_VirtualCameraView.SettingsClicked -= OnSettingsClicked;
            m_VirtualCameraView.DeviceModeToggled -= OnDeviceModeToggled;
            m_VirtualCameraView.HelpToggled -= OnHelpToggled;
            m_VirtualCameraView.TakeIterationToggled -= OnTakeIterationToggled;
            m_VirtualCameraView.ResetMenuToggled -= OnResetMenuToggled;
            m_VirtualCameraView.ShowTakesView -= OnShowTakesView;
            m_ResetView.onResetLensClicked -= OnResetViewOptionClicked;
            m_ResetView.onResetPoseClicked -= OnResetViewOptionClicked;
            m_ResetView.onRebaseToggled -= OnResetViewOptionClicked;
        }

        void OnHasDeviceChanged(bool value)
        {
            m_VirtualCameraView.SetGamepadState(value);
        }

        void OnShowTakesView(bool value)
        {
            m_SignalBus.Fire(new ShowTakeLibrarySignal() { value = value });
        }

        void OnTakeIterationToggled(bool value)
        {
            m_SignalBus.Fire(new ShowTakeIterationOrHelpSignal() { value = value });
        }

        void OnHelpToggled(bool value)
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleHelp {value = value});
        }

        void OnSettingsClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.OpenSettingsView());
        }

        void OnResetMenuToggled(bool value)
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleResetView()
            {
                value = value
            });
        }

        void OnLensSettingsClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.OpenLensSettingsView());
        }

        void OnDeviceModeToggled(bool value)
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleDeviceMode());
        }

        void OnRigSettingsClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.OpenRigSettingsView());
        }

        void OnJoystickSettingsClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.OpenJoystickSettingsView());
        }

        void OnResetViewOptionClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.ResetViewOptionSelected());
        }

        void OnRecordClicked()
        {
            m_SignalBus.Fire(new MainViewSignals.ToggleRecording());
        }
    }
}
