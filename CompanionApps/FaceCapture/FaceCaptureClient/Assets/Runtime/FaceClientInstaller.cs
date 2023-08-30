using System;
using UnityEngine;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    public class FaceClientInstaller : MonoInstaller
    {
        const string k_PrivacyPolicyURL = "https://unity3d.com/legal/privacy-policy";
        const string k_DocumentationURL = "https://docs.unity3d.com/Packages/com.unity.live-capture@2.0/manual/face-capture.html";
        const string k_SupportURL = "https://forum.unity.com/threads/1111255/";
        const string k_SessionDocumentationURL = "https://docs.unity3d.com/Packages/com.unity.live-capture@2.0/manual/ref-component-arkit-face-device.html";

        [SerializeField]
        Canvas m_Canvas;

        [SerializeField]
        RectTransform m_Background;

        [SerializeField]
        RectTransform m_SafeArea;

        [SerializeField]
        BackgroundEventsView m_BackgroundEventsView;

        [SerializeField]
        DialogView m_BackgroundView;

        [SerializeField]
        BackgroundOverlayView m_BackgroundOverlayView;

        [SerializeField]
        CountdownView m_CountdownView;

        [SerializeField]
        HelpView m_HelpView;

        [SerializeField]
        HelpTooltip m_HelpTooltip;

        [SerializeField]
        TextAsset m_HelpData;

        [SerializeField]
        MainView m_MainView;

        [SerializeField]
        CalibrationView m_CalibrationView;

        [SerializeField]
        NotificationView m_NotificationView;

        [SerializeField]
        SettingsView m_SettingsView;

        [SerializeField]
        LaunchScreenView m_LaunchScreen;

        [SerializeField]
        PermissionsView m_PermissionsView;

        [SerializeField]
        SessionView m_SessionView;

        [SerializeField]
        ARFaceManagerSystem m_FaceManagerSystem;

        [SerializeField]
        ApplicationEventsSystem m_ApplicationEvents;

        [SerializeField]
        WizardView m_WizardIntroView;

        [SerializeField]
        WizardView m_WizardChecklistView;

        [SerializeField]
        WizardPermissionsView m_WizardPermissionsView;

        [Serializable]
        struct TabletResources
        {
            public WizardView m_WizardIntroView;
            public WizardView m_WizardChecklistView;
            public WizardPermissionsView m_WizardPermissionsView;
        }

        [SerializeField]
        TabletResources m_TabletResources;

        bool m_IsTablet;

        T GetResource<T>(T mobileResource, T tabletResource)
        {
            if (m_IsTablet)
            {
                return tabletResource;
            }
            else
            {
                return mobileResource;
            }
        }

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            m_IsTablet = SystemInfo.deviceModel.Contains("iPad");

            var canvasTransform = m_Canvas.transform;

            // Permissions
            Container.Bind<string>().WithId("AppName").FromInstance("Face Capture");
            Container.DeclareSignal<PermissionsSignals.OpenSystemSettings>();
            Container.DeclareSignal<PermissionsSignals.CameraRequest>();
            Container.DeclareSignal<PermissionsSignals.CameraGranted>();
            Container.DeclareSignal<PermissionsSignals.MicrophoneRequest>();
            Container.BindSignal<PermissionsSignals.OpenSystemSettings>()
                .ToMethod(() => IOSHelper.OpenSettings());
            Container.FixedBindSignal<PermissionsSignals.CameraRequest>()
                .ToMethod<IPermissionsView>((x,s) => x.Show(s.Message, s.Cancelable))
                .FromResolve();
            Container.FixedBindSignal<PermissionsSignals.MicrophoneRequest>()
                .ToMethod<IPermissionsView>((x,s) => x.Show(s.Message, s.Cancelable))
                .FromResolve();

            // Coroutine Runner
            Container.BindInterfacesTo<CoroutineRunner>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("CoroutineRunner").AsSingle();

            // Notification
            Container.BindInterfacesTo<NotificationSystem>().AsSingle();
            Container.DeclareSignal<ShowNotificationSignal>();
            Container.DeclareSignal<ShowModalSignal>();
            Container.FixedBindSignal<ShowNotificationSignal>()
                .ToMethod<INotificationSystem>((x,s) => x.Show(s.value))
                .FromResolve();
            Container.FixedBindSignal<ShowModalSignal>()
                .ToMethod<INotificationSystem>((x,s) => x.Show(s.ToModalPopupData()))
                .FromResolve();

            // Help
            Container.Bind<TextAsset>().WithId("Help").FromInstance(m_HelpData);
            Container.DeclareSignal<HelpSignals.OpenPrivacyPolicy>();
            Container.DeclareSignal<HelpSignals.OpenDocumentation>();
            Container.DeclareSignal<HelpSignals.OpenSupport>();
            Container.BindSignal<HelpSignals.OpenPrivacyPolicy>()
                .ToMethod(() => Application.OpenURL(k_PrivacyPolicyURL));
            Container.BindSignal<HelpSignals.OpenDocumentation>()
                .ToMethod(() => Application.OpenURL(k_DocumentationURL));
            Container.BindSignal<HelpSignals.OpenSupport>()
                .ToMethod(() => Application.OpenURL(k_SupportURL));

            // Server Discovery
            Container.BindInterfacesAndSelfTo<ServerDiscoverySystem>().AsSingle();
            Container.BindInterfacesTo<ServerDiscoveryController>().AsSingle();
            Container.DeclareSignal<ServerDiscoveryUpdatedSignal>();

            // Remote
            Container.BindInterfacesTo<FaceCaptureHostFactory>().AsSingle();
            Container.BindInterfacesTo<RemoteSystem>().AsSingle();
            Container.DeclareSignal<RemoteConnectedSignal>();
            Container.DeclareSignal<RemoteConnectionFailedSignal>();
            Container.DeclareSignal<RemoteDisconnectedSignal>();

            Container.FixedBindSignal<RemoteConnectedSignal>()
                .ToMethod<INotificationSystem>((x,s) => x.Show($"Connected to {s.ip}"))
                .FromResolve();
            Container.FixedBindSignal<RemoteConnectionFailedSignal>()
                .ToMethod<INotificationSystem>((x,s) => x.Show("Could not connect to server"))
                .FromResolve();
			Container.FixedBindSignal<RemoteDisconnectedSignal>()
                .ToMethod<DisconnectedNotificationCommand>((x,s) => x.Notify())
                .FromNew();

            // Companion App Host Listener
            Container.BindInterfacesTo<CompanionAppHostController>().AsSingle();

            // Face
            var faceManagerSystem = GameObject.Instantiate(m_FaceManagerSystem);
            var cameraBackground = faceManagerSystem.GetComponentInChildren<ARCameraBackground>();

            Container.BindInterfacesTo<ARFaceManagerSystem>().FromInstance(faceManagerSystem);
            Container.Bind<ARCameraBackground>().FromInstance(cameraBackground);
            Container.BindInterfacesTo<FaceTracker>().AsSingle();
            Container.BindInterfacesTo<FaceWireframeController>().AsSingle();
            Container.BindInterfacesTo<FaceTrackingNotificationController>().AsSingle();
            Container.BindInterfacesTo<FacePoseSender>().AsSingle();

            // Camera Snapshot
            var cameraSnapshotSystem = cameraBackground.gameObject.AddComponent<CameraSnapshotSystem>();
            Container.BindInterfacesTo<CameraSnapshotSystem>().FromInstance(cameraSnapshotSystem);

            // Reconnect
            Container.FixedBindSignal<ApplicationPauseChangedSignal>()
                .ToMethod<ReconnectCommand>((x,s) => x.OnApplicationPause(s.value))
                .FromNew();

            // Connection
            Container.Bind<ConnectionModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<ConnectionModelController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ConnectionViewController>().AsSingle();
            Container.DeclareSignal<RequestConnectSignal>();
            Container.DeclareSignal<RequestDisconnectSignal>();
            Container.DeclareSignal<SelectServerSignal>();
            Container.DeclareSignal<SetConnectionModeSignal>();

            Container.FixedBindSignal<RequestConnectSignal>()
                .ToMethod<IRemoteSystem>((x,s) => x.Connect(s.Ip, s.Port, s.ClientType))
                .FromResolve();
            Container.FixedBindSignal<RequestDisconnectSignal>()
                .ToMethod<IRemoteSystem>(x => x.Disconnect)
                .FromResolve();

            // Application Events
            Container.Bind<ApplicationEventsSystem>()
                .FromComponentInNewPrefab(m_ApplicationEvents)
                .AsSingle().NonLazy();
            Container.DeclareSignal<ApplicationFocusChangedSignal>().OptionalSubscriber();;
            Container.DeclareSignal<ApplicationPauseChangedSignal>();

            // Audio Recorder
            Container.Bind<AudioRecorder>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("AudioRecorder")
                .AsSingle().NonLazy();

            // Video Recorder
            Container.Bind<VideoRecorder>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("VideoRecorder")
                .AsSingle().NonLazy();

            // Audio And Video Recorder Controller
            Container.BindInterfacesTo<AudioVideoRecorderController>().AsSingle();

            // Settings
            Container.BindInterfacesAndSelfTo<Settings>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsController>().AsSingle();
            Container.DeclareSignal<SettingsSignals.Reset>();
            Container.DeclareSignal<SettingsSignals.UpdateProperty>();
            Container.FixedBindSignal<ApplicationPauseChangedSignal>()
                .ToMethod<Settings>((x,s) => { if (s.value) x.Save(); })
                .FromResolve();

            // Dim Controller
            Container.BindInterfacesTo<DimController>().AsSingle();

            // Auto Hide Controller
            Container.BindInterfacesTo<AutoHideController>().AsSingle();

            // Calibration Controller
            Container.BindInterfacesTo<CalibrationController>().AsSingle();

            // Background Events View
            var backgroundEventsView = GameObject.Instantiate(m_BackgroundEventsView, m_Background);

            Container.BindInterfacesTo<BackgroundEventsView>().FromInstance(backgroundEventsView);
            Container.BindInterfacesTo<BackgroundEventsViewMediator>().AsSingle();
            Container.DeclareSignal<BackgroundEventsSignals.Clicked>();
            Container.DeclareSignal<BackgroundEventsSignals.Touched>();

            // Background View
            var backgroundView = GameObject.Instantiate(m_BackgroundView, m_Background);

            Container.BindInterfacesTo<BackgroundController>().AsSingle();
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.Background)
                .FromInstance(backgroundView);

            // Background overlay
            Container.Bind<IBackgroundOverlayView>().FromInstance(m_BackgroundOverlayView);
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.BackgroundOverlay)
                .FromInstance(m_BackgroundOverlayView);

            // FaceCaptureClient Controller
            Container.BindInterfacesTo<FaceCaptureClientController>().AsSingle();

            // Help View
            var helpView = Instantiate(m_HelpView, m_SafeArea);

            Container.BindInterfacesTo<HelpView>().FromInstance(helpView);
            Container.BindInterfacesTo<HelpViewMediator>().AsSingle();

            // Main View
            var mainView = Instantiate(m_MainView, m_SafeArea);

            Container.BindInterfacesTo<MainView>().FromInstance(mainView);
            Container.BindInterfacesTo<MainViewMediator>().AsSingle();
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.Main)
                .FromInstance(mainView);
            Container.DeclareSignal<MainViewSignals.OpenSettings>();
            Container.DeclareSignal<MainViewSignals.ToggleRecording>();
            Container.DeclareSignal<MainViewSignals.ToggleHelp>();
            Container.DeclareSignal<MainViewSignals.TrackingClicked>();
            Container.DeclareSignal<MainViewSignals.TakesClicked>();

            // Calibration view
            var calibrationView = Instantiate(m_CalibrationView, m_SafeArea);

            Container.BindInterfacesTo<CalibrationView>().FromInstance(calibrationView);
            Container.BindInterfacesTo<CalibrationViewController>().AsSingle();
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.Calibration)
                .FromInstance(calibrationView);
            Container.DeclareSignal<CalibrationViewSignals.PerformCalibration>();
            Container.DeclareSignal<CalibrationViewSignals.ConfirmCalibration>();
            Container.DeclareSignal<CalibrationViewSignals.ClearCalibration>();
            Container.DeclareSignal<CalibrationViewSignals.CalibrationFailed>();
            Container.DeclareSignal<CalibrationViewSignals.CalibrationSucceeded>();

            var calibrationCountdownView = GameObject.Instantiate(m_CountdownView, m_SafeArea);
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.CalibrationCountdown)
                .FromInstance(calibrationCountdownView);
            Container.Bind<ICountdownView>()
                .WithId(FaceClientViewId.CalibrationCountdown)
                .FromInstance(calibrationCountdownView);
            calibrationCountdownView.GetComponent<RectTransform>().StretchToParent();

            // Help Tooltip
            var helpTooltip = Instantiate(m_HelpTooltip, m_SafeArea).GetComponent<HelpTooltip>();
            helpTooltip.GetComponent<RectTransform>().StretchToParent();
            Container.Bind<HelpTooltip>().FromInstance(helpTooltip);

            // Help Mode
            Container.BindInterfacesAndSelfTo<HelpSystem>().AsSingle();
            Container.DeclareSignal<HelpModeSignals.Toggle>();
            Container.DeclareSignal<HelpModeSignals.OpenTooltip>();
            Container.DeclareSignal<HelpModeSignals.CloseTooltip>();
            Container.FixedBindSignal<HelpModeSignals.Toggle>()
                .ToMethod<HelpSystem>((x, s) => x.Toggle(s.value))
                .FromResolve();
            Container.FixedBindSignal<HelpModeSignals.OpenTooltip>()
                .ToMethod<HelpSystem>((x, s) => x.OpenTooltip(s.value))
                .FromResolve();
            Container.FixedBindSignal<HelpModeSignals.CloseTooltip>()
                .ToMethod<HelpSystem>(x => x.CloseTooltip)
                .FromResolve();
            Container.FixedBindSignal<BackgroundEventsSignals.Touched>()
                .ToMethod<HelpSystem>(x => x.CloseTooltip)
                .FromResolve();
            Container.FixedBindSignal<BackgroundEventsSignals.Clicked>()
                .ToMethod<HelpSystem>(x => x.CloseTooltip)
                .FromResolve();

            // Countdown View
            var countdownView = GameObject.Instantiate(m_CountdownView, m_SafeArea);

            countdownView.GetComponent<RectTransform>().StretchToParent();

            Container.BindInterfacesTo<CountdownView>().FromInstance(countdownView);
            Container.BindInterfacesTo<CountdownController>().AsSingle();
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.Countdown)
                .FromInstance(countdownView);

            // Settings View
            var settingsView = GameObject.Instantiate(m_SettingsView, m_SafeArea);

            Container.BindInterfacesTo<SettingsView>().FromInstance(settingsView);
            Container.BindInterfacesTo<SettingsViewMediator>().AsSingle();
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.Settings)
                .FromInstance(settingsView);
            Container.DeclareSignal<SettingsViewSignals.Close>();

            var settingsConnectionView = settingsView.GetComponentInChildren<IConnectionView>();

            InstallConnectionView(Container, settingsConnectionView);

            // Session
            var sessionView = GameObject.Instantiate(m_SessionView, m_SafeArea);
            Container.BindInterfacesTo<SessionView>().FromInstance(sessionView);
            Container.DeclareSignal<SessionSignals.OpenDocumentation>();
            Container.BindSignal<SessionSignals.OpenDocumentation>()
                .ToMethod(() => Application.OpenURL(k_SessionDocumentationURL));
            Container.BindInterfacesTo<SessionViewMediator>().AsSingle();
            Container.BindInterfacesAndSelfTo<SessionViewController>().AsSingle();

            // Launch Screen View
            var launchScreen = GameObject.Instantiate(m_LaunchScreen, m_SafeArea);

            Container.BindInterfacesTo<LaunchScreenView>().FromInstance(launchScreen);
            Container.BindInterfacesTo<LaunchScreenViewMediator>().AsSingle();
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.LaunchScreen)
                .FromInstance(launchScreen);

            var connectionView = launchScreen.GetComponentInChildren<IConnectionView>();

            InstallConnectionView(Container, connectionView);

            // Notification View
            var notificationView = GameObject.Instantiate(m_NotificationView, m_SafeArea);

            Container.BindInterfacesTo<NotificationView>().FromInstance(notificationView);
            Container.Bind<IDialogView>()
                .WithId(FaceClientViewId.Notification)
                .FromInstance(notificationView);

            // Permissions View
            var permissionsView = GameObject.Instantiate(m_PermissionsView, m_SafeArea);

            Container.BindInterfacesTo<PermissionsView>().FromInstance(permissionsView);
            Container.BindInterfacesTo<PermissionsViewMediator>().AsSingle();

            // Wizard Views
            InstallOnboardingWizard(Container, canvasTransform);

            Container.DeclareSignal<WizardSignals.Open>();
            Container.DeclareSignal<WizardSignals.Skip>();
            Container.DeclareSignal<WizardSignals.Next>();


            // Initialize
            Container.BindInterfacesTo<AppInitializer>().AsSingle();
            Container.BindInterfacesTo<CameraPermissionsInitializer>().AsSingle();
        }

        void InstallConnectionView(DiContainer container, IConnectionView instance)
        {
            var subContainer = container.CreateSubContainer();

            subContainer.Bind<IConnectionView>().FromInstance(instance).AsSingle();
            subContainer.BindInterfacesAndSelfTo<ConnectionViewMediator>().AsSingle();

            container.BindInterfacesTo<ConnectionViewMediator>().FromSubContainerResolve().ByInstance(subContainer).AsCached();
            container.Bind<IConnectionView>().FromSubContainerResolve().ByInstance(subContainer).AsCached();
        }

        void InstallOnboardingWizard(DiContainer parentContainer, Transform root)
        {
            var subContainer = parentContainer.CreateSubContainer().WithKernel();

            var wizardPermissionsView = GetResource(m_WizardPermissionsView, m_TabletResources.m_WizardPermissionsView);
            var wizardIntroView = GetResource(m_WizardIntroView, m_TabletResources.m_WizardIntroView);
            var wizardChecklistView = GetResource(m_WizardChecklistView, m_TabletResources.m_WizardChecklistView);
            InstallWizardView<IWizardView, WizardPermissionsView, WizardPermissionsViewMediator>
                (subContainer, wizardPermissionsView, root);
            InstallWizardView<IWizardView, WizardView, WizardViewMediator>
                (subContainer, wizardIntroView, root);
            InstallWizardView<IWizardView, WizardView, WizardViewMediator>
                (subContainer, wizardChecklistView, root);

            subContainer.BindInterfacesAndSelfTo<WizardGroup>().AsSingle();
        }

        void InstallWizardView<InterfaceType, ViewType, MediatorType>(DiContainer parentContainer, ViewType view, Transform root)
            where InterfaceType : IWizardView
            where ViewType : WizardView
            where MediatorType : WizardViewMediator
        {
            var subContainer = parentContainer.CreateSubContainer().WithKernel();

            var viewInstance = GameObject.Instantiate(view, root);
            viewInstance.GetComponent<RectTransform>().StretchToParent();
            viewInstance.Hide();

            subContainer.BindInterfacesAndSelfTo<ViewType>().FromInstance(viewInstance);

            subContainer.BindInterfacesAndSelfTo<MediatorType>().AsSingle();

            parentContainer.Bind<InterfaceType>().FromSubContainerResolve().ByInstance(subContainer).AsCached();
        }
    }
}
