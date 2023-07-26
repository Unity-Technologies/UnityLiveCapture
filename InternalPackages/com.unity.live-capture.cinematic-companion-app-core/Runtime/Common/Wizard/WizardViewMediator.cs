using System;
using System.Collections.Generic;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class WizardViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IWizardView m_WizardView;

        [Inject]
        protected SignalBus m_SignalBus;

        public virtual void Initialize()
        {
            m_WizardView.SkipClicked += OnSkipClicked;
            m_WizardView.NextClicked += OnNextClicked;
        }

        public virtual void Dispose()
        {
            m_WizardView.SkipClicked -= OnSkipClicked;
            m_WizardView.NextClicked -= OnNextClicked;
        }

        void OnSkipClicked()
        {
            m_SignalBus.Fire<WizardSignals.Skip>();
        }

        void OnNextClicked()
        {
            m_SignalBus.Fire<WizardSignals.Next>();
        }
    }
}
