using System;
using System.Collections.Generic;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class WizardPermissionsViewMediator : WizardViewMediator
    {
        [Inject]
        IWizardPermissionsView m_WizardView;

        public override void Initialize()
        {
            base.Initialize();
            m_WizardView.OpenPermissionsSettings += OnOpenPermissionsSettings;
        }

        public override void Dispose()
        {
            base.Dispose();
            m_WizardView.OpenPermissionsSettings -= OnOpenPermissionsSettings;
        }

        void OnOpenPermissionsSettings()
        {
            m_SignalBus.Fire<PermissionsSignals.OpenSystemSettings>();
        }
    }
}
