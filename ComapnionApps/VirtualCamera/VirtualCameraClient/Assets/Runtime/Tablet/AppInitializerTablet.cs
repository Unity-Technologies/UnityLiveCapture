using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class AppInitializerTablet : IInitializable
    {
        const string k_ClientType = "Virtual Camera";
        const string k_SessionViewMessage = "Client Device is connected but not assigned to any VirtualCameraDevice in the Scene.";

        [Inject]
        DeviceDataSystem m_DeviceDataSystem;
        [Inject]
        ConnectionModel m_Connection;
        [Inject]
        ConnectionViewController m_ConnectionViewController;
        [Inject]
        IConnectionScreenView m_ConnectionScreenView;
        [Inject]
        IMainView m_MainView;
        [Inject]
        INotificationView m_NotificationView;
        [Inject]
        ISettingsView m_SettingsView;
        [Inject]
        ICountdownView m_CountdownView;
        [Inject]
        ISessionView m_Sessionview;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_DeviceDataSystem.LoadData();

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            m_SettingsView.UpdateDeviceData(m_DeviceDataSystem.deviceData);
            m_SettingsView.CollapseAllSections();

            m_Sessionview.SetMessage(k_SessionViewMessage);

            m_MainView.Hide();
            m_CountdownView.Hide();
            m_NotificationView.Hide();
            m_ConnectionScreenView.Hide();
            m_Sessionview.Hide();

            m_Connection.ClientType = k_ClientType;
            m_Connection.Load();
            m_ConnectionViewController.UpdateIpAndPort();
            m_ConnectionViewController.UpdateLayout();

            m_SignalBus.Fire(new ShowMainViewSignal() { value = MainViewId.None });
        }
    }
}
