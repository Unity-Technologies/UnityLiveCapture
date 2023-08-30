using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class LaunchScreenViewMediator : IInitializable, IDisposable
    {
        [Inject]
        ILaunchScreenView m_LaunchScreenView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_LaunchScreenView.SystemPermissionsClicked += OnSystemPermissionsClicked;
            m_LaunchScreenView.DocumentationClicked += OnDocumentationClicked;
            m_LaunchScreenView.SupportClicked += OnSupportClicked;
            m_LaunchScreenView.RestartWizardClicked += OnRestartWizardClicked;
        }

        public void Dispose()
        {
            m_LaunchScreenView.SystemPermissionsClicked -= OnSystemPermissionsClicked;
            m_LaunchScreenView.DocumentationClicked -= OnDocumentationClicked;
            m_LaunchScreenView.SupportClicked -= OnSupportClicked;
            m_LaunchScreenView.RestartWizardClicked -= OnRestartWizardClicked;
        }

        void OnSystemPermissionsClicked()
        {
            m_SignalBus.Fire(new PermissionsSignals.OpenSystemSettings());
        }

        void OnDocumentationClicked()
        {
            m_SignalBus.Fire(new HelpSignals.OpenDocumentation());
        }

        void OnSupportClicked()
        {
            m_SignalBus.Fire(new HelpSignals.OpenSupport());
        }

        void OnRestartWizardClicked()
        {
            m_SignalBus.Fire(new WizardSignals.Open());
        }
    }
}
