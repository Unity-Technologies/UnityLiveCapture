using System;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class SessionViewController : IInitializable, IDisposable, ISessionStateListener
    {
        [Inject]
        ISessionView m_SessionView;

        [Inject]
        SignalBus m_SignalBus;

        bool m_IsSessionActive;
        bool m_IsConnected;

        public void Initialize()
        {
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        public void SetSessionState(bool active)
        {
            m_IsSessionActive = active;
            UpdateView();
        }

        void OnConnected()
        {
            m_IsConnected = true;
            UpdateView();
        }

        void OnDisconnected()
        {
            m_IsConnected = false;
            UpdateView();
        }

        void UpdateView()
        {
            // We only show the session view if we are connected.
            if (m_IsConnected && !m_IsSessionActive)
            {
                m_SessionView.Show();
            }
            else
            {
                m_SessionView.Hide();
            }
        }
    }
}
