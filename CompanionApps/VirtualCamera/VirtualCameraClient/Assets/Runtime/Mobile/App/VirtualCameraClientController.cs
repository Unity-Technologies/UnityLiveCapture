using System;
using System.Collections;
using Unity.LiveCapture.CompanionApp;
using Unity.CompanionAppCommon;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class VirtualCameraClientController : IInitializable, IDisposable, IHelpModeListener
    {
        [Inject]
        StateModel m_State;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        [Inject]
        ISettingsView m_SettingsView;

        [Inject]
        IConnectionScreenView m_ConnectionScreenView;

        [Inject]
        INotificationView m_NotificationView;

        [Inject]
        IRigSettingsView m_RigSettingsView;

        [Inject]
        IJoystickSettingsView m_JoystickSettingsView;

        [Inject]
        IGamepadSettingsView m_GamepadSettingsView;

        [Inject]
        ICameraLensSettingsView m_LensSettingsView;

        [Inject]
        IMainViewMobile m_MainView;

        [Inject]
        IResetView m_ResetView;

        [Inject]
        IJoysticksView m_JoysticksView;

        [Inject]
        DeviceDataSystem m_DeviceDataSystem;

        [Inject]
        ICoroutineRunner m_Runner;

        [Inject]
        Gamepad.GamepadSystem m_Gamepad;

        [Inject]
        Gamepad.IGamepadDriver m_GamepadDriver;

        [Inject]
        SignalBus m_SignalBus;

        Coroutine m_SwitchToMainViewCoroutine;

        public void Initialize()
        {
            m_SignalBus.Subscribe<MainViewSignals.OpenSettingsView>(OnOpenSettingsView);
            m_SignalBus.Subscribe<MainViewSignals.OpenRigSettingsView>(OnOpenRigSettings);
            m_SignalBus.Subscribe<MainViewSignals.OpenJoystickSettingsView>(OnOpenJoystickSettings);
            m_SignalBus.Subscribe<MainViewSignals.OpenLensSettingsView>(OnOpenLensSettings);
            m_SignalBus.Subscribe<MainViewSignals.ToggleHelp>(OnToggleHelp);
            m_SignalBus.Subscribe<MainViewSignals.ToggleDeviceMode>(OnToggleDeviceMode);
            m_SignalBus.Subscribe<MainViewSignals.ToggleRecording>(OnToggleRecording);
            m_SignalBus.Subscribe<ShowTakeIterationSignal>(OnShowTakeIteration);
            m_SignalBus.Subscribe<MainViewSignals.ToggleResetView>(OnToggleResetView);
            m_SignalBus.Subscribe<MainViewSignals.ResetViewOptionSelected>(OnResetViewOptionSelected);
            m_SignalBus.Subscribe<LensSettingsViewSignals.Close>(OnLensSettingsClosed);
            m_SignalBus.Subscribe<ConnectionScreenViewSignals.Close>(OnConnectionScreenClosed);
            m_SignalBus.Subscribe<RigSettingsViewSignals.Close>(OnRigSettingsClosed);
            m_SignalBus.Subscribe<JoystickSettingsViewSignals.Close>(OnJoystickSettingsClosed);
            m_SignalBus.Subscribe<GamepadSettingsViewSignals.Close>(OnGamepadSettingsClosed);
            m_SignalBus.Subscribe<SettingsViewSignals.Close>(OnSettingsClosed);
            m_SignalBus.Subscribe<PermissionsSignals.CameraGranted>(OnCameraGranted);
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<MainViewSignals.OpenSettingsView>(OnOpenSettingsView);
            m_SignalBus.Unsubscribe<MainViewSignals.OpenRigSettingsView>(OnOpenRigSettings);
            m_SignalBus.Unsubscribe<MainViewSignals.OpenJoystickSettingsView>(OnOpenJoystickSettings);
            m_SignalBus.Unsubscribe<MainViewSignals.OpenLensSettingsView>(OnOpenLensSettings);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleHelp>(OnToggleHelp);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleDeviceMode>(OnToggleDeviceMode);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleRecording>(OnToggleRecording);
            m_SignalBus.Unsubscribe<ShowTakeIterationSignal>(OnShowTakeIteration);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleResetView>(OnToggleResetView);
            m_SignalBus.Unsubscribe<MainViewSignals.ResetViewOptionSelected>(OnResetViewOptionSelected);
            m_SignalBus.Unsubscribe<LensSettingsViewSignals.Close>(OnLensSettingsClosed);
            m_SignalBus.Unsubscribe<ConnectionScreenViewSignals.Close>(OnConnectionScreenClosed);
            m_SignalBus.Unsubscribe<RigSettingsViewSignals.Close>(OnRigSettingsClosed);
            m_SignalBus.Unsubscribe<JoystickSettingsViewSignals.Close>(OnJoystickSettingsClosed);
            m_SignalBus.Unsubscribe<GamepadSettingsViewSignals.Close>(OnGamepadSettingsClosed);
            m_SignalBus.Unsubscribe<SettingsViewSignals.Close>(OnSettingsClosed);
            m_SignalBus.Unsubscribe<PermissionsSignals.CameraGranted>(OnCameraGranted);
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnDisconnected);

            if (!SceneState.IsBeingDestroyed)
            {
                StopSwitchToMainViewCoroutine();
            }
        }

        public void SetHelpMode(bool value)
        {
            if (!value)
            {
                if (m_DeviceDataSystem.deviceData.mainViewOptions.HasFlag(MainViewOptions.Joysticks))
                {
                    m_JoysticksView.Show();
                }

                OnReturnToMainView();
            }
            else
            {
                m_JoysticksView.Hide();

                m_Gamepad.Enabled = false;
            }
        }

        void OnCameraGranted()
        {
            m_ConnectionScreenView.Show();
            m_NotificationView.Show();

            m_Gamepad.Enabled = false;
        }

        void OnSettingsClosed()
        {
            CloseTooltip();

            m_SettingsView.Hide();
            m_MainView.Show();

            OnReturnToMainView();
        }

        void OnRigSettingsClosed()
        {
            CloseTooltip();

            m_RigSettingsView.Hide();
            m_MainView.Show();

            OnReturnToMainView();
        }

        void OnJoystickSettingsClosed()
        {
            CloseTooltip();

            m_JoystickSettingsView.Hide();
            m_MainView.Show();

            OnReturnToMainView();
        }

        void OnGamepadSettingsClosed()
        {
            CloseTooltip();

            m_GamepadSettingsView.Hide();
            m_MainView.Show();

            OnReturnToMainView();
        }

        void OnConnectionScreenClosed()
        {
            m_ConnectionScreenView.Hide();
            m_MainView.Show();

            OnReturnToMainView();
        }

        void OnConnected()
        {
            // Exit connection screen on successful connection, with a delay.
            if (m_ConnectionScreenView.IsShown)
            {
                // Make sure we don't have multiple coroutines running.
                StopSwitchToMainViewCoroutine();
                m_SwitchToMainViewCoroutine = m_Runner.StartCoroutine(SwitchToMainView());
            }
        }

        void OnDisconnected() => StopSwitchToMainViewCoroutine();

        IEnumerator SwitchToMainView()
        {
            yield return new WaitForSeconds(1);

            // Close the connection screen if it is still opened.
            if (m_ConnectionScreenView.IsShown)
            {
                OnConnectionScreenClosed();
            }
        }

        void OnOpenSettingsView()
        {
            if (!m_State.IsHelpMode)
            {
                m_SettingsView.Show();
            }
            else
            {
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.Settings});
            }

            m_Gamepad.Enabled = false;
        }

        void OnOpenRigSettings()
        {
            CloseTooltip();
            ShowResetView(false);

            m_RigSettingsView.Show();

            m_Gamepad.Enabled = false;
        }

        void OnOpenJoystickSettings()
        {
            CloseTooltip();
            ShowResetView(false);

            if (m_GamepadDriver.HasDevice)
            {
                m_GamepadSettingsView.Show();
            }
            else
            {
                m_JoystickSettingsView.Show();
            }

            m_Gamepad.Enabled = false;
        }

        void OnOpenLensSettings()
        {
            CloseTooltip();
            ShowResetView(false);

            m_MainView.SetLensViewOpenState(true);
            m_LensSettingsView.Show();
        }

        void OnLensSettingsClosed()
        {
            CloseTooltip();

            m_MainView.SetLensViewOpenState(false);
            m_LensSettingsView.Hide();
        }

        void OnToggleHelp(MainViewSignals.ToggleHelp signal)
        {
            // Prevent reset view from being opened as we enter help mode.
            ShowResetView(false);
            m_SignalBus.Fire(new HelpModeSignals.Toggle {value = signal.value});
        }

        void OnToggleDeviceMode()
        {
            var newMode = m_CompanionApp.DeviceMode == DeviceMode.LiveStream
                ? DeviceMode.Playback
                : DeviceMode.LiveStream;

            m_CompanionApp.SetDeviceMode(newMode);
            OnReturnToMainView(newMode);
        }

        void OnToggleRecording()
        {
            if (!m_State.IsHelpMode)
            {
                if (m_CompanionApp.IsRecording)
                {
                    m_SignalBus.Fire(new RequestStopRecordingSignal());
                }
                else
                {
                    m_SignalBus.Fire(new RequestStartRecordingSignal());
                }
            }

            OnReturnToMainView();
        }

        void OnShowTakeIteration(ShowTakeIterationSignal signal)
        {
            if (signal.value)
            {
                m_Gamepad.Enabled = false;
            }
            else
            {
                OnReturnToMainView();
            }
        }

        void OnToggleResetView(MainViewSignals.ToggleResetView signal)
        {
            CloseTooltip();

            var show = signal.value;

            if (show && m_State.IsHelpMode)
            {
                m_MainView.SetMenuTogglesOffWithoutNotify();
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.Reset });
                return;
            }

            ShowResetView(show);
        }

        void OnResetViewOptionSelected(MainViewSignals.ResetViewOptionSelected signal)
        {
            CloseTooltip();

            ShowResetView(false);
        }

        void ShowResetView(bool value)
        {
            if (value)
            {
                m_MainView.TryGetPosition(MainViewId.Reset, out var position, out var pivot);
                m_MainView.TryGetSize(MainViewId.Reset, out var size);

                m_ResetView.Position = position;
                m_ResetView.Pivot = pivot;
                m_ResetView.Size = size;
                m_ResetView.Show();
            }
            else
            {
                m_MainView.SetMenuTogglesOffWithoutNotify();
                m_ResetView.Hide();
            }
        }

        void CloseTooltip()
        {
            m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());
        }

        void StopSwitchToMainViewCoroutine()
        {
            if (m_SwitchToMainViewCoroutine != null)
            {
                m_Runner.StopCoroutine(m_SwitchToMainViewCoroutine);
            }
        }

        void OnReturnToMainView(DeviceMode? explicitMode = null)
        {
            m_Gamepad.Enabled = !m_State.IsHelpMode;
        }
    }
}
