using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Unity.LiveCapture.VirtualCamera;
using Unity.TouchFramework;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    [Serializable]
    class TabletResources
    {
        [SerializeField]
        Camera m_MainCamera;
        [SerializeField]
        Gamepad.GamepadDriver m_GamepadDriver;
        [SerializeField]
        RawImage m_VideoStreamDisplay;
        [SerializeField]
        PermissionsView m_PermissionsView;
        [SerializeField]
        SessionView m_SessionView;
        [SerializeField]
        GameObject m_MainViewPrefab;
        [SerializeField]
        GameObject m_RigSettingsPrefab;
        [SerializeField]
        GameObject m_JoystickSettingsPrefab;
        [SerializeField]
        GameObject m_GamepadSettingsPrefab;
        [SerializeField]
        GamepadLayoutView m_GamepadLayoutView;
        [SerializeField]
        GameObject m_FocalLengthDialPrefab;
        [SerializeField]
        GameObject m_FocusApertureDialPrefab;
        [SerializeField]
        GameObject m_FocusModePrefab;
        [SerializeField]
        ConnectionView m_ConnectionViewPrefab;
        [SerializeField]
        DialogView m_PopupPrefab;
        [SerializeField]
        ConnectionScreenView m_ConnectionScreenViewPrefab;
        [SerializeField]
        NotificationView m_NotificationViewPrefab;
        [SerializeField]
        SettingsView m_SettingsView;
        [SerializeField]
        ResetView m_ResetView;
        [SerializeField]
        OrphanTouch m_OrphanTouch;
        [SerializeField]
        FocusReticle m_FocusReticle;
        [SerializeField]
        TextAsset m_HelpData;
        [SerializeField]
        HelpTooltip m_HelpTooltip;
        [SerializeField]
        HelpView m_HelpView;
        [SerializeField]
        CountdownView m_CountdownView;
        [SerializeField]
        TakeIterationView m_TakeIterationView;
        [SerializeField]
        TakeSelectionView m_TakeSelectionView;
        [SerializeField]
        TakeMetadataView m_TakeMetadataView;
        [SerializeField]
        TakeRenameView m_TakeRenameView;
        [SerializeField]
        TakeDeleteView m_TakeDeleteView;
        [SerializeField]
        GameObject m_ARSession;
        [SerializeField]
        WizardView m_WizardIntroView;
        [SerializeField]
        WizardView m_WizardChecklistView;
        [SerializeField]
        WizardPermissionsView m_WizardPermissionsView;

        /*
        public Vector2 referenceResolution;
        public Vector2 authoringResolution;
        public TakePlayerView takePlayerView;
        public FocusReticleView focusReticleView;
        public Button backgroundButton;
        public HelpDialogData helpData;
        public GameObject ARSession;
        */

        public void Install(DiContainer container, CanvasComponents canvasComponent)
        {
            container.Bind<TextAsset>().WithId("Help").FromInstance(m_HelpData);

            m_ARSession.SetActive(true);

            Gamepad.UIInputUtility.LimitToScheme(Gamepad.UIInputUtility.TouchScheme);

            // Main Camera
            container.Bind<Camera>().WithId("Main Camera").FromInstance(m_MainCamera);

            InstallVideoStreamView(container, canvasComponent);

            var canvasTransform = canvasComponent.canvas.transform;

            // Orphan Touch
            var orphanTouchInstance = GameObject.Instantiate(m_OrphanTouch, canvasTransform);
            container.Bind<IOrphanTouch>().FromInstance(orphanTouchInstance);
            orphanTouchInstance.GetComponent<RectTransform>().StretchToParent();

            container.BindInterfacesTo<OrphanTouchMediator>().AsSingle();

            // Gamepad
            var gamepadDriver = GameObject.Instantiate(m_GamepadDriver, canvasComponent.canvas.transform);
            container.Bind<Gamepad.IGamepadDriver>().FromInstance(gamepadDriver.GetComponent<Gamepad.GamepadDriver>());
            container.BindInterfacesTo<Gamepad.GamepadDriverMediator>().AsSingle();

            //
            var focusReticleInstance = GameObject.Instantiate(m_FocusReticle, canvasComponent.canvas.transform);
            container.Bind<IFocusReticle>().FromInstance(focusReticleInstance);

            // Help View
            var helpViewInstance = GameObject.Instantiate(m_HelpView, canvasTransform);
            helpViewInstance.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesTo<HelpView>().FromInstance(helpViewInstance);
            container.BindInterfacesTo<HelpViewMediator>().AsSingle();

            var helpBackgroundLayout = helpViewInstance.GetComponent<VerticalLayoutGroup>();
            var padding = helpBackgroundLayout.padding;
            padding.top = 70;
            helpBackgroundLayout.padding = padding;

            helpViewInstance.Hide();

            // Main View
            var instance = GameObject.Instantiate(m_MainViewPrefab, canvasTransform);
            instance.GetComponent<RectTransform>().StretchToParent();

            container.BindInterfacesTo<MainView>().FromInstance(instance.GetComponent<MainView>());

            container.BindInterfacesTo<TimeControlView>().FromInstance(instance.GetComponentInChildren<TimeControlView>());

            container.BindInterfacesAndSelfTo<TakeLibraryView>().FromInstance(instance.GetComponentInChildren<TakeLibraryView>(true));

            container.DeclareSignal<MainViewSignals.ToggleDeviceMode>();
            container.DeclareSignal<MainViewSignals.ToggleRecording>();

            var joysticksViewInstance = instance.GetComponentInChildren<JoysticksView>();
            container.Bind<IJoysticksView>().FromInstance(joysticksViewInstance);
            container.BindInterfacesTo<JoysticksController>().AsSingle();

            // Rig Settings
            instance = GameObject.Instantiate(m_RigSettingsPrefab, canvasTransform);
            var rigSettingsView = instance.GetComponent<RigSettingsView>();
            container.Bind<IRigSettingsView>().FromInstance(rigSettingsView);
            container.BindInterfacesAndSelfTo<RigSettingsMediator>().AsSingle();

            container.Bind<IPositionView>().FromInstance(instance.GetComponentInChildren<PositionView>(true));
            container.Bind<IRotationLockView>().FromInstance(instance.GetComponentInChildren<RotationLockView>(true));
            container.Bind<IDampingView>().FromInstance(instance.GetComponentInChildren<DampingView>(true));

            // Joystick Settings
            instance = GameObject.Instantiate(m_JoystickSettingsPrefab, canvasTransform);
            var joystickSettingsView = instance.GetComponent<JoystickSettingsView>();
            container.Bind<IJoystickSettingsView>().FromInstance(joystickSettingsView);
            container.BindInterfacesAndSelfTo<JoystickSettingsMediator>().AsSingle();

            container.Bind<IJoystickGeneralTabView>().FromInstance(instance.GetComponentInChildren<JoystickGeneralTabView>(true));

            // Gamepad Settings
            instance = GameObject.Instantiate(m_GamepadSettingsPrefab, canvasTransform);
            var gamepadSettingsView = instance.GetComponent<GamepadSettingsView>();
            container.Bind<IGamepadSettingsView>().FromInstance(gamepadSettingsView);
            container.BindInterfacesAndSelfTo<GamepadSettingsMediator>().AsSingle();

            container.Bind<IGamepadConfigurationTabView>().FromInstance(instance.GetComponentInChildren<GamepadConfigurationTabView>(true));
            container.Bind<IGamepadPositionTabView>().FromInstance(instance.GetComponentInChildren<GamepadPositionTabView>(true));
            container.Bind<IGamepadRotationTabView>().FromInstance(instance.GetComponentInChildren<GamepadRotationTabView>(true));
            container.Bind<IGamepadSlidersTabView>().FromInstance(instance.GetComponentInChildren<GamepadSlidersTabView>(true));

            container.Bind<TakeMetadataView>().FromInstance(
                GameObject.Instantiate(m_TakeMetadataView, canvasTransform).GetComponent<TakeMetadataView>());

            container.Bind<TakeRenameView>().FromInstance(
                GameObject.Instantiate(m_TakeRenameView, canvasTransform).GetComponent<TakeRenameView>());

            container.Bind<TakeDeleteView>().FromInstance(
                GameObject.Instantiate(m_TakeDeleteView, canvasTransform).GetComponent<TakeDeleteView>());

            container.BindInterfacesTo<FocalLengthView>().FromInstance(
                GameObject.Instantiate(m_FocalLengthDialPrefab, canvasTransform).GetComponent<FocalLengthView>());

            container.BindInterfacesTo<FocusApertureView>().FromInstance(
                GameObject.Instantiate(m_FocusApertureDialPrefab, canvasTransform).GetComponent<FocusApertureView>());

            container.BindInterfacesTo<FocusModeView>().FromInstance(
                GameObject.Instantiate(m_FocusModePrefab, canvasTransform).GetComponent<FocusModeView>());

            container.BindInterfacesTo<ResetView>().FromInstance(
                GameObject.Instantiate(m_ResetView, canvasTransform).GetComponent<ResetView>());

            container.BindInterfacesAndSelfTo<TakeIterationView>().FromInstance(
                GameObject.Instantiate(m_TakeIterationView, canvasTransform).GetComponent<TakeIterationView>());

            container.BindInterfacesAndSelfTo<TakeSelectionView>().FromInstance(
                GameObject.Instantiate(m_TakeSelectionView, canvasTransform).GetComponent<TakeSelectionView>());

            var settingsViewInstance = GameObject.Instantiate(m_SettingsView, canvasTransform);
            container.Bind<ISettingsView>().FromInstance(settingsViewInstance);
            settingsViewInstance.GetRectTransform().anchorMin = Vector2.up;
            settingsViewInstance.GetRectTransform().anchorMax = Vector2.up;
            settingsViewInstance.GetComponent<Image>().enabled = false;

            var settingsConnectionView = settingsViewInstance.GetComponentInChildren<ConnectionView>();
            Assert.IsNotNull(settingsConnectionView);
            InstallConnectionView(container, settingsConnectionView);

            var countdownViewInstance = GameObject.Instantiate(m_CountdownView, canvasComponent.canvas.transform);
            container.BindInterfacesTo<CountdownView>().FromInstance(countdownViewInstance);
            countdownViewInstance.GetComponent<RectTransform>().StretchToParent();

            var helpTooltip = GameObject.Instantiate(m_HelpTooltip, canvasTransform).GetComponent<HelpTooltip>();
            container.Bind<HelpTooltip>().FromInstance(helpTooltip);

            // Gamepad Layout View
            var gamepadLayoutViewInstance = GameObject.Instantiate(m_GamepadLayoutView, canvasTransform);
            gamepadLayoutViewInstance.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesTo<GamepadLayoutView>().FromInstance(gamepadLayoutViewInstance);
            container.BindInterfacesTo<GamepadLayoutViewMediator>().AsSingle();
            gamepadLayoutViewInstance.Hide();

            // Session View
            var sessionView = GameObject.Instantiate(m_SessionView, canvasTransform);

            container.BindInterfacesTo<SessionView>().FromInstance(sessionView);

            InstallConnectionScreen(container, canvasComponent);

            //Foreground layer of the app
            var notificationViewInstance = GameObject.Instantiate(m_NotificationViewPrefab, canvasTransform);
            container.Bind<INotificationView>().FromInstance(notificationViewInstance);
            notificationViewInstance.GetComponent<RectTransform>().StretchToParent();

            // Permissions View
            var permissionsView = GameObject.Instantiate(m_PermissionsView, canvasTransform);

            container.BindInterfacesTo<PermissionsView>().FromInstance(permissionsView);
            container.BindInterfacesTo<PermissionsViewMediator>().AsSingle();

            // Wizard Views
            InstallOnboardingWizard(container, canvasComponent);

            container.DeclareSignal<WizardSignals.Open>();
            container.DeclareSignal<WizardSignals.Skip>();
            container.DeclareSignal<WizardSignals.Next>();
        }

        void InstallVideoStreamView(DiContainer container, CanvasComponents canvasComponents)
        {
            // We need to disable the gameobject so awake is not called before we set the stream provider.
            m_VideoStreamDisplay.gameObject.SetActive(false);
            var videoStreamDisplayInstance = GameObject.Instantiate(m_VideoStreamDisplay, canvasComponents.canvas.transform);
            videoStreamDisplayInstance.gameObject.SetActive(true);
            m_VideoStreamDisplay.gameObject.SetActive(true);
            container.Bind<RawImage>().WithId("Video Streaming").FromInstance(videoStreamDisplayInstance);
            videoStreamDisplayInstance.transform.SetParent(canvasComponents.canvas.transform);
            videoStreamDisplayInstance.GetComponent<RectTransform>().StretchToParent();
            videoStreamDisplayInstance.GetComponent<RectTransform>().localScale = new Vector3(1, -1, 1);
        }

        ConnectionView InstantiateAndInstallConnectionView(DiContainer container, CanvasComponents canvasComponent)
        {
            var cavasTransform = canvasComponent.canvas.transform;
            var connectionViewInstance = GameObject.Instantiate(m_ConnectionViewPrefab, cavasTransform);

            InstallConnectionView(container, connectionViewInstance);

            return connectionViewInstance;
        }

        void InstallConnectionView(DiContainer container, IConnectionView instance)
        {
            var subContainer = container.CreateSubContainer();

            subContainer.Bind<IConnectionView>().FromInstance(instance).AsSingle();
            subContainer.BindInterfacesAndSelfTo<ConnectionViewMediator>().AsSingle();

            container.BindInterfacesTo<ConnectionViewMediator>().FromSubContainerResolve().ByInstance(subContainer).AsCached();
            container.Bind<IConnectionView>().FromSubContainerResolve().ByInstance(subContainer).AsCached();
        }

        void InstallConnectionScreen(DiContainer container, CanvasComponents canvasComponents)
        {
            var connectionViewInstance = InstantiateAndInstallConnectionView(container, canvasComponents);
            var connectionScreenViewInstance = GameObject.Instantiate(m_ConnectionScreenViewPrefab, canvasComponents.canvas.transform);

            connectionScreenViewInstance.GetComponent<RectTransform>().StretchToParent();

            var dialogDone = connectionScreenViewInstance.DialogDone;
            var connectionViewRectTransform = connectionViewInstance.GetComponent<RectTransform>();

            connectionViewRectTransform.SetParent(dialogDone.Contents, false);
            connectionViewRectTransform.StretchToParent();

            container.Bind<IConnectionScreenView>().FromInstance(connectionScreenViewInstance);
            container.BindInterfacesAndSelfTo<ConnectionScreenMediator>().AsSingle();
            container.DeclareSignal<ConnectionScreenViewSignals.Close>();
        }

        void InstallOnboardingWizard(DiContainer parentContainer, CanvasComponents canvasComponents)
        {
            var subContainer = parentContainer.CreateSubContainer().WithKernel();

            InstallWizardView<IWizardView, WizardPermissionsView, WizardPermissionsViewMediator>
                (subContainer, m_WizardPermissionsView, canvasComponents);
            InstallWizardView<IWizardView, WizardView, WizardViewMediator>
                (subContainer, m_WizardIntroView, canvasComponents);
            InstallWizardView<IWizardView, WizardView, WizardViewMediator>
                (subContainer, m_WizardChecklistView, canvasComponents);

            subContainer.BindInterfacesAndSelfTo<WizardGroup>().AsSingle();
        }

        void InstallWizardView<InterfaceType, ViewType, MediatorType>(DiContainer parentContainer, ViewType view, CanvasComponents canvasComponents)
            where InterfaceType : IWizardView
            where ViewType : WizardView
            where MediatorType : WizardViewMediator
        {
            var subContainer = parentContainer.CreateSubContainer().WithKernel();

            var viewInstance = GameObject.Instantiate(view, canvasComponents.canvas.transform);
            viewInstance.GetComponent<RectTransform>().StretchToParent();
            viewInstance.Hide();

            subContainer.BindInterfacesAndSelfTo<ViewType>().FromInstance(viewInstance);

            subContainer.BindInterfacesAndSelfTo<MediatorType>().AsSingle();

            parentContainer.Bind<InterfaceType>().FromSubContainerResolve().ByInstance(subContainer).AsCached();
        }
    }
}
