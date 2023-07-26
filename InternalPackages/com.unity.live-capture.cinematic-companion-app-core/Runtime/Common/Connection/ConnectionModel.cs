using Unity.LiveCapture.Networking.Discovery;

namespace Unity.CompanionAppCommon
{
    enum ConnectionMode
    {
        Scan,
        Manual
    }

    enum ConnectionState
    {
        Disconnected,
        TryingToConnect,
        Connected
    }

    class ConnectionModel
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string ClientType { get; set; }
        public ConnectionMode Mode { get; set; }
        public ConnectionState State { get; set; }
        public string ConnectedServerName { get; set; }
        public DiscoveryInfo[] DiscoveryResults { get; set; }
        public int SelectedServer { get; set; }
        public bool InterruptedByFocusChange { get; set; }
    }
}
