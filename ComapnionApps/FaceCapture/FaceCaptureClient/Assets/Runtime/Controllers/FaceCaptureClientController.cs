using System;
using Unity.LiveCapture.CompanionApp;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class FaceCaptureClientController : IInitializable, IDisposable, ITickable, IDeviceModeListener
    {
        const string k_AttemptRecordingInPlaybackModeMessage = "You cannot record in playback mode";

        [Inject]
        IBackgroundEventsView m_BackgroundEventsView;

        [Inject]
        IBackgroundOverlayView m_BackgroundOverlayView;

        [Inject]
        ICountdownView m_CountdownView;

        [Inject]
        IMainView m_MainView;

        [Inject]
        ISettingsView m_SettingsView;

        [Inject]
        ILaunchScreenView m_LaunchScreenView;

        [Inject]
        ICalibrationView m_CalibrationView;

        [Inject]
        INotificationView m_NotificationView;

        [Inject]
        ICountdownController m_CountdownController;

        [Inject]
        IARFaceManagerSystem m_FaceManager;

        [Inject]
        IFaceTracker m_FaceTracker;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        [Inject]
        HelpSystem m_HelpSystem;

        [Inject]
        SignalBus m_SignalBus;

        IDialogView[] m_MutuallyExclusiveViews;

        public void Initialize()
        {
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnRemoteConnected);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnRemoteDisconnected);
            m_SignalBus.Subscribe<MainViewSignals.OpenSettings>(OnMainViewOpenSettings);
            m_SignalBus.Subscribe<MainViewSignals.ToggleRecording>(OnMainViewToggleRecording);
            m_SignalBus.Subscribe<MainViewSignals.ToggleHelp>(OnMainViewToggleHelp);
            m_SignalBus.Subscribe<MainViewSignals.TrackingClicked>(OnMainViewTrackingClicked);
            m_SignalBus.Subscribe<MainViewSignals.TakesClicked>(OnMainViewTakesClicked);
            m_SignalBus.Subscribe<SettingsViewSignals.Close>(OnSettingsViewClose);
            m_SignalBus.Subscribe<PermissionsSignals.CameraGranted>(OnCameraPermissionsGranted);

            m_CalibrationView.StateChanged += OnCalibrationViewStateChanged;

            m_FaceManager.Enabled = false;

            m_MutuallyExclusiveViews = new IDialogView[]
            {
                m_CountdownView,
                m_MainView,
                m_SettingsView,
                m_LaunchScreenView
            };
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(OnRemoteConnected);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnRemoteDisconnected);
            m_SignalBus.Unsubscribe<MainViewSignals.OpenSettings>(OnMainViewOpenSettings);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleRecording>(OnMainViewToggleRecording);
            m_SignalBus.Unsubscribe<MainViewSignals.ToggleHelp>(OnMainViewToggleHelp);
            m_SignalBus.Unsubscribe<MainViewSignals.TrackingClicked>(OnMainViewTrackingClicked);
            m_SignalBus.Unsubscribe<MainViewSignals.TakesClicked>(OnMainViewTakesClicked);
            m_SignalBus.Unsubscribe<SettingsViewSignals.Close>(OnSettingsViewClose);
            m_SignalBus.Unsubscribe<PermissionsSignals.CameraGranted>(OnCameraPermissionsGranted);

            m_CalibrationView.StateChanged -= OnCalibrationViewStateChanged;
        }

        public void Tick()
        {
            m_MainView.SetTrackingState(m_FaceTracker.IsTracking);
        }

        void OnCameraPermissionsGranted()
        {
            ShowLaunchScreen();
        }

        void OnRemoteConnected()
        {
            m_CompanionApp.SetDeviceMode(DeviceMode.LiveStream);

            ShowMainView();
        }

        void OnRemoteDisconnected()
        {
            ShowLaunchScreen();
        }

        void ShowLaunchScreen()
        {
            m_FaceManager.Enabled = false;

            ShowExclusive(m_LaunchScreenView);
            m_CalibrationView.ViewState = ICalibrationView.State.MenuClosed;

            m_BackgroundEventsView.Hide();
            m_NotificationView.Show();
        }

        void OnMainViewToggleRecording()
        {
            if (m_HelpSystem.IsActive())
            {
                return;
            }

            if (m_CompanionApp.DeviceMode == DeviceMode.Playback)
            {
                m_SignalBus.Fire(new ShowNotificationSignal()
                {
                    value = k_AttemptRecordingInPlaybackModeMessage
                });
                return;
            }

            m_CountdownController.ToggleRecording();
            m_MainView.SetRecordingState(m_CountdownView.IsPlaying);
        }

        void OnMainViewToggleHelp(MainViewSignals.ToggleHelp signal)
        {
            m_HelpSystem.Toggle(signal.IsActive);
        }

        void OnMainViewTrackingClicked(MainViewSignals.TrackingClicked signal)
        {
            DispatchHelpIfNeeded(HelpTooltipId.Tracking);
        }

        void OnMainViewTakesClicked()
        {
            DispatchHelpIfNeeded(HelpTooltipId.Takes);
        }

        void OnMainViewOpenSettings()
        {
            if (DispatchHelpIfNeeded(HelpTooltipId.Settings))
            {
                return;
            }

            m_FaceManager.Enabled = false;

            ShowExclusive(m_SettingsView);

            m_BackgroundEventsView.Hide();
            m_NotificationView.Hide();
        }

        void OnCalibrationViewStateChanged(ICalibrationView.State state)
        {
            switch (state)
            {
                case ICalibrationView.State.MenuClosed:
                case ICalibrationView.State.MenuOpen:
                case ICalibrationView.State.Calibrating:
                    ShowMainView();
                    m_BackgroundOverlayView.Hide();
                    m_BackgroundOverlayView.DisposeTexture();
                    break;

                case ICalibrationView.State.Reviewing:
                    m_FaceManager.Enabled = false;
                    m_BackgroundEventsView.Hide();
                    m_NotificationView.Hide();
                    m_MainView.Hide();
                    break;
            }

            switch (state)
            {
                case ICalibrationView.State.MenuClosed:
                case ICalibrationView.State.MenuOpen:
                case ICalibrationView.State.Reviewing:
                    m_MainView.RecordButtonActive = true;
                    break;
                case ICalibrationView.State.Calibrating:
                    m_MainView.RecordButtonActive = false;
                    break;
            }
        }

        bool DispatchHelpIfNeeded(HelpTooltipId id)
        {
            if (m_HelpSystem.IsActive())
            {
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = id});
                return true;
            }

            return false;
        }

        void OnSettingsViewClose()
        {
            ShowMainView();
        }

        void ShowMainView()
        {
            m_BackgroundEventsView.Show();
            m_NotificationView.Show();

            ShowExclusive(m_MainView);

            m_FaceManager.Enabled = true;
        }

        void ShowExclusive(IDialogView view)
        {
            foreach (var v in m_MutuallyExclusiveViews)
            {
                if (v != view)
                {
                    v.Hide();
                }
            }

            if (!view.IsShown)
            {
                view.Show();
            }
        }

        public void SetDeviceMode(DeviceMode mode)
        {
            m_MainView.RecordButtonOpacity = mode == DeviceMode.LiveStream ? 1 : .5f;
        }
    }
}
