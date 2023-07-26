using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class AppInitializer : IInitializable
    {
        const string k_ClientType = "ARKit Face Capture";
        const string k_SessionViewMessage = "Client Device is connected but not assigned to any ARKit FaceDevice in the Scene.";

        [Inject]
        ConnectionModel m_Connection;

        [Inject(Id = FaceClientViewId.Background)]
        IDialogView m_BackgroundView;

        [Inject(Id = FaceClientViewId.BackgroundOverlay)]
        IDialogView m_BackgroundOverlayView;

        [Inject]
        IHelpView m_HelpView;

        [Inject(Id = FaceClientViewId.Main)]
        IDialogView m_MainView;

        [Inject(Id = FaceClientViewId.Calibration)]
        IDialogView m_Calibration;

        [Inject(Id = FaceClientViewId.Countdown)]
        IDialogView m_CountdownView;

        [Inject(Id = FaceClientViewId.CalibrationCountdown)]
        IDialogView m_CalibrationCountdownView;

        [Inject(Id = FaceClientViewId.Settings)]
        IDialogView m_SettingsView;

        [Inject(Id = FaceClientViewId.LaunchScreen)]
        IDialogView m_LaunchScreenView;

        [Inject(Id = FaceClientViewId.Notification)]
        IDialogView m_NotificationView;

        [Inject]
        ISessionView m_SessionView;

        [Inject]
        IPermissionsView m_PermissionsView;

        [Inject]
        ConnectionViewController m_ConnectionViewController;

        [Inject]
        IARFaceManagerSystem m_FaceManager;

        [Inject]
        Settings m_Settings;

        [Inject]
        SettingsController m_SettingsController;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            m_Connection.ClientType = k_ClientType;
            m_Connection.Load();
            m_ConnectionViewController.UpdateIpAndPort();
            m_ConnectionViewController.UpdateLayout();

            m_SessionView.SetMessage(k_SessionViewMessage);

            m_BackgroundView.Hide();
            m_BackgroundOverlayView.Hide();
            m_HelpView.Hide();
            m_MainView.Hide();
            m_Calibration.Hide();
            m_SettingsView.Hide();
            m_CountdownView.Hide();
            m_CalibrationCountdownView.Hide();
            m_NotificationView.Hide();
            m_PermissionsView.Hide();
            m_LaunchScreenView.Hide();
            m_SessionView.Hide();

            m_HelpView.Alignment = TextAnchor.MiddleCenter;

            m_Settings.Load();
            m_SettingsController.NotifyAllProperties();

#if !UNITY_EDITOR
            FaceManagerSupportCheck();
#endif
        }

        void FaceManagerSupportCheck()
        {
            if (!m_FaceManager.IsSupported)
            {
                m_SignalBus.Fire(new ShowModalSignal()
                {
                    Title = "Unsupported device",
                    PositiveText = "Go to website",
                    PositiveCallback = () =>
                    {
                        Application.OpenURL("https://support.apple.com/en-us/HT209183");
                    },
                    Text = "ARKit Face Tracking is not supported on this device.\n\nPlease go to Apple's website to " +
                    "learn which devices are supported.",
                    BackgroundOpacity = 1f,
                    ClosePopupOnConfirm = false
                });
            }
        }
    }
}
