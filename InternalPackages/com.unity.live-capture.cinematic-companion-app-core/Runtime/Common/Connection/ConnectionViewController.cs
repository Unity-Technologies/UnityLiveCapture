using System;
using System.Collections.Generic;
using Zenject;

namespace Unity.CompanionAppCommon
{
    public class ConnectionViewController : IInitializable, IDisposable
    {
        [Inject]
        List<IConnectionView> m_ConnectionViews;
        [Inject]
        ConnectionModel m_Connection;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_SignalBus.Subscribe<RemoteConnectedSignal>(UpdateLayout);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(UpdateLayout);
            m_SignalBus.Subscribe<RequestConnectSignal>(UpdateLayout);
            m_SignalBus.Subscribe<RemoteConnectionFailedSignal>(UpdateLayout);
            m_SignalBus.Subscribe<SetConnectionModeSignal>(UpdateLayout);
            m_SignalBus.Subscribe<SelectServerSignal>(UpdateLayout);
            m_SignalBus.Subscribe<ServerDiscoveryUpdatedSignal>(UpdateLayout);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<RequestConnectSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<RemoteConnectionFailedSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<SetConnectionModeSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<SelectServerSignal>(UpdateLayout);
            m_SignalBus.Unsubscribe<ServerDiscoveryUpdatedSignal>(UpdateLayout);
        }

        public void UpdateIpAndPort()
        {
            foreach (var view in m_ConnectionViews)
            {
                view.Ip = m_Connection.Ip;
                view.Port = m_Connection.Port;
            }
        }

        public void UpdateLayout()
        {
            foreach (var view in m_ConnectionViews)
            {
                view.UpdateLayout(m_Connection);
            }
        }
    }
}
