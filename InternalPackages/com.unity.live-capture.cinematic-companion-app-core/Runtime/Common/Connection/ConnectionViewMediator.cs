using System.Net;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Discovery;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class ConnectionViewMediator : IInitializable
    {
        [Inject]
        ConnectionModel m_Connection;
        [Inject]
        IConnectionView m_ConnectionView;
        [Inject]
        INotificationSystem m_Notification;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_ConnectionView.onConnectClicked += OnConnectClicked;
            m_ConnectionView.onServerSelected += OnServerSelected;
            m_ConnectionView.onManualClicked += OnManualClicked;
            m_ConnectionView.onScanClicked += OnScanClicked;
        }

        void OnScanClicked()
        {
            m_SignalBus.Fire(new SetConnectionModeSignal() { value = ConnectionMode.Scan });
        }

        void OnManualClicked()
        {
            m_SignalBus.Fire(new SetConnectionModeSignal() { value = ConnectionMode.Manual });
        }

        void OnConnectClicked()
        {
            if (m_Connection.State == ConnectionState.Connected)
            {
                m_SignalBus.Fire(new RequestDisconnectSignal());
            }
            else
            {
                var isValid = true;
                var ip = m_ConnectionView.Ip;
                var port = m_ConnectionView.Port;

                if (m_Connection.Mode == ConnectionMode.Scan)
                {
                    isValid = m_Connection.DiscoveryResults != null
                        && m_Connection.DiscoveryResults.Length > 0
                        && m_Connection.SelectedServer >= 0
                        && m_Connection.SelectedServer < m_Connection.DiscoveryResults.Length;

                    if (isValid)
                    {
                        var result = m_Connection.DiscoveryResults[m_Connection.SelectedServer];

                        var parsed = ParseDiscoveryResult(result, out ip, out port);
                        if (!parsed)
                        {
                            m_Notification.Show($"No common network interface found with server");
                        }

                        isValid = parsed;
                    }
                }

                if (isValid)
                {
                    m_SignalBus.Fire(new RequestConnectSignal()
                    {
                        Ip = ip,
                        Port = port,
                        ClientType = m_Connection.ClientType
                    });
                }
            }
        }

        void OnServerSelected(int value)
        {
            m_SignalBus.Fire(new SelectServerSignal() { value = value });
        }

        bool ParseDiscoveryResult(DiscoveryInfo result, out string ip, out int port)
        {
            var closestAddress = NetworkUtilities.FindClosestAddresses(result.EndPoints);

            if (closestAddress.remoteEndPoint == null)
            {
                ip = default;
                port = default;
                return false;
            }

            ip = closestAddress.remoteEndPoint.Address.ToString();
            port = closestAddress.remoteEndPoint.Port;
            return true;
        }
    }
}
