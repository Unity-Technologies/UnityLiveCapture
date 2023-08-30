using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// Represents a connection to a remote.
    /// </summary>
    /// <remarks>
    /// The connection health is tested using a heartbeat, sending and receiving UDP packets periodically from the
    /// remote to ensure it is still reachable.
    /// </remarks>
    class Connection
    {
        /// <summary>
        /// The threshold for missed heartbeat messages before the connection is assumed to be dead.
        /// </summary>
        const int k_HeartbeatDisconnectCount = 8;

        /// <summary>
        /// The duration in seconds between heartbeat messages.
        /// </summary>
        static readonly TimeSpan k_HeartbeatPeriod = TimeSpan.FromSeconds(1.0);

        readonly NetworkBase m_Network;
        readonly NetworkSocket m_Tcp;
        readonly NetworkSocket m_Udp;

        readonly CancellationTokenSource m_HeartbeatCancellationToken = new CancellationTokenSource();
        readonly Queue<Packet> m_TempReceiveQueue = new Queue<Packet>();

        DateTime m_LastHeartbeat;
        bool m_Disposed;

        /// <summary>
        /// The remote at the other end of the connection.
        /// </summary>
        public Remote Remote { get; }

        /// <summary>
        /// Creates a new <see cref="Connection"/> instance.
        /// </summary>
        /// <param name="network">The networking instance on the local side of the connection.</param>
        /// <param name="tcp">The local tcp socket connected to the remote.</param>
        /// <param name="udp">The local udp socket used to communicate with the remote.</param>
        /// <param name="remote">The remote at the other end of the connection.</param>
        public Connection(NetworkBase network, NetworkSocket tcp, NetworkSocket udp, Remote remote)
        {
            m_Network = network ?? throw new ArgumentNullException(nameof(tcp));
            m_Tcp = tcp ?? throw new ArgumentNullException(nameof(tcp));
            m_Udp = udp ?? throw new ArgumentNullException(nameof(udp));
            Remote = remote ?? throw new ArgumentNullException(nameof(remote));

            m_Tcp.RegisterConnection(this);
            m_Udp.RegisterConnection(this);

            m_Network.RegisterConnection(this);

            DoHeartbeat();
        }

        /// <summary>
        /// Disposes of this connection.
        /// </summary>
        /// <param name="status">How the connection was terminated.</param>
        public void Close(DisconnectStatus status)
        {
            if (m_Disposed)
                return;

            m_Disposed = true;

            m_HeartbeatCancellationToken.Cancel();

            m_Network.DeregisterConnection(this, status);

            // Remote the connection from sockets that are shared, and
            // dispose sockets that are exclusive to this connection.
            if (!m_Udp.IsDisposed)
            {
                m_Udp.DeregisterConnection(this);

                if (m_Udp.RemoteEndPoint != null)
                    m_Udp.Dispose();
            }
            if (!m_Tcp.IsDisposed)
            {
                m_Tcp.DeregisterConnection(this);
                m_Tcp.Dispose();
            }
        }

        /// <summary>
        /// Sends a message on this connection.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="synchronous">If true the caller is blocked until the message is received by the remote.</param>
        public void Send(Packet packet, bool synchronous)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(Connection));

            switch (packet.Message.ChannelType)
            {
                case ChannelType.ReliableOrdered:
                    m_Tcp.Send(packet, synchronous);
                    break;
                case ChannelType.UnreliableUnordered:
                    m_Udp.Send(packet, synchronous);
                    break;
                default:
                    throw new ArgumentException($"Message channel {packet.Message.ChannelType} is not supported.");
            }
        }

        /// <summary>
        /// Receives messages from the sockets and checks the connection health.
        /// </summary>
        public void Update()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(Connection));

            ReceiveInternal(m_Udp);
            ReceiveInternal(m_Tcp);

            // Check if any of the last few heartbeat messages were received. If we have not received
            // any for long enough, we can assume the connection is lost. Losing multiple packets sent
            // on a local network seconds apart is extremely unlikely on a modern network.

            // An issue was encountered on iOS only where m_LastHeartbeat was not being correctly set
            // when assigned the value DateTime.Now in the constructor, instead being set to the default
            // DateTime value. To fix the issue we initialize the value here. It could be related to the
            // fact the constructor is called from the thread pool. This only occurs for the first
            // connection however.
            if (m_LastHeartbeat == DateTime.MinValue)
                m_LastHeartbeat = DateTime.Now;

            var timeSinceLastBeat = (DateTime.Now - m_LastHeartbeat).TotalSeconds;
            var disconnectDuration = k_HeartbeatDisconnectCount * k_HeartbeatPeriod.TotalSeconds;

            if (timeSinceLastBeat > disconnectDuration)
                Close(DisconnectStatus.Timeout);
        }

        void ReceiveInternal(NetworkSocket socket)
        {
            m_TempReceiveQueue.Clear();
            socket.Receive(this, m_TempReceiveQueue);

            while (m_TempReceiveQueue.Count > 0)
            {
                var packet = m_TempReceiveQueue.Dequeue();
                var message = packet.Message;

                switch (packet.PacketType)
                {
                    case Packet.Type.Generic:
                    {
                        m_Network.ReceiveMessage(message);
                        break;
                    }
                    case Packet.Type.Heartbeat:
                    {
                        m_LastHeartbeat = DateTime.Now;
                        message.Dispose();
                        break;
                    }
                    case Packet.Type.Disconnect:
                    {
                        Close(DisconnectStatus.Graceful);
                        message.Dispose();
                        return;
                    }
                    default:
                        Debug.LogError($"A packet of type {packet.PacketType} ({(int)packet.PacketType}) was received but that type is never used!");
                        message.Dispose();
                        break;
                }
            }
        }

        async void DoHeartbeat()
        {
            while (!m_HeartbeatCancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(k_HeartbeatPeriod, m_HeartbeatCancellationToken.Token);
                }
                catch (TaskCanceledException)
                {
                }

                if (!m_HeartbeatCancellationToken.IsCancellationRequested)
                {
                    var message = Message.Get(Remote, ChannelType.UnreliableUnordered);
                    var packet = new Packet(message, Packet.Type.Heartbeat);

                    Send(packet, false);
                }
            }
        }
    }
}
