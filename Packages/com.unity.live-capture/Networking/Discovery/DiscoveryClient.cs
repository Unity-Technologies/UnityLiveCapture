using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// A class that listens for the availability of any servers on the network.
    /// </summary>
    class DiscoveryClient : DiscoveryBase
    {
        /// <summary>
        /// How much time must pass since a discovery server was last heard from before it is determined to be lost.
        /// </summary>
        static readonly TimeSpan k_ServerLossTime = TimeSpan.FromSeconds(16);

        struct Server
        {
            public DiscoveryInfo Discovery;
            public DateTime LastUpdate;
        }

        readonly ConcurrentQueue<DiscoveryInfo> m_UpdatedServers = new ConcurrentQueue<DiscoveryInfo>();
        readonly ConcurrentQueue<Guid> m_LostServers = new ConcurrentQueue<Guid>();
        readonly Dictionary<Guid, Server> m_Servers = new Dictionary<Guid, Server>();
        string m_ProductName;
        byte[] m_RequestPacket;
        IPAddress[] m_IgnoreAddresses;

        /// <summary>
        /// The event invoked when a server has been discovered or has changed configuration.
        /// </summary>
        /// <remarks>
        /// This is also invoked when a server has changed configuration after <see cref="ServerLost"/>
        /// is invoked with the previous configuration.
        /// </remarks>
        public Action<DiscoveryInfo> ServerFound;

        /// <summary>
        /// The event invoked when a server becomes no longer available or has changed configuration.
        /// </summary>
        /// <remarks>
        /// This will be invoked if a server notifies the client that it is shutting down, or if the server
        /// is unable to reach this client for a while. This is also invoked when a server has changed
        /// configuration before <see cref="ServerFound"/> is invoked with the new configuration.
        /// </remarks>
        public Action<DiscoveryInfo> ServerLost;

        /// <summary>
        /// Starts the discovery client.
        /// </summary>
        /// <remarks>
        /// If the client is already started, it will be restarted.
        /// </remarks>
        /// <param name="productName">The name of the server application. Only servers with a matching
        /// product name will be found by this client. The name must not exceed <see cref="Constants.StringMaxLength"/>
        /// characters in length.</param>
        /// <param name="discoverLocal">Will servers running on this device be discovered in addition to
        /// remote servers.</param>
        /// <returns>True if the client has started successfully, false otherwise.</returns>
        public bool Start(string productName, bool discoverLocal = false)
        {
            while (m_UpdatedServers.TryDequeue(out _)) { }
            while (m_LostServers.TryDequeue(out _)) { }
            m_Servers.Clear();

            m_ProductName = productName;

            m_RequestPacket = CreatePacket(PacketType.Request, SizeOfCache<RequestData>.Size, out var offset);
            m_RequestPacket.WriteStruct(new RequestData(productName), offset);

            // ignore packets from IP addresses on this device if we don't want to discover local servers
            m_IgnoreAddresses = discoverLocal ? null : NetworkUtilities.GetIPAddresses(true);

            // start the networking
            if (!StartInternal())
                return false;

            Refresh();

            return true;
        }

        /// <summary>
        /// Broadcast a request for servers to send updated discovery information.
        /// </summary>
        /// <remarks>
        /// This will reduce the time the client might need to wait before detecting new servers or updated
        /// server configurations. Avoid calling frequently, as it could reduce network performance.
        /// </remarks>
        public void Refresh()
        {
            if (!IsRunning)
                return;

            Broadcast(m_RequestPacket, false);
        }

        /// <inheritdoc />
        protected override IPAddress[] GetSendAddresses()
        {
            return NetworkUtilities.GetIPAddresses(false);
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            foreach (var server in m_Servers.Values.ToArray())
                OnServerLost(server);
        }

        /// <inheritdoc />
        protected override void OnUpdate(DateTime now)
        {
            while (m_UpdatedServers.TryDequeue(out var discovery))
            {
                var id = discovery.ServerInfo.ID;

                // when a server changes we report the previous configuration as lost
                if (m_Servers.TryGetValue(id, out var server) && !discovery.Equals(server.Discovery))
                    OnServerLost(server);

                // report the first time we see a server configuration
                if (!m_Servers.ContainsKey(id))
                    ServerFound?.Invoke(discovery);

                // record the time we last updated this server
                m_Servers[id] = new Server
                {
                    Discovery = discovery,
                    LastUpdate = now,
                };
            }

            // report servers that have not been heard from in a while as lost
            foreach (var guidServer in m_Servers)
            {
                var server = guidServer.Value;

                if (now - server.LastUpdate > k_ServerLossTime)
                    m_LostServers.Enqueue(server.Discovery.ServerInfo.ID);
            }

            while (m_LostServers.TryDequeue(out var id))
            {
                if (m_Servers.TryGetValue(id, out var server))
                {
                    OnServerLost(server);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnDataReceived(byte[] packet, PacketHeader header, int dataSize, int dataOffset)
        {
            switch (header.Type)
            {
                case PacketType.Shutdown:
                {
                    if (dataSize != SizeOfCache<ShutdownData>.Size)
                        return;

                    var data = packet.ReadStruct<ShutdownData>(dataOffset);
                    m_LostServers.Enqueue(data.ID);
                    break;
                }
                case PacketType.Discovery:
                {
                    var minSize = SizeOfCache<ServerData>.Size + sizeof(byte);

                    if (dataSize < minSize)
                        return;

                    var serverData = packet.ReadStruct<ServerData>(dataOffset, out dataOffset);

                    // only consider discovery packets from a matching product
                    if (serverData.ProductName != m_ProductName)
                        return;

                    var endPointCount = packet.ReadStruct<byte>(dataOffset, out dataOffset);

                    if (dataSize - minSize != endPointCount * SizeOfCache<EndPointData>.Size)
                        return;

                    var endPoints = new IPEndPoint[endPointCount];

                    for (var i = 0; i < endPointCount; i++)
                        endPoints[i] = packet.ReadStruct<EndPointData>(dataOffset, out dataOffset).GetEndPoint();

                    // don't consider discovery packets from addresses that should be ignored
                    if (m_IgnoreAddresses != null)
                    {
                        foreach (var ignoreAddress in m_IgnoreAddresses)
                        {
                            foreach (var endPoint in endPoints)
                            {
                                if (ignoreAddress.Equals(endPoint.Address))
                                    return;
                            }
                        }
                    }

                    var discovery = new DiscoveryInfo(serverData, endPoints);
                    m_UpdatedServers.Enqueue(discovery);
                    break;
                }
            }
        }

        void OnServerLost(Server server)
        {
            if (m_Servers.Remove(server.Discovery.ServerInfo.ID))
            {
                ServerLost?.Invoke(server.Discovery);
            }
        }
    }
}
