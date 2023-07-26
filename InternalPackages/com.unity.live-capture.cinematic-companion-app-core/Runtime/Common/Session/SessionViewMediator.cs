using System;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class SessionViewMediator : IInitializable, IDisposable
    {
        [Inject]
        ISessionView m_SessionView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_SessionView.DocumentationClicked += OnDocumentationClicked;
        }

        public void Dispose()
        {
            m_SessionView.DocumentationClicked -= OnDocumentationClicked;
        }

        void OnDocumentationClicked() => m_SignalBus.Fire(new SessionSignals.OpenDocumentation());
    }
}
