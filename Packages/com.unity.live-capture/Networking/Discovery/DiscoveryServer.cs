using System;
using System.Linq;
using System.Net;

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// A class that announces the availability of a server on the network.
    /// </summary>
    class DiscoveryServer : DiscoveryBase
    {
        /// <summary>
        /// The time to wait after sending a discovery broadcast before sending another.
        /// </summary>
        static readonly TimeSpan k_SendPeriod = TimeSpan.FromSeconds(5);

        byte[] m_DiscoveryPacket;
        ServerData m_ServerConfiguration;
        DateTime m_LastSendTime;
        IPAddress[] m_SendAddresses;

        /// <summary>
        /// Starts the discovery server, periodically broadcasting the availability of a server on all network interfaces.
        /// </summary>
        /// <remarks>
        /// If the server is already started, it will be restarted.
        /// </remarks>
        /// <param name="serverConfiguration">The description of the server to announce.</param>
        /// <param name="endPoints">The end points that clients use to connect to the server.</param>
        /// <returns>True if the server has started successfully, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endPoints"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="endPoints"/> contains
        /// more than <see cref="byte.MaxValue"/> entries.</exception>
        public bool Start(ServerData serverConfiguration, IPEndPoint[] endPoints)
        {
            if (endPoints == null)
                throw new ArgumentNullException(nameof(endPoints));
            if (endPoints.Length > byte.MaxValue)
                throw new ArgumentException($"There cannot be more than {byte.MaxValue} values in the array.", nameof(endPoints));

            m_ServerConfiguration = serverConfiguration;
            m_LastSendTime = default;

            // We need to send out discovery packets on each interface that clients can
            // connect using, except the loopback interface since that only applies
            // the the local device.
            var uniqueEndPoints = endPoints
                .Distinct()
                .Where(e => !e.Address.Equals(IPAddress.Loopback))
                .ToArray();

            m_SendAddresses = uniqueEndPoints
                .Select(e => e.Address)
                .ToArray();

            // Create the server discover packet, which consists of the server information followed
            // by the end points clients can connect to.
            var size = SizeOfCache<ServerData>.Size + sizeof(byte) + (uniqueEndPoints.Length * SizeOfCache<EndPointData>.Size);
            m_DiscoveryPacket = CreatePacket(PacketType.Discovery, size, out var offset);

            offset = m_DiscoveryPacket.WriteStruct(ref serverConfiguration, offset);
            offset = m_DiscoveryPacket.WriteStruct((byte)uniqueEndPoints.Length, offset);

            foreach (var endPoint in uniqueEndPoints)
                offset = m_DiscoveryPacket.WriteStruct(new EndPointData(endPoint), offset);

            // start the networking
            if (!StartInternal())
                return false;

            // immediately broadcast the existence of this server
            BroadcastDiscovery();

            return true;
        }

        /// <inheritdoc />
        protected override IPAddress[] GetSendAddresses()
        {
            // ensure that we only create sockets for currently valid addresses
            return m_SendAddresses.Intersect(NetworkUtilities.GetIPAddresses(false)).ToArray();
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            // Send a message informing clients the server is no longer available
            // so they don't need to wait for the timeout to realize it is lost.
            var packet = CreatePacket(PacketType.Shutdown, SizeOfCache<ShutdownData>.Size, out var offset);
            packet.WriteStruct(new ShutdownData(m_ServerConfiguration.ID), offset);
            Broadcast(packet, true);
        }

        /// <inheritdoc />
        protected override void OnUpdate(DateTime now)
        {
            if (now - m_LastSendTime > k_SendPeriod)
            {
                BroadcastDiscovery();
            }
        }

        /// <inheritdoc />
        protected override void OnDataReceived(byte[] packet, PacketHeader header, int dataSize, int dataOffset)
        {
            switch (header.Type)
            {
                case PacketType.Request:
                {
                    if (dataSize != SizeOfCache<RequestData>.Size)
                        return;

                    var data = packet.ReadStruct<RequestData>(dataOffset);

                    // only consider request packets from a matching product
                    if (data.ProductName != m_ServerConfiguration.ProductName)
                        return;

                    BroadcastDiscovery();
                    break;
                }
            }
        }

        void BroadcastDiscovery()
        {
            Broadcast(m_DiscoveryPacket, false);
            m_LastSendTime = DateTime.Now;
        }
    }
}
