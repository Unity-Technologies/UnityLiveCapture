using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// The base class for the discovery client and server implementations.
    /// </summary>
    abstract class DiscoveryBase
    {
        /// <summary>
        /// The enum used to determine what type of data was received.
        /// </summary>
        protected enum PacketType : byte
        {
            /// <summary>
            /// The packet holds a <see cref="RequestData"/> instance.
            /// </summary>
            Request = 10,
            /// <summary>
            /// The packet is a <see cref="ShutdownData"/> instance.
            /// </summary>
            Shutdown = 20,
            /// <summary>
            /// The packet is a <see cref="ServerData"/> instance followed by a variable number of <see cref="EndPointData"/> instances.
            /// </summary>
            Discovery = 30,
        }

        /// <summary>
        /// The metadata in a packet that precedes the data payload.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct PacketHeader
        {
            /// <summary>
            /// The type of data in the message payload.
            /// </summary>
            public PacketType Type;
        }

        /// <summary>
        /// The size of the message buffers in bytes. This limits the size of the messages used by the discovery protocol.
        /// </summary>
        const int k_BufferSize = 1024;

        static readonly ConcurrentBag<SocketAsyncEventArgs> s_SendArgsPool = new ConcurrentBag<SocketAsyncEventArgs>();

        Socket m_RecieveSocket;
        readonly List<Socket> m_SendSockets = new List<Socket>();
        EndPoint m_BroadcastEndPoint;
        volatile bool m_RecreateSendSockets;

        /// <summary>
        /// Is discovery started.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// The UDP port used for server discovery messages.
        /// </summary>
        /// <remarks>
        /// By default this will be <see cref="Constants.DefaultPort"/>. Changes will
        /// not take effect until the next time discovery is started. It is supported for
        /// multiple discovery servers or discovery clients to use the same port.
        /// </remarks>
        public int Port { get; set; } = Constants.DefaultPort;

        /// <summary>
        /// Creates the buffers and sockets used for server discovery.
        /// </summary>
        /// <remarks>
        /// If discovery is already running it will be restarted.
        /// </remarks>
        /// <returns>True if discovery started successfully; false otherwise.</returns>
        protected bool StartInternal()
        {
            if (IsRunning)
                Stop();

            if (!NetworkUtilities.IsPortValid(Port, out var portMessage))
            {
                Debug.LogError($"Unable to start {GetType().Name}: Port {Port} is not valid. {portMessage}");
                return false;
            }
            if (!string.IsNullOrEmpty(portMessage))
            {
                Debug.Log($"{GetType().Name}: {portMessage}");
            }

            // create a socket used to receive broadcasts
            if (!TryCreateSocket(new IPEndPoint(IPAddress.Any, Port), false, out m_RecieveSocket))
            {
                Debug.LogError($"{GetType().Name}: Failed to create receive socket, stopping server discovery!");
                Stop();
                return false;
            }

            var receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.SetBuffer(new byte[k_BufferSize], 0, k_BufferSize);
            receiveArgs.Completed += ReceiveComplete;
            receiveArgs.UserToken = m_RecieveSocket;
            BeginReceive(receiveArgs);

            m_BroadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);
            CreateSendSockets();

            // when using sockets, we need to be very careful to close them before trying to unload the domain
#if UNITY_EDITOR
            EditorApplication.quitting += Stop;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
#else
            Application.quitting += Stop;
#endif

            IsRunning = true;
            return true;
        }

        /// <summary>
        /// Stops discovery.
        /// </summary>
        public void Stop()
        {
#if UNITY_EDITOR
            EditorApplication.quitting -= Stop;
            AssemblyReloadEvents.beforeAssemblyReload -= Stop;
#else
            Application.quitting -= Stop;
#endif

            OnStop();

            if (m_RecieveSocket != null)
            {
                NetworkUtilities.DisposeSocket(m_RecieveSocket);
                m_RecieveSocket = null;
            }

            foreach (var socket in m_SendSockets)
                NetworkUtilities.DisposeSocket(socket);
            m_SendSockets.Clear();

            IsRunning = false;
        }

        /// <summary>
        /// Called to update the server discovery and invoke any events.
        /// </summary>
        public void Update()
        {
            if (!IsRunning)
                return;

            var now = DateTime.Now;

            // Recreate the sockets used to send messages only on the main thread
            if (m_RecreateSendSockets)
            {
                CreateSendSockets();
            }

            OnUpdate(now);
        }

        /// <summary>
        /// Creates a packet with a header that can be send on the network.
        /// </summary>
        /// <param name="type">The type of data in the packet.</param>
        /// <param name="size">The size of the data payload in the packet.</param>
        /// <param name="offset">Returns the byte index in the packet where the data should begin.</param>
        /// <returns>A new byte array with the completed header.</returns>
        protected byte[] CreatePacket(PacketType type, int size, out int offset)
        {
            var header = new PacketHeader
            {
                Type = type,
            };
            var headerSize = SizeOfCache<PacketHeader>.Size;

            var packet = new byte[headerSize + size];
            packet.WriteStruct(header);
            offset = headerSize;
            return packet;
        }

        /// <summary>
        /// Sends a server discovery packet out on the network on all sockets.
        /// </summary>
        /// <remarks>
        /// This is thread safe.
        /// </remarks>
        /// <param name="packet">The packet to send.</param>
        /// <param name="synchronous">Block the calling thread until the message is sent.</param>
        protected void Broadcast(byte[] packet, bool synchronous)
        {
            foreach (var socket in m_SendSockets)
                Broadcast(socket, packet, synchronous);
        }

        void CreateSendSockets()
        {
            // clear any existing send sockets
            foreach (var socket in m_SendSockets)
                NetworkUtilities.DisposeSocket(socket);
            m_SendSockets.Clear();

            // Create a UDP socket used to send broadcasts on each relevant network interface. The port is irrelevant
            // so we let the OS pick a free one. If a network interface is currently initializing, the socket will
            // fail to be created. Since we periodically try to create new sockets if any were not successfully
            // created, it should fail silently.
            foreach (var address in GetSendAddresses().Reverse())
            {
                if (TryCreateSocket(new IPEndPoint(address, 0), true, out var socket))
                    m_SendSockets.Add(socket);
            }

            m_RecreateSendSockets = false;
        }

        void Broadcast(Socket socket, byte[] packet, bool synchronous)
        {
            try
            {
                if (synchronous)
                {
                    socket.SendTo(packet, m_BroadcastEndPoint);
                }
                else
                {
                    if (!s_SendArgsPool.TryTake(out var args))
                    {
                        args = new SocketAsyncEventArgs();
                        args.Completed += SendComplete;
                    }

                    args.SetBuffer(packet, 0, packet.Length);
                    args.RemoteEndPoint = m_BroadcastEndPoint;

                    if (!socket.SendToAsync(args))
                        SendComplete(socket, args);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{GetType().Name}: Failed to send message: {e}");
            }
        }

        void SendComplete(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                switch (args.SocketError)
                {
                    case SocketError.Success:
                        break;

                    // This error indicates a datagram was not received and an ICMP "Port Unreachable"
                    // response was received. We don't care if a UDP packet was not received, so don't
                    // close the connection in that case.
                    case SocketError.ConnectionReset:
                        break;

                    // If we can't broadcast on this network interface, the available networks may have changed.
                    // In this case, we should try to re-initialize using the current interfaces.
                    case SocketError.AddressNotAvailable:
                    case SocketError.HostUnreachable:
                    case SocketError.NetworkUnreachable:
                    case SocketError.NetworkDown:
                        m_RecreateSendSockets = true;
                        return;

                    // When the socket is disposed, suppress the error since it is expected that
                    // the operation should not complete.
                    case SocketError.Shutdown:
                    case SocketError.Interrupted:
                    case SocketError.OperationAborted:
                    case SocketError.ConnectionAborted:
                    case SocketError.Disconnecting:
                        return;

                    default:
                        throw new SocketException((int)args.SocketError);
                }
            }
            catch (SocketException e)
            {
                Debug.LogError($"{GetType().Name}: Failed to send message: {e}");
            }
            finally
            {
                s_SendArgsPool.Add(args);
            }
        }

        void BeginReceive(SocketAsyncEventArgs args)
        {
            var socket = args.UserToken as Socket;

            // When this returns false it has completed synchronously and the receive
            // callback will not be called automatically.
            if (!socket.ReceiveAsync(args))
                ReceiveComplete(socket, args);
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
                        BeginReceive(args);
                        return;

                    // This error indicates a datagram was not received and an ICMP "Port Unreachable"
                    // response was received. We don't care if a UDP packet was not received, so don't
                    // close the connection in that case.
                    case SocketError.ConnectionReset:
                        break;

                    // When the socket is disposed, suppress the error since it is expected that
                    // the operation should not complete.
                    case SocketError.Shutdown:
                    case SocketError.Interrupted:
                    case SocketError.OperationAborted:
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionRefused:
                    case SocketError.Disconnecting:
                        return;

                    default:
                        throw new SocketException((int)args.SocketError);
                }

                if (args.BytesTransferred >= SizeOfCache<PacketHeader>.Size)
                {
                    var header = args.Buffer.ReadStruct<PacketHeader>(0, out var offset);
                    var dataSize = args.BytesTransferred - offset;

                    OnDataReceived(args.Buffer, header, dataSize, offset);
                }

                BeginReceive(args);
            }
            catch (ObjectDisposedException)
            {
                // suppress the exception thrown by this callback if the socket was disposed
            }
            catch (Exception e)
            {
                Debug.LogError($"{GetType().Name}: Failed to receive message: {e}");
            }
        }

        bool TryCreateSocket(EndPoint endPoint, bool isSender, out Socket socket)
        {
            socket = null;

            try
            {
                socket = NetworkUtilities.CreateSocket(ProtocolType.Udp);

                // Multiple discovery servers or clients can share the same address/port without issue, as they
                // only respond to relevant traffic received.
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

                // This socket broadcasts on the network
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                // bind the socket the the interface
                socket.Bind(endPoint);

                // ensure we can fit any message we want to send or receive with the socket
                socket.SendBufferSize = k_BufferSize;
                socket.ReceiveBufferSize = k_BufferSize;

                // We need to check that broadcasting is supported on this interface. This can be done by
                // trying to broadcast and checking if an error is thrown.
                if (isSender)
                {
                    socket.SendTo(new byte[0] { }, m_BroadcastEndPoint);
                }

                return true;
            }
            catch (Exception)
            {
                if (socket != null)
                {
                    NetworkUtilities.DisposeSocket(socket, 0);
                    socket = null;
                }
                return false;
            }
        }

        /// <summary>
        /// Determines the local IP addresses discovery messages are sent on.
        /// </summary>
        /// <returns>An array containing the ip addresses.</returns>
        protected abstract IPAddress[] GetSendAddresses();

        /// <summary>
        /// Called just before stopping the server discovery.
        /// </summary>
        protected virtual void OnStop()
        {
        }

        /// <summary>
        /// Called when the server discovery should update.
        /// </summary>
        /// <param name="now">The current time.</param>
        protected virtual void OnUpdate(DateTime now)
        {
        }

        /// <summary>
        /// Called when a packet is received.
        /// </summary>
        /// <remarks>
        /// This may be called from the thread pool.
        /// </remarks>
        /// <param name="packet">The entire packet buffer.</param>
        /// <param name="header">The packet header.</param>
        /// <param name="dataSize">The size in bytes of the data payload in the packet.</param>
        /// <param name="dataOffset">The index in the data buffer the data begins at.</param>
        protected virtual void OnDataReceived(byte[] packet, PacketHeader header, int dataSize, int dataOffset)
        {
        }
    }
}
