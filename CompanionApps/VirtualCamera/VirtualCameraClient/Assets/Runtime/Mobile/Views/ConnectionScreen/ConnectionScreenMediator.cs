using System;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ConnectionScreenMediator : IInitializable, IDisposable
    {
        [Inject]
        ConnectionModel m_Connection;
        [Inject]
        IConnectionScreenView m_ConnectionScreenView;
        [Inject]
        IMainView m_MainView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_ConnectionScreenView.OnShow += OnShow;
            m_ConnectionScreenView.DialogDone.DoneClicked += OnDoneClicked;

            m_ConnectionScreenView.SystemPermissionsClicked += OnSystemPermissionsClicked;
            m_ConnectionScreenView.DocumentationClicked += OnDocumentationClicked;
            m_ConnectionScreenView.SupportClicked += OnSupportClicked;
            m_ConnectionScreenView.RestartWizardClicked += OnRestartWizardClicked;

            m_SignalBus.Subscribe<RemoteConnectedSignal>(UpdateLayout);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(UpdateLayout);
        }

        public void Dispose()
        {
            m_ConnectionScreenView.SystemPermissionsClicked -= OnSystemPermissionsClicked;
            m_ConnectionScreenView.DocumentationClicked -= OnDocumentationClicked;
            m_ConnectionScreenView.SupportClicked -= OnSupportClicked;
            m_ConnectionScreenView.RestartWizardClicked -= OnRestartWizardClicked;

            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(UpdateLayout);
        }

        void OnShow(bool value)
        {
            if (value)
            {
                UpdateLayout();
            }
        }

        void UpdateLayout()
        {
            m_ConnectionScreenView.DialogDone.ShowFooter(m_Connection.State == ConnectionState.Connected);
        }

        void OnDoneClicked()
        {
            m_SignalBus.Fire(new ConnectionScreenViewSignals.Close());
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
