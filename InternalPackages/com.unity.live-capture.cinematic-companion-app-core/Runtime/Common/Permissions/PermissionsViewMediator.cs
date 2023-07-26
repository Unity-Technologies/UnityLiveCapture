using System;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class PermissionsViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IPermissionsView m_PermissionsView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_PermissionsView.OpenSettingsClicked += OnOpenSettingsClicked;
            m_PermissionsView.CancelClicked += OnCancelClicked;
        }

        public void Dispose()
        {
            m_PermissionsView.OpenSettingsClicked -= OnOpenSettingsClicked;
            m_PermissionsView.OpenSettingsClicked -= OnCancelClicked;
        }

        void OnOpenSettingsClicked()
        {
            m_SignalBus.Fire(new PermissionsSignals.OpenSystemSettings());
        }

        void OnCancelClicked()
        {
            m_PermissionsView.Hide();
        }
    }
}
