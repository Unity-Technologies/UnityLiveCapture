using System;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Unity.LiveCapture;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    [Serializable]
    public class CanvasComponents
    {
        public Canvas canvas;

        public CanvasScaler canvasScaler;
    }

    public class MobileInstaller : MonoInstaller
    {
        const string k_PrivacyPolicyURL = "https://unity3d.com/legal/privacy-policy";
        const string k_DocumentationURL = "https://docs.unity3d.com/Packages/com.unity.live-capture@2.0/manual/virtual-camera.html";
        const string k_SupportURL = "https://forum.unity.com/threads/1111255/";
        const string k_SessionDocumentationURL = "https://docs.unity3d.com/Packages/com.unity.live-capture@2.0/manual/ref-component-virtual-camera-device.html";

        const Platform k_Platform = Platform.Phone;

        [SerializeField]
        CanvasComponents m_CanvasComponents;

        [SerializeField]
        MobileResources m_MobileResources;

        [SerializeField]
        Transform m_ARCamera;

        [SerializeField]
        ApplicationEventsSystem m_ApplicationEvents;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.Bind<Platform>().FromInstance(k_Platform);

            Container.Bind<Canvas>().FromInstance(m_CanvasComponents.canvas).AsSingle();

            Assert.IsNotNull(m_ARCamera, "AR camera not assigned");
            Container.Bind<Transform>().WithId("AR camera").FromInstance(m_ARCamera);

            // Permissions
            Container.Bind<string>().WithId("AppName").FromInstance("Virtual Camera");
            Container.DeclareSignal<PermissionsSignals.OpenSystemSettings>();
            Container.DeclareSignal<PermissionsSignals.CameraRequest>();
            Container.DeclareSignal<PermissionsSignals.CameraGranted>();
            Container.BindSignal<PermissionsSignals.OpenSystemSettings>()
                .ToMethod(() => IOSHelper.OpenSettings());
            Container.FixedBindSignal<PermissionsSignals.CameraRequest>()
                .ToMethod<IPermissionsView>((x, s) => x.Show(s.Message, s.Cancelable))
                .FromResolve();

            // Coroutine Runner
            Container.BindInterfacesTo<CoroutineRunner>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("CoroutineRunner").AsSingle();

            // Scene State
            Container.BindInterfacesTo<SceneState>().AsSingle();

            // Notification
            Container.BindInterfacesTo<NotificationSystem>().AsSingle();
            Container.DeclareSignal<ShowNotificationSignal>();
            Container.DeclareSignal<ShowModalSignal>();
            Container.FixedBindSignal<ShowNotificationSignal>()
                .ToMethod<INotificationSystem>((x, s) => x.Show(s.value))
                .FromResolve();
            Container.FixedBindSignal<ShowModalSignal>()
                .ToMethod<INotificationSystem>((x, s) => x.Show(s.ToModalPopupData()))
                .FromResolve();

            // Client Controller
            Container.BindInterfacesTo<VirtualCameraClientController>().AsSingle();

            // Help
            Container.DeclareSignal<HelpSignals.OpenPrivacyPolicy>();
            Container.DeclareSignal<HelpSignals.OpenDocumentation>();
            Container.DeclareSignal<HelpSignals.OpenSupport>();
            Container.BindSignal<HelpSignals.OpenPrivacyPolicy>()
                .ToMethod(() => Application.OpenURL(k_PrivacyPolicyURL));
            Container.BindSignal<HelpSignals.OpenDocumentation>()
                .ToMethod(() => Application.OpenURL(k_DocumentationURL));
            Container.BindSignal<HelpSignals.OpenSupport>()
                .ToMethod(() => Application.OpenURL(k_SupportURL));

            // Session
            Container.DeclareSignal<SessionSignals.OpenDocumentation>();
            Container.BindSignal<SessionSignals.OpenDocumentation>()
                .ToMethod(() => Application.OpenURL(k_SessionDocumentationURL));
            Container.BindInterfacesTo<SessionViewMediator>().AsSingle();
            Container.BindInterfacesAndSelfTo<SessionViewController>().AsSingle();

            // Server Discovery
            Container.BindInterfacesAndSelfTo<ServerDiscoverySystem>().AsSingle();
            Container.BindInterfacesTo<ServerDiscoveryController>().AsSingle();
            Container.DeclareSignal<ServerDiscoveryUpdatedSignal>();

            // Remote
            Container.BindInterfacesTo<VirtualCameraHostFactory>().AsSingle();
            Container.BindInterfacesTo<RemoteSystem>().AsSingle();
            Container.DeclareSignal<RemoteConnectedSignal>();
            Container.DeclareSignal<RemoteConnectionFailedSignal>();
            Container.DeclareSignal<RemoteDisconnectedSignal>();

            Container.FixedBindSignal<RemoteConnectedSignal>()
                .ToMethod<INotificationSystem>((x, s) => x.Show($"Connected to {s.ip}"))
                .FromResolve();
            Container.FixedBindSignal<RemoteConnectionFailedSignal>()
                .ToMethod<INotificationSystem>((x, s) => x.Show("Could not connect to server"))
                .FromResolve();
            Container.FixedBindSignal<RemoteDisconnectedSignal>()
                .ToMethod<DisconnectedNotificationCommand>((x, s) => x.Notify())
                .FromNew();

            // Reconnect
            Container.FixedBindSignal<ApplicationPauseChangedSignal>()
                .ToMethod<ReconnectCommand>((x, s) => x.OnApplicationPause(s.value))
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
                .ToMethod<IRemoteSystem>((x, s) => x.Connect(s.Ip, s.Port, s.ClientType))
                .FromResolve();
            Container.FixedBindSignal<RequestDisconnectSignal>()
                .ToMethod<IRemoteSystem>(x => x.Disconnect)
                .FromResolve();

            // Application Events
            Container.Bind<ApplicationEventsSystem>()
                .FromComponentInNewPrefab(m_ApplicationEvents)
                .AsSingle().NonLazy();
            Container.DeclareSignal<ApplicationFocusChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ApplicationPauseChangedSignal>();

            // Timecode
            Container.DeclareSignal<SetTimecodeSourceSignal>();
            Container.FixedBindSignal<SetTimecodeSourceSignal>()
                .ToMethod<ITimeSystem>((x, s) => x.TimecodeSource = TimecodeSourceManager.Instance[s.Id])
                .FromResolve();

            // Companion App Host
            Container.BindInterfacesTo<CompanionAppHostController>().AsSingle();

            // Take Library
            Container.BindInterfacesAndSelfTo<TakeLibraryModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<TakeLibraryController>().AsSingle();
            Container.BindInterfacesAndSelfTo<TakeIterationController>().AsSingle();
            Container.DeclareSignal<TakeDescriptorsChangedSignal>();
            Container.DeclareSignal<TakeMetadataDescriptorsChangedSignal>();
            Container.DeclareSignal<ShowTakeIterationSignal>();
            Container.DeclareSignal<ShowTakeIterationOrHelpSignal>();
            Container.DeclareSignal<ShowTakeLibrarySignal>();

            // Orphan Touch
            Container.DeclareSignal<OrphanTouchSignal>();
            Container.FixedBindSignal<OrphanTouchSignal>().ToMethod<CloseLensSettingsCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<OrphanTouchSignal>()
                .ToMethod<HelpSystem>(x => x.CloseTooltip)
                .FromResolve();

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

            // Joystick stuff
            Container.BindInterfacesTo<JoysticksMediator>().AsSingle();
            Container.DeclareSignal<SetJoystickVerticalSignal>();
            Container.DeclareSignal<SetJoystickLateralSignal>();
            Container.DeclareSignal<SetJoystickForwardSignal>();
            Container.DeclareSignal<SetGamepadVerticalSignal>();
            Container.DeclareSignal<SetGamepadLateralSignal>();
            Container.DeclareSignal<SetGamepadForwardSignal>();
            Container.DeclareSignal<SetGamepadTiltSignal>();
            Container.DeclareSignal<SetGamepadPanSignal>();
            Container.DeclareSignal<SetGamepadRollSignal>();
            Container.Bind<SetJoystickValueCommand>().AsSingle();
            Container.FixedBindSignal<SetJoystickLateralSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetJoystickLateral)
                .FromResolve();
            Container.FixedBindSignal<SetJoystickVerticalSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetJoystickVertical)
                .FromResolve();
            Container.FixedBindSignal<SetJoystickForwardSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetJoystickForward)
                .FromResolve();
            Container.FixedBindSignal<SetGamepadLateralSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetGamepadLateral)
                .FromResolve();
            Container.FixedBindSignal<SetGamepadVerticalSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetGamepadVertical)
                .FromResolve();
            Container.FixedBindSignal<SetGamepadForwardSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetGamepadForward)
                .FromResolve();
            Container.FixedBindSignal<SetGamepadTiltSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetGamepadTilt)
                .FromResolve();
            Container.FixedBindSignal<SetGamepadPanSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetGamepadPan)
                .FromResolve();
            Container.FixedBindSignal<SetGamepadRollSignal>()
                .ToMethod<SetJoystickValueCommand>(x => x.SetGamepadRoll)
                .FromResolve();

            //Services
            Container.BindInterfacesTo<PreviewManager>().AsSingle().WithArguments((uint)64);
            Container.BindInterfacesAndSelfTo<ARSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<VideoStreamingSystem>().AsSingle();
            Container.Bind<DeviceDataSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<TimeSystem>().AsSingle();
            Container.Bind<StateModel>().AsSingle();
            Container.Bind<SettingsModel>().AsSingle();
            Container.Bind<CameraModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<Gamepad.GamepadSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<Gamepad.GamepadSerializer>().AsSingle();
            Container.BindInterfacesAndSelfTo<Gamepad.ActionProcessor>().AsSingle();
            Container.BindInterfacesAndSelfTo<Gamepad.ActionNotifier>().AsSingle();

            // Signals
            Container.DeclareSignal<VideoStreamChangedSignal>();
            Container.DeclareSignal<ResetLensSignal>();
            Container.DeclareSignal<RequestStartRecordingSignal>();
            Container.DeclareSignal<RequestStopRecordingSignal>();

            Container.DeclareSignal<SetToCurrentTiltSignal>();
            Container.DeclareSignal<SetMainViewOptionSignal>();
            Container.DeclareSignal<SetRecordingCountdownEnabledSignal>();
            Container.DeclareSignal<SetRecordingCountdownDurationSignal>();

            Container.DeclareSignal<SendHostSignal>();
            Container.DeclareSignal<ReceiveHostSignal>();

            Container.DeclareSignal<HideMainViewSignal>();

            Container.DeclareSignal<SendChannelFlagsSignal>();

            Container.DeclareSignal<TogglePlaybackSignal>();
            Container.DeclareSignal<SkipFramesSignal>();

            // Signal to commands
            Container.FixedBindSignal<RemoteConnectedSignal>().ToMethod<ConnectionSuccessCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<RemoteDisconnectedSignal>().ToMethod<DisconnectedCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<VideoStreamChangedSignal>().ToMethod<VideoStreamChangedCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<ResetLensSignal>().ToMethod<ResetLensCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<RequestStartRecordingSignal>().ToMethod<RequestStartRecordingCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<RequestStopRecordingSignal>().ToMethod<RequestStopRecordingCommand>(x => x.Execute).FromNew();

            Container.FixedBindSignal<SetToCurrentTiltSignal>().ToMethod<SetToCurrentTiltCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<SetMainViewOptionSignal>().ToMethod<SetMainViewOptionCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<SetRecordingCountdownEnabledSignal>().ToMethod<SetRecordingCountdownEnabledCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<SetRecordingCountdownDurationSignal>().ToMethod<SetRecordingCountdownDurationCommand>(x => x.Execute).FromNew();

            Container.FixedBindSignal<SendHostSignal>().ToMethod<SendSettingCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<ReceiveHostSignal>().ToMethod<ReceiveSettingCommand>(x => x.Execute).FromNew();

            // ------
            Container.BindInterfacesTo<HostRecordingChangedCommand>().AsSingle();

            Container.FixedBindSignal<SendChannelFlagsSignal>().ToMethod<TakesCommand>(x => x.SendChannelFlags).FromNew();

            Container.FixedBindSignal<TogglePlaybackSignal>().ToMethod<TogglePlaybackCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<SkipFramesSignal>().ToMethod<SkipFramesCommand>(x => x.Execute).FromNew();

            Container.FixedBindSignal<ShowTakeIterationSignal>().ToMethod<ShowTakeIterationCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<ShowTakeIterationOrHelpSignal>().ToMethod<ShowTakeIterationOrHelpCommand>(x => x.Execute).FromNew();
            Container.FixedBindSignal<ShowTakeLibrarySignal>().ToMethod<ShowTakeLibraryCommand>(x => x.Execute).FromNew();

            m_MobileResources.Install(Container, m_CanvasComponents);

            // Inititialize
            Container.BindInterfacesTo<AppInitializer>().AsSingle();
            Container.BindInterfacesTo<CameraPermissionsInitializer>().AsSingle();

            Container.Bind<AppDisposer>().AsSingle();

            InitExecutionOrder();
        }

        void InitExecutionOrder()
        {
            Container.BindTickableExecutionOrder<ARSystem>(-10);

            Container.BindDisposableExecutionOrder<SceneState>(int.MaxValue);
        }
    }
}
