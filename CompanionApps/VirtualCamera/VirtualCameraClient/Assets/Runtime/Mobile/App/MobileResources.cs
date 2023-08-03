using System;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    [Serializable]
    class MobileResources
    {
        [SerializeField]
        Vector2 m_ReferenceResolution;

        [SerializeField]
        Vector2 m_AuthoringResolution;

        [SerializeField]
        Camera MainCamera;

        [SerializeField]
        Gamepad.GamepadDriver m_GamepadDriver;

        [SerializeField]
        GamepadLayoutView m_GamepadLayoutView;

        [SerializeField]
        PermissionsView m_PermissionsView;

        [SerializeField]
        SessionView m_SessionView;

        [SerializeField]
        ConnectionScreenView m_ConnectionScreenView;

        [SerializeField]
        ConnectionView m_ConnectionView;

        [SerializeField]
        MainViewMobile m_MainView;

        [SerializeField]
        RawImage m_VideoStreamDisplay;

        [SerializeField]
        NotificationView m_NotificationView;

        [SerializeField]
        CameraLensSettingsView m_LensSettingsView;

        [SerializeField]
        RigSettingsView m_RigSettingsView;

        [SerializeField]
        JoystickSettingsView m_JoystickSettingsView;

        [SerializeField]
        GamepadSettingsView m_GamepadSettingsView;

        [SerializeField]
        SettingsView m_SettingsView;

        [SerializeField]
        FocusReticle m_FocusReticle;

        [SerializeField]
        CountdownView m_CountDownView;

        [SerializeField]
        ResetView m_ResetView;

        [SerializeField]
        OrphanTouch m_OrphanTouch;

        [SerializeField]
        TextAsset m_HelpData;

        [SerializeField]
        GameObject m_ARSession;

        [SerializeField]
        SafeAreaRect m_SafeArea;

        [SerializeField]
        HelpTooltip m_HelpTooltip;

        [SerializeField]
        HelpView m_HelpView;

        [SerializeField]
        TakeLibraryView m_TakeLibraryView;

        [SerializeField]
        TakeDeleteView m_TakeDeletedView;

        [SerializeField]
        TakeMetadataView m_TakeMetadataView;

        [SerializeField]
        TakeRenameView m_TakeRenameView;

        [SerializeField]
        TakeIterationView m_TakeIterationView;

        [SerializeField]
        TakeSelectionViewFullScreen m_TakeSelectionView;

        [SerializeField]
        WizardView m_WizardIntroView;

        [SerializeField]
        WizardView m_WizardChecklistView;

        [SerializeField]
        WizardPermissionsView m_WizardPermissionsView;

        public void Install(DiContainer container, CanvasComponents canvasComponent)
        {
            container.Bind<TextAsset>().WithId("Help").FromInstance(m_HelpData);

            var canvasTransform = canvasComponent.canvas.transform;

            m_ARSession.SetActive(true);

            Gamepad.UIInputUtility.LimitToScheme(Gamepad.UIInputUtility.TouchScheme);

            // Main Camera
            container.Bind<Camera>().WithId("Main Camera").FromInstance(MainCamera);

            // Video Stream Display
            // We need to disable the gameobject so awake is not called before we set the stream provider.
            m_VideoStreamDisplay.gameObject.SetActive(false);
            var videoStreamDisplay = GameObject.Instantiate(m_VideoStreamDisplay, canvasTransform);
            videoStreamDisplay.gameObject.SetActive(true);
            m_VideoStreamDisplay.gameObject.SetActive(true);
            container.Bind<RawImage>().WithId("Video Streaming").FromInstance(videoStreamDisplay);
            videoStreamDisplay.transform.SetParent(canvasTransform);
            videoStreamDisplay.GetComponent<RectTransform>().StretchToParent();
            videoStreamDisplay.GetComponent<RectTransform>().localScale = new Vector3(1, -1, 1);

            // Gamepad
            var gamepadDriver = GameObject.Instantiate(m_GamepadDriver, canvasComponent.canvas.transform);
            container.Bind<Gamepad.IGamepadDriver>().FromInstance(gamepadDriver.GetComponent<Gamepad.GamepadDriver>());
            container.BindInterfacesTo<Gamepad.GamepadDriverMediator>().AsSingle();

            // Focus Reticle
            var focusReticle = GameObject.Instantiate(m_FocusReticle, canvasComponent.canvas.transform);
            container.BindInterfacesTo<FocusReticle>().FromInstance(focusReticle);
            container.BindInterfacesTo<FocusReticleController>().AsSingle();

            // Orphan Touch
            var orphanTouch = GameObject.Instantiate(m_OrphanTouch, canvasComponent.canvas.transform);
            container.Bind<IOrphanTouch>().FromInstance(orphanTouch);
            orphanTouch.GetComponent<RectTransform>().StretchToParent();

            container.BindInterfacesAndSelfTo<OrphanTouchMediator>().AsSingle();

            // Help View
            var helpView = GameObject.Instantiate(m_HelpView, canvasTransform);
            helpView.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesTo<HelpView>().FromInstance(helpView);
            container.BindInterfacesTo<HelpViewMediator>().AsSingle();
            helpView.Hide();

            // Main View
            var mainView = GameObject.Instantiate(m_MainView, canvasTransform);
            container.BindInterfacesTo<MainViewMobile>().FromInstance(mainView);
            mainView.GetComponent<RectTransform>().StretchToParent();

            container.BindInterfacesTo<MainViewMobileMediator>().AsSingle();
            container.DeclareSignal<MainViewSignals.OpenLensSettingsView>();
            container.DeclareSignal<MainViewSignals.OpenRigSettingsView>();
            container.DeclareSignal<MainViewSignals.OpenJoystickSettingsView>();
            container.DeclareSignal<MainViewSignals.OpenSettingsView>();
            container.DeclareSignal<MainViewSignals.ToggleDeviceMode>();
            container.DeclareSignal<MainViewSignals.ToggleHelp>();
            container.DeclareSignal<MainViewSignals.ToggleRecording>();
            container.DeclareSignal<MainViewSignals.ToggleResetView>();
            container.DeclareSignal<MainViewSignals.ResetViewOptionSelected>();

            // Time Control View
            container.BindInterfacesTo<TimeControlView>().FromInstance(
                mainView.GetComponentInChildren<TimeControlView>());
            container.BindInterfacesTo<TimeControlViewMediator>().AsSingle();

            container.Bind<TakeIterationToggle>().FromInstance(
                mainView.GetComponentInChildren<TakeIterationToggle>(true));

            // Joysticks View
            var joysticksView = mainView.GetComponentInChildren<JoysticksView>(true);
            container.Bind<IJoysticksView>().FromInstance(joysticksView);
            container.BindInterfacesTo<JoysticksController>().AsSingle();

            // Reset View
            var resetView = GameObject.Instantiate(m_ResetView, canvasTransform);
            container.BindInterfacesTo<ResetView>().FromInstance(resetView);
            container.BindInterfacesTo<ResetViewMediator>().AsSingle();

            //Middle layer of the app
            m_SafeArea.transform.SetAsLastSibling();

            // Lens Settings View
            var lensSettingsView = GameObject.Instantiate(m_LensSettingsView, m_SafeArea.transform);
            container.BindInterfacesTo<CameraLensSettingsView>().FromInstance(lensSettingsView);
            lensSettingsView.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesTo<CameraLensSettingsMediator>().AsSingle();
            container.DeclareSignal<LensSettingsViewSignals.Close>();

            // Focus Mode View
            container.Bind<IFocusModeView>()
                .FromInstance(lensSettingsView.GetComponentInChildren<IFocusModeView>(true));
            container.BindInterfacesTo<FocusModeViewMediator>().AsSingle();

            // Rig Settings View
            var rigSettingView = GameObject.Instantiate(m_RigSettingsView, m_SafeArea.transform);
            container.Bind<IRigSettingsView>().FromInstance(rigSettingView);
            rigSettingView.GetComponent<RectTransform>().StretchToParent();

            container.Bind<IPositionView>().FromInstance(rigSettingView.GetComponentInChildren<PositionView>(true));
            container.Bind<IRotationLockView>().FromInstance(rigSettingView.GetComponentInChildren<RotationLockView>(true));
            container.Bind<IDampingView>().FromInstance(rigSettingView.GetComponentInChildren<DampingView>(true));

            container.BindInterfacesAndSelfTo<DampingMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<PositionMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<RotationLockMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<RigSettingsMediator>().AsSingle();
            container.DeclareSignal<RigSettingsViewSignals.Close>();

            // Joystick Settings View
            var joystickSettingsView = GameObject.Instantiate(m_JoystickSettingsView, m_SafeArea.transform);
            container.Bind<IJoystickSettingsView>().FromInstance(joystickSettingsView);
            joystickSettingsView.GetComponent<RectTransform>().StretchToParent();

            container.Bind<IJoystickGeneralTabView>().FromInstance(joystickSettingsView.GetComponentInChildren<JoystickGeneralTabView>(true));

            container.BindInterfacesAndSelfTo<JoystickGeneralTabMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<JoystickSettingsMediator>().AsSingle();
            container.DeclareSignal<JoystickSettingsViewSignals.Close>();

            // Gamepad Settings View
            var gamepadSettingsView = GameObject.Instantiate(m_GamepadSettingsView, m_SafeArea.transform);
            container.Bind<IGamepadSettingsView>().FromInstance(gamepadSettingsView);
            gamepadSettingsView.GetComponent<RectTransform>().StretchToParent();

            container.Bind<IGamepadConfigurationTabView>().FromInstance(gamepadSettingsView.GetComponentInChildren<GamepadConfigurationTabView>(true));
            container.Bind<IGamepadPositionTabView>().FromInstance(gamepadSettingsView.GetComponentInChildren<GamepadPositionTabView>(true));
            container.Bind<IGamepadRotationTabView>().FromInstance(gamepadSettingsView.GetComponentInChildren<GamepadRotationTabView>(true));
            container.Bind<IGamepadSlidersTabView>().FromInstance(gamepadSettingsView.GetComponentInChildren<GamepadSlidersTabView>(true));

            container.BindInterfacesAndSelfTo<GamepadConfigurationTabMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<GamepadPositionTabMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<GamepadRotationTabMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<GamepadSlidersTabMediator>().AsSingle();
            container.BindInterfacesAndSelfTo<GamepadSettingsMediator>().AsSingle();
            container.DeclareSignal<GamepadSettingsViewSignals.Close>();

            // Settings View
            var settingsView = GameObject.Instantiate(m_SettingsView, m_SafeArea.transform);
            container.Bind<ISettingsView>().FromInstance(settingsView);

            var settingsConnectionView = settingsView.GetComponentInChildren<ConnectionView>();
            Assert.IsNotNull(settingsConnectionView);
            InstallConnectionView(container, settingsConnectionView);

            container.BindInterfacesAndSelfTo<SettingsMediator>().AsSingle();
            container.DeclareSignal<SettingsViewSignals.Close>();
            container.FixedBindSignal<SettingsViewSignals.Close>()
                .ToMethod<IMainView>(x => x.Show)
                .FromResolve();

            // Take system UI, part of MainView.
            // TODO bind interface only whenever possible.
            var takeGalleryView = GameObject.Instantiate(m_TakeLibraryView, m_SafeArea.transform);
            takeGalleryView.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesAndSelfTo<TakeLibraryView>().FromInstance(takeGalleryView);

            var takeDeletedView = GameObject.Instantiate(m_TakeDeletedView, m_SafeArea.transform);
            container.Bind<TakeDeleteView>().FromInstance(takeDeletedView);

            var takeMetadataView = GameObject.Instantiate(m_TakeMetadataView, m_SafeArea.transform);
            takeMetadataView.GetComponent<RectTransform>().StretchToParent();
            container.Bind<TakeMetadataView>().FromInstance(takeMetadataView);

            var takeRenameView = GameObject.Instantiate(m_TakeRenameView, m_SafeArea.transform);
            takeRenameView.GetComponent<RectTransform>().StretchToParent();
            container.Bind<TakeRenameView>().FromInstance(takeRenameView);

            var takeIterationView = GameObject.Instantiate(m_TakeIterationView, m_SafeArea.transform);
            takeIterationView.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesAndSelfTo<TakeIterationView>().FromInstance(takeIterationView);

            var takeSelectionView = GameObject.Instantiate(m_TakeSelectionView, m_SafeArea.transform);
            takeSelectionView.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesAndSelfTo<TakeSelectionViewFullScreen>().FromInstance(takeSelectionView);

            // Help Tooltip
            var helpTooltip = GameObject.Instantiate(m_HelpTooltip, canvasTransform).GetComponent<HelpTooltip>();
            helpTooltip.GetComponent<RectTransform>().StretchToParent();
            container.Bind<HelpTooltip>().FromInstance(helpTooltip);

            // Countdown View
            var countdownView = GameObject.Instantiate(m_CountDownView, canvasComponent.canvas.transform);
            container.BindInterfacesTo<CountdownView>().FromInstance(countdownView);
            countdownView.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesTo<CountdownMediator>().AsSingle();

            //Foreground layer of the app

            // Gamepad Layout View
            var gamepadLayoutViewInstance = GameObject.Instantiate(m_GamepadLayoutView, m_SafeArea.transform);
            gamepadLayoutViewInstance.GetComponent<RectTransform>().StretchToParent();
            container.BindInterfacesTo<GamepadLayoutView>().FromInstance(gamepadLayoutViewInstance);
            container.BindInterfacesTo<GamepadLayoutViewMediator>().AsSingle();
            gamepadLayoutViewInstance.Hide();

            // Session View
            var sessionView = GameObject.Instantiate(m_SessionView, canvasTransform);

            container.BindInterfacesTo<SessionView>().FromInstance(sessionView);

            InstallConnectionScreen(container, canvasComponent);

            var notificationView = GameObject.Instantiate(m_NotificationView, canvasComponent.canvas.transform);
            container.Bind<INotificationView>().FromInstance(notificationView);
            notificationView.GetComponent<RectTransform>().StretchToParent();

            // Permissions View
            var permissionsView = GameObject.Instantiate(m_PermissionsView, canvasComponent.canvas.transform);

            container.BindInterfacesTo<PermissionsView>().FromInstance(permissionsView);
            container.BindInterfacesTo<PermissionsViewMediator>().AsSingle();

            // Wizard Views
            InstallOnboardingWizard(container, canvasComponent);

            container.DeclareSignal<WizardSignals.Open>();
            container.DeclareSignal<WizardSignals.Skip>();
            container.DeclareSignal<WizardSignals.Next>();
        }

        ConnectionView InstantiateAndInstallConnectionView(DiContainer container, CanvasComponents canvasComponent)
        {
            var cavasTransform = canvasComponent.canvas.transform;
            var connectionView = GameObject.Instantiate(m_ConnectionView, cavasTransform);

            InstallConnectionView(container, connectionView);

            return connectionView;
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
            var connectionScreenView = GameObject.Instantiate(m_ConnectionScreenView, canvasComponents.canvas.transform);

            connectionScreenView.GetComponent<RectTransform>().StretchToParent();

            var dialogDone = connectionScreenView.DialogDone;
            var connectionViewRectTransform = connectionViewInstance.GetComponent<RectTransform>();

            connectionViewRectTransform.SetParent(dialogDone.Contents, false);
            connectionViewRectTransform.StretchToParent();

            container.Bind<IConnectionScreenView>().FromInstance(connectionScreenView);
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
