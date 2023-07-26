using System;
using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class ConnectionModelController : IInitializable, IDisposable
    {
        [Inject]
        ConnectionModel m_Connection;
        [Inject]
        SignalBus m_SignalBus;
        string m_RequestedServerName;

        public void Initialize()
        {
            m_SignalBus.Subscribe<SetConnectionModeSignal>(OnSetConnectionMode);
            m_SignalBus.Subscribe<RemoteConnectionFailedSignal>(OnConnectionFailed);
            m_SignalBus.Subscribe<ServerDiscoveryUpdatedSignal>(OnServerDiscoveryUpdated);
            m_SignalBus.Subscribe<RequestConnectSignal>(OnRequestConnect);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisconnected);
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnectionSuccess);
            m_SignalBus.Subscribe<SelectServerSignal>(OnSelectServer);
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<SetConnectionModeSignal>(OnSetConnectionMode);
            m_SignalBus.Unsubscribe<RemoteConnectionFailedSignal>(OnConnectionFailed);
            m_SignalBus.Unsubscribe<ServerDiscoveryUpdatedSignal>(OnServerDiscoveryUpdated);
            m_SignalBus.Unsubscribe<RequestConnectSignal>(OnRequestConnect);
            m_SignalBus.Unsubscribe<RemoteDisconnectedSignal>(OnDisconnected);
            m_SignalBus.Unsubscribe<RemoteConnectedSignal>(OnConnectionSuccess);
            m_SignalBus.Unsubscribe<SelectServerSignal>(OnSelectServer);
        }

        void OnSetConnectionMode(SetConnectionModeSignal signal)
        {
            m_Connection.Mode = signal.value;
        }

        void OnSelectServer(SelectServerSignal signal)
        {
            m_Connection.SelectedServer = signal.value;
        }

        void OnConnectionFailed()
        {
            m_Connection.State = ConnectionState.Disconnected;
            m_Connection.ConnectedServerName = string.Empty;
        }

        void OnServerDiscoveryUpdated(ServerDiscoveryUpdatedSignal signal)
        {
            var results = signal.value;

            m_Connection.DiscoveryResults = results;

            if (results == null)
            {
                m_Connection.SelectedServer = -1;
            }
            else
            {
                m_Connection.SelectedServer = Mathf.Clamp(m_Connection.SelectedServer, 0, results.Length - 1);
            }
        }

        void OnRequestConnect(RequestConnectSignal signal)
        {
            m_Connection.Ip = signal.Ip;
            m_Connection.Port = signal.Port;
            m_Connection.State = ConnectionState.TryingToConnect;
            m_Connection.InterruptedByFocusChange = false;

            m_RequestedServerName = string.Empty;

            if (m_Connection.Mode == ConnectionMode.Scan &&
                m_Connection.DiscoveryResults != null &&
                m_Connection.SelectedServer >= 0 &&
                m_Connection.SelectedServer < m_Connection.DiscoveryResults.Length)
            {
                m_RequestedServerName = m_Connection.DiscoveryResults[m_Connection.SelectedServer].ServerInfo.InstanceName;
            }
        }

        void OnDisconnected(RemoteDisconnectedSignal signal)
        {
            m_Connection.State = ConnectionState.Disconnected;
            m_Connection.ConnectedServerName = string.Empty;
        }

        void OnConnectionSuccess(RemoteConnectedSignal signal)
        {
            m_Connection.State = ConnectionState.Connected;
            m_Connection.ConnectedServerName = m_RequestedServerName;
            m_Connection.InterruptedByFocusChange = false;
            m_Connection.Save();
        }
    }
}
