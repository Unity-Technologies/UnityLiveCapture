using System;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class ServerDiscoveryController : IInitializable, IDisposable
    {
        [Inject]
        ServerDiscoverySystem m_ServerDiscoverySystem;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisonnected);
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnectionSuccess);

            m_ServerDiscoverySystem.StartServerDiscovery();
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnDisonnected);
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(OnConnectionSuccess);
        }

        void OnDisonnected(RemoteDisconnectedSignal signal)
        {
            m_ServerDiscoverySystem.StartServerDiscovery();
        }

        void OnConnectionSuccess(RemoteConnectedSignal signal)
        {
            m_ServerDiscoverySystem.StopServerDiscovery();
        }
    }
}
