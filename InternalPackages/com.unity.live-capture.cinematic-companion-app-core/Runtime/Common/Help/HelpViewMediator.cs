using System;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class HelpViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IHelpView m_HelpModeView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_HelpModeView.DocumentationClicked += OnDocumentationClick;
            m_HelpModeView.SupportClicked += OnSupportClick;
        }

        public void Dispose()
        {
            m_HelpModeView.DocumentationClicked -= OnDocumentationClick;
            m_HelpModeView.SupportClicked -= OnSupportClick;
        }

        void OnDocumentationClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenDocumentation());
        }

        void OnSupportClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenSupport());
        }
    }
}
