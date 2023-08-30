using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// Handles sending and receiving message for a single socket.
    /// </summary>
    /// <remarks>
    /// This class is thread safe.
    /// </remarks>
    class NetworkSocket : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PacketHeader
        {
            public static readonly int Size = Marshal.SizeOf<PacketHeader>();

            public Guid SenderID;
            public Packet.Type Type;
            public int DataLength;
        }

        class SendState
        {
            public Packet Packet;
        }

        class ReceiveState
        {
            public int MessageLength;
            public int BytesReceived;
        }

        /// <summary>
        /// The limit is (2^16 - 1) - 20 byte IP header - 8 byte UDP header.
        /// </summary>
        /// <remarks>
        /// To avoid fragmenting packets which increases risk of packet loss, we could enforce packet sizes to be less than the MTU of
        /// ethernet (1500 bytes) or wifi (2312 bytes).
        /// </remarks>
        const int k_UdpMessageSizeMax = ushort.MaxValue - 20 - 8;

        /// <summary>
        /// The time in milliseconds reliable sockets will wait before throwing an exception if a send
        /// operation has not yet finished.
        /// </summary>
        const int k_ReliableSendTimeout = 10 * 1000;

        static readonly ConcurrentBag<SocketAsyncEventArgs> s_SendArgsPool = new ConcurrentBag<SocketAsyncEventArgs>();
        static readonly BufferPool s_BufferPool = new BufferPool(k_UdpMessageSizeMax);

        readonly NetworkBase m_Network;
        readonly Socket m_Socket;
        readonly bool m_IsShared;
        readonly Action<Remote> m_OnInitialized;
        readonly bool m_IsTcp;
        readonly ChannelType m_ChannelType;
        readonly object m_ConnectionLock = new object();
        readonly Dictionary<Guid, Connection> m_RemoteIDToConnection = new Dictionary<Guid, Connection>();
        readonly Dictionary<Connection, Queue<Packet>> m_ReceivedPackets = new Dictionary<Connection, Queue<Packet>>();
        readonly SocketAsyncEventArgs m_ReceiveArgs;

        volatile bool m_Disposed;

        /// <summary>
        /// The address and port the socket is bound to.
        /// </summary>
        internal Socket Socket
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(NetworkSocket));

                return m_Socket;
            }
        }

        /// <summary>
        /// The address and port the socket is bound to.
        /// </summary>
        internal IPEndPoint LocalEndPoint
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(NetworkSocket));

                return m_Socket.LocalEndPoint as IPEndPoint;
            }
        }

        /// <summary>
        /// The remote address and port the socket is communicating with. Will be null
        /// if this socket communicates with many remotes.
        /// </summary>
        internal IPEndPoint RemoteEndPoint
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(NetworkSocket));

                try
                {
                    return m_Socket.RemoteEndPoint as IPEndPoint;
                }
                catch (SocketException)
                {
                    // this will be thrown for UDP sockets that have not had Connect called on them
                    return null;
                }
            }
        }

        /// <summary>
        /// Has the socket been closed.
        /// </summary>
        public bool IsDisposed => m_Disposed;

        /// <summary>
        /// Creates a new <see cref="NetworkSocket"/> instance.
        /// </summary>
        /// <param name="network">The networking instance that manages this socket.</param>
        /// <param name="socket">The socket to take ownership of.</param>
        /// <param name="isShared">Indicates if the socket communicate with many remotes, as
        /// opposed to being owned by a single connection.</param>
        /// <param name="onInitialized">A callback executed when an initialization message is
        /// received on this socket to establish a new connection.</param>
        internal NetworkSocket(NetworkBase network, Socket socket, bool isShared, Action<Remote> onInitialized = null)
        {
            m_Network = network ?? throw new ArgumentNullException(nameof(network));
            m_Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            m_IsShared = isShared;
            m_OnInitialized = onInitialized;

            switch (socket.ProtocolType)
            {
                case ProtocolType.Tcp:
                {
                    m_IsTcp = true;
                    m_ChannelType = ChannelType.ReliableOrdered;

                    // Disable Nagle's Algorithm for tcp sockets. This helps to reduce latency when
                    // fewer, smaller message are being sent.
                    m_Socket.NoDelay = true;

                    // If a connection is idle for a long time, it may be closed by routers/firewalls.
                    // This option ensures that the connection keeps active.
                    m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    // By default tcp sockets will persist after being closed in order to ensure all
                    // data has been send and received successfully, but this will block the port for a while.
                    // We need to disable this behaviour so the socket closes immediately.
                    m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                    m_Socket.LingerState = new LingerOption(true, 0);

                    // the default timeout is infinite, which is not ideal
                    m_Socket.SendTimeout = k_ReliableSendTimeout;
                    break;
                }
                case ProtocolType.Udp:
                {
                    m_IsTcp = false;
                    m_ChannelType = ChannelType.UnreliableUnordered;

                    // On Mac we need to ensure the buffers are large enough to contain the
                    // largest size of message we want to be able to send to prevent errors.
                    m_Socket.ReceiveBufferSize = k_UdpMessageSizeMax;
                    m_Socket.SendBufferSize = k_UdpMessageSizeMax;

                    // By default we receive "Port Unreachable" ICMP messages for packets that fail to reach
                    // their destination, causing a ConnectionReset socket exception to be thrown. We don't want
                    // that exception to be thrown ideally, so we try to disable them on implementations for
                    // which it is supported.
                    // https://docs.microsoft.com/en-us/windows/win32/winsock/winsock-ioctls?redirectedfrom=MSDN
                    // SIO_UDP_CONNRESET = 0x9800000C
                    const int k_IOUdpConnectionReset = -1744830452;

                    try
                    {
                        m_Socket.IOControl(k_IOUdpConnectionReset, new byte[] { 0, 0, 0, 0 }, null);
                    }
                    catch (SocketException)
                    {
                    }

                    break;
                }
                default:
                    throw new ArgumentException($"Socket uses {socket.ProtocolType} protocol, but only TCP or UDP sockets are supported.", nameof(socket));
            }

            // don't allow binding this socket to a local port already in use, instead throw an error
            m_Socket.ExclusiveAddressUse = true;

            // Start receiving messages on the socket. This is safe to do, event if we are not connected
            // yet, as the receive loop will repeat until the connection is formed
            m_ReceiveArgs = new SocketAsyncEventArgs();
            m_ReceiveArgs.SetBuffer(new byte[k_UdpMessageSizeMax], 0, k_UdpMessageSizeMax);
            m_ReceiveArgs.Completed += ReceiveComplete;
            m_ReceiveArgs.UserToken = new ReceiveState();

            BeginReceive();
        }

        /// <summary>
        /// Closes the socket and clears all received packed.
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;

            NetworkUtilities.DisposeSocket(m_Socket);

            lock (m_ConnectionLock)
            {
                m_RemoteIDToConnection.Clear();

                foreach (var remoteMessages in m_ReceivedPackets)
                {
                    var queue = remoteMessages.Value;

                    while (queue.Count > 0)
                        queue.Dequeue().Message.Dispose();
                }

                m_ReceivedPackets.Clear();
            }
        }

        /// <summary>
        /// Enable receiving messages for a given connection on this socket.
        /// </summary>
        /// <param name="connection">The connection to receive messages for.</param>
        internal void RegisterConnection(Connection connection)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(NetworkSocket));

            lock (m_ConnectionLock)
            {
                m_RemoteIDToConnection[connection.Remote.ID] = connection;
                m_ReceivedPackets.Add(connection, new Queue<Packet>());
            }
        }

        /// <summary>
        /// Stops this socket from receiving messages from a connection.
        /// </summary>
        /// <param name="connection">The connection to stop receiving messages from.</param>
        internal void DeregisterConnection(Connection connection)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(NetworkSocket));

            lock (m_ConnectionLock)
            {
                // When a new connection is formed to a remote before the previous
                // connection is deregistered, we need to ensure not to remove the
                // new connection
                if (m_RemoteIDToConnection[connection.Remote.ID] == connection)
                    m_RemoteIDToConnection.Remove(connection.Remote.ID);

                if (m_ReceivedPackets.TryGetValue(connection, out var queue))
                {
                    while (queue.Count > 0)
                        queue.Dequeue().Message.Dispose();

                    m_ReceivedPackets.Remove(connection);
                }
            }
        }

        /// <summary>
        /// Sends a packet on this socket.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="synchronous">If true the caller is blocked until the message is received by the remote.</param>
        internal void Send(Packet packet, bool synchronous)
        {
            try
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(NetworkSocket));

                var message = packet.Message;
                var data = message.Data;

                if (!m_IsTcp)
                {
                    var maxLength = k_UdpMessageSizeMax - PacketHeader.Size;

                    if (data.Length > maxLength)
                        throw new ArgumentException($"Message is {data.Length} bytes long but messages using {nameof(ChannelType.UnreliableUnordered)} are limited to {maxLength} bytes.");
                }

                var count = PacketHeader.Size + (int)data.Length;
                var buffer = s_BufferPool.Get(count);

                var header = new PacketHeader
                {
                    SenderID = m_Network.ID,
                    Type = packet.PacketType,
                    DataLength = (int)data.Length,
                };

                buffer.WriteStruct(ref header);
                data.Seek(0, SeekOrigin.Begin);
                data.Read(buffer, PacketHeader.Size, header.DataLength);

                var remote = message.Remote;
                var endPoint = default(EndPoint);
                switch (m_ChannelType)
                {
                    case ChannelType.ReliableOrdered:
                        endPoint = remote.TcpEndPoint;
                        break;
                    case ChannelType.UnreliableUnordered:
                        endPoint = remote.UdpEndPoint;
                        break;
                }

                if (synchronous)
                {
                    try
                    {
                        m_Socket.SendTo(buffer, 0, count, SocketFlags.None, endPoint);
                    }
                    catch (SocketException e)
                    {
                        HandleSendError(e.SocketErrorCode);
                    }
                    finally
                    {
                        s_BufferPool.Release(buffer);

                        packet.Message.Dispose();
                    }
                }
                else
                {
                    if (!s_SendArgsPool.TryTake(out var args))
                    {
                        args = new SocketAsyncEventArgs();
                        args.Completed += SendComplete;
                        args.UserToken = new SendState();
                    }

                    var state = args.UserToken as SendState;
                    state.Packet = packet;

                    args.SetBuffer(buffer, 0, count);
                    args.RemoteEndPoint = endPoint;

                    // SendTo is not valid for connected sockets on all implementations.
                    // See error code WSAEISCONN here:
                    // https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
                    var isAsync = false;

                    if (m_IsShared)
                        isAsync = m_Socket.SendToAsync(args);
                    else
                        isAsync = m_Socket.SendAsync(args);

                    if (!isAsync)
                        SendComplete(m_Socket, args);
                }
            }
            catch
            {
                packet.Message.Dispose();
                throw;
            }
        }

        void SendComplete(object sender, SocketAsyncEventArgs args)
        {
            var state = args.UserToken as SendState;

            try
            {
                HandleSendError(args.SocketError);
            }
            finally
            {
                s_BufferPool.Release(args.Buffer);
                s_SendArgsPool.Add(args);

                state.Packet.Message.Dispose();
            }
        }

        void HandleSendError(SocketError socketError)
        {
            var logError = false;

            switch (socketError)
            {
                case SocketError.Success:
                    break;

                // When the socket is disposed, suppress the error since it is expected that
                // the operation should not complete.
                case SocketError.Shutdown:
                case SocketError.Interrupted:
                case SocketError.OperationAborted:
                case SocketError.ConnectionAborted:
                case SocketError.Disconnecting:
                    break;

                // For TCP sockets this indicates the connection is no longer valid. For UDP
                // sockets this error indicates a datagram was not received and an ICMP
                // "Port Unreachable" response was received. We don't care if a UDP packet
                // was not received, so don't close the connection in that case.
                case SocketError.ConnectionReset:
                    if (m_IsTcp)
                    {
                        OnSocketError();
                        // fail silently since this is fairly common
                    }
                    break;

                // The message size error should only apply to UDP sockets. On Mac however,
                // this error is reported by TCP sockets sending large messages even though
                // the messages are correctly received.
                case SocketError.MessageSize:
                    if (!m_IsTcp)
                    {
                        OnSocketError();
                        logError = true;
                    }
                    break;

                default:
                    OnSocketError();
                    logError = true;
                    break;
            }

            if (logError)
                Debug.LogError($"Failed to send {m_Socket.ProtocolType} message with socket error: {socketError} ({(int)socketError})");
        }

        /// <summary>
        /// Transfers all packets from the reception queue for a given connection into the provided queue.
        /// </summary>
        /// <param name="connection">The connection to get the received messages for.</param>
        /// <param name="packets">The queue to append received packets to.</param>
        internal void Receive(Connection connection, Queue<Packet> packets)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(NetworkSocket));

            lock (m_ConnectionLock)
            {
                if (!m_ReceivedPackets.TryGetValue(connection, out var queue))
                    throw new ArgumentException($"Connection to remote {connection.Remote} is not registered.", nameof(connection));

                while (queue.Count > 0)
                    packets.Enqueue(queue.Dequeue());
            }
        }

        void BeginReceive()
        {
            var args = m_ReceiveArgs;

            // clear the state to prepare for receiving a new message
            var state = args.UserToken as ReceiveState;
            state.BytesReceived = 0;
            state.MessageLength = -1;

            // Ensure we don't read past the header when using a stream protocol until we
            // know how much data to expect. This way we don't read into any following messages.
            if (m_IsTcp)
                args.SetBuffer(args.Buffer, 0, PacketHeader.Size);

            ContinueReceive(args);
        }

        void ContinueReceive(SocketAsyncEventArgs args)
        {
            // When this returns false it has completed synchronously and the receive
            // callback will not be called automatically.
            if (!m_Socket.ReceiveAsync(args))
                ReceiveComplete(m_Socket, args);
        }

        void ReceiveComplete(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                switch (args.SocketError)
                {
                    case SocketError.Success:
                        break;

                    // if we are not connected yet, keep waiting until we are connected
                    case SocketError.NotConnected:
                    case SocketError.WouldBlock:
                        BeginReceive();
                        return;

                    // When the socket is disposed, suppress the error since it is expected that
                    // the operation should not complete.
                    case SocketError.Shutdown:
                    case SocketError.Interrupted:
                    case SocketError.OperationAborted:
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionRefused:
                    case SocketError.Disconnecting:
                        return;

                    // For TCP sockets this indicates the connection is no longer valid. For UDP
                    // sockets this error indicates a datagram was not received and an ICMP
                    // "Port Unreachable" response was received. We don't care if a UDP packet
                    // was not received, so don't close the connection in that case.
                    case SocketError.ConnectionReset:
                        if (m_IsTcp)
                        {
                            OnSocketError();
                            return;
                        }
                        break;

                    default:
                        throw new SocketException((int)args.SocketError);
                }

                var state = args.UserToken as ReceiveState;

                if (m_IsTcp)
                {
                    // We must not assume the entire message has arrived yet, messages can be received in fragments
                    // of any size when using a stream socket type. First we need to receive the header so we know
                    // how much following data to read.
                    state.BytesReceived += args.BytesTransferred;

                    if (state.BytesReceived != state.MessageLength)
                    {
                        if (state.BytesReceived < PacketHeader.Size)
                        {
                            args.SetBuffer(args.Buffer, state.BytesReceived, PacketHeader.Size - state.BytesReceived);
                        }
                        else if (state.MessageLength < 0)
                        {
                            state.MessageLength = PacketHeader.Size + args.Buffer.ReadStruct<PacketHeader>().DataLength;

                            var buffer = args.Buffer;
                            if (buffer.Length < state.MessageLength)
                            {
                                buffer = new byte[state.MessageLength];
                                Buffer.BlockCopy(args.Buffer, 0, buffer, 0, state.BytesReceived);
                            }

                            args.SetBuffer(buffer, state.BytesReceived, state.MessageLength - state.BytesReceived);
                        }
                        else
                        {
                            args.SetBuffer(args.Buffer, state.BytesReceived, state.MessageLength - state.BytesReceived);
                        }

                        ContinueReceive(args);
                        return;
                    }
                }

                var header = args.Buffer.ReadStruct<PacketHeader>();

                if (header.Type == Packet.Type.Initialization)
                {
                    if (m_OnInitialized == null)
                        throw new Exception($"An initialization message was received but no initialization function was provided.");

                    var offset = PacketHeader.Size;
                    var versionData = args.Buffer.ReadStruct<VersionData>(offset);

                    var version = versionData.GetVersion();
                    if (version != m_Network.ProtocolVersion)
                        throw new Exception($"Cannot initialize connection, there is a protocol version mismatch (local={m_Network.ProtocolVersion} remote={version}).");

                    offset += Marshal.SizeOf<VersionData>();
                    var remoteData = args.Buffer.ReadStruct<RemoteData>(offset);

                    var remote = new Remote(remoteData.ID, remoteData.GetTcpEndPoint(), remoteData.GetUdpEndPoint());
                    m_OnInitialized(remote);
                }
                else
                {
                    lock (m_ConnectionLock)
                    {
                        if (m_RemoteIDToConnection.TryGetValue(header.SenderID, out var connection) &&
                            m_ReceivedPackets.TryGetValue(connection, out var queue))
                        {
                            var message = Message.Get(connection.Remote, m_ChannelType, header.DataLength);
                            var packet = new Packet(message, header.Type);

                            var data = packet.Message.Data;
                            data.Write(args.Buffer, PacketHeader.Size, header.DataLength);
                            data.Seek(0, SeekOrigin.Begin);

                            queue.Enqueue(packet);
                        }
                    }
                }

                BeginReceive();
            }
            catch (ObjectDisposedException)
            {
                // suppress the exception thrown by this callback if the socket was disposed
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to receive {m_Socket.ProtocolType} message: {e}");
                OnSocketError();
            }
        }

        void OnSocketError()
        {
            lock (m_ConnectionLock)
            {
                foreach (var connection in m_RemoteIDToConnection)
                    m_Network.OnSocketError(connection.Value);
            }
        }
    }
}
