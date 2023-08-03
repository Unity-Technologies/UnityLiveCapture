using UnityEngine;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.VirtualCamera;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class AppInitializer : IInitializable
    {
        const string k_ClientType = "Virtual Camera";
        const string k_SessionViewMessage = "Client Device is connected but not assigned to any VirtualCameraDevice in the Scene.";

        [Inject]
        SignalBus m_SignalBus;

        [Inject]
        ConnectionModel m_Connection;

        [Inject]
        IFocusReticle m_FocusReticle;

        [Inject]
        ConnectionViewController m_ConnectionViewController;

        [Inject]
        IMainView m_MainView;

        [Inject]
        IConnectionScreenView m_ConnectionScreenView;

        [Inject]
        INotificationView m_NotificationView;

        [Inject]
        ICameraLensSettingsView m_CameraLensSettingsView;

        [Inject]
        IRigSettingsView m_RigSettingsView;

        [Inject]
        IJoystickSettingsView m_JoystickSettings;

        [Inject]
        IGamepadSettingsView m_GamepadSettings;

        [Inject]
        ISettingsView m_SettingsView;

        [Inject]
        IResetView m_ResetView;

        [Inject]
        IPermissionsView m_PermissionsView;

        [Inject]
        DeviceDataSystem m_DeviceDataSystem;

        [Inject]
        ICountdownView m_CountdownView;

        [Inject]
        ISessionView m_Sessionview;

        [Inject]
        ITakeLibraryView m_TakeLibraryView;

        public void Initialize()
        {
            m_DeviceDataSystem.LoadData();

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            m_SettingsView.UpdateDeviceData(m_DeviceDataSystem.deviceData);
            m_SettingsView.CollapseAllSections();
            m_SettingsView.Hide();

            m_Sessionview.SetMessage(k_SessionViewMessage);

            m_FocusReticle.SetActive(false);
            m_ConnectionScreenView.Hide();
            m_NotificationView.Hide();
            m_CameraLensSettingsView.Hide();
            m_CameraLensSettingsView.ToggleOff();
            m_RigSettingsView.Hide();
            m_JoystickSettings.Hide();
            m_GamepadSettings.Hide();
            m_CountdownView.Hide();
            m_ResetView.Hide();
            m_TakeLibraryView.Hide();
            m_MainView.Hide();
            m_PermissionsView.Hide();
            m_Sessionview.Hide();

            m_Connection.ClientType = k_ClientType;
            m_Connection.Load();
            m_ConnectionViewController.UpdateIpAndPort();
            m_ConnectionViewController.UpdateLayout();

            m_SignalBus.Fire(new ShowTakeIterationSignal(){value = false});
            m_SignalBus.Fire(new SetMainViewOptionSignal());
        }
    }
}
