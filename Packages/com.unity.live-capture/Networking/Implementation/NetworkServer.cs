using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A server capable of connecting to many <see cref="NetworkClient"/> instances.
    /// </summary>
    class NetworkServer : NetworkBase
    {
        class AcceptState
        {
            public Socket Listener;
            public NetworkSocket Udp;
        }

        // The size of the tcp backlog, which limits the number of connections which can be in queue
        // to complete their handshake. Incoming connections will not succeed if the queue is full.
        // 10 seems to be a common default value, and in our case we are not expecting to accept many
        // connections at once.
        const int k_MaxPendingConnections = 20;

        readonly List<Socket> m_Listeners = new List<Socket>();
        readonly List<NetworkSocket> m_UdpSockets = new List<NetworkSocket>();
        readonly List<IPEndPoint> m_EndPoints = new List<IPEndPoint>();
        int m_Port;

        /// <summary>
        /// The port the server is running on.
        /// </summary>
        /// <remarks>
        /// Returns -1 if the server is not running.
        /// </remarks>
        public int Port => IsRunning ? m_Port : -1;

        /// <summary>
        /// The local end points on the server that remotes connect to.
        /// </summary>
        /// <remarks>
        /// The list will be empty if the server is not running.
        /// </remarks>
        public IReadOnlyList<IPEndPoint> EndPoints => m_EndPoints;

        /// <summary>
        /// Starts up the server.
        /// </summary>
        /// <remarks>
        /// If the server is already running but a new port is provided, the server will be
        /// restarted on the new port.
        /// </remarks>
        /// <param name="port">The port to listen for incoming TCP/UDP connections on.</param>
        /// <returns>True if the server started successfully.</returns>
        public bool StartServer(int port)
        {
            if (!NetworkUtilities.IsPortValid(port, out var portMessage))
            {
                Debug.LogError($"Unable to start server: Port {port} is not valid. {portMessage}");
                return false;
            }
            if (!string.IsNullOrEmpty(portMessage))
            {
                Debug.Log($"Server: {portMessage}");
            }

            if (IsRunning)
            {
                if (m_Port == port)
                    return true;

                Stop();
            }

            m_Port = port;
            m_EndPoints.Clear();

            foreach (var ip in NetworkUtilities.GetIPAddresses(true))
            {
                var state = new AcceptState();
                var localEndPoint = new IPEndPoint(ip, port);

                if (!CreateUdpSocket(state, localEndPoint) || !CreateTcpSocket(state, localEndPoint))
                {
                    Stop();
                    return false;
                }

                m_EndPoints.Add(localEndPoint);
            }

            m_IsRunning = true;
            RaiseStartedEvent();
            return true;
        }

        /// <inheritdoc/>
        public override void Stop(bool graceful = true)
        {
            m_EndPoints.Clear();

            // stop accepting new connections before closing existing connections
            foreach (var socket in m_Listeners)
                NetworkUtilities.DisposeSocket(socket);
            m_Listeners.Clear();

            base.Stop(graceful);

            // these must be cleaned up after any connections using them have been disposed
            foreach (var socket in m_UdpSockets)
                socket.Dispose();
            m_UdpSockets.Clear();
        }

        /// <summary>
        /// Closes the connection to a given remote.
        /// </summary>
        /// <param name="remote">The remote to disconnect from.</param>
        /// <param name="graceful">Wait for the remote to acknowledge the disconnection.
        /// This may block for many seconds in the worst case.</param>
        public void Disconnect(Remote remote, bool graceful = true)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            DisconnectInternal(remote, graceful);
        }

        bool CreateUdpSocket(AcceptState state, IPEndPoint localEndPoint)
        {
            // One UDP socket is shared for all clients communicating via this interface. This considerably
            // simplifies the protocol/firewall issues, though if we are receiving or sending very large
            // amounts of data it could be a bottleneck. We could consider a combination of a basic protocol
            // to hand off communication to other ports and multicasting if we need to scale more.
            try
            {
                var socket = NetworkUtilities.CreateSocket(ProtocolType.Udp);
                var udp = new NetworkSocket(this, socket, true);
                m_UdpSockets.Add(udp);

                state.Udp = udp;

                socket.Bind(localEndPoint);
            }
            catch (Exception e)
            {
                Debug.LogError($"Server: Failed to create UDP socket on {localEndPoint}. {e}");
                return false;
            }

            return true;
        }

        bool CreateTcpSocket(AcceptState state, IPEndPoint localEndPoint)
        {
            try
            {
                var socket = NetworkUtilities.CreateSocket(ProtocolType.Tcp);
                m_Listeners.Add(socket);

                state.Listener = socket;

                socket.Bind(localEndPoint);
                socket.Listen(k_MaxPendingConnections);
                socket.BeginAccept(OnAccept, state);
            }
            catch (Exception e)
            {
                Debug.LogError($"Server: Failed to start TCP listener on {localEndPoint}. {e}");
                return false;
            }

            return true;
        }

        void OnAccept(IAsyncResult result)
        {
            var state = result.AsyncState as AcceptState;

            try
            {
                var socket = state.Listener.EndAccept(result);

                var tcp = default(NetworkSocket);
                tcp = new NetworkSocket(this, socket, false, (remote) =>
                {
                    new Connection(this, tcp, state.Udp, remote);
                });
                DoHandshake(tcp, state.Udp);
            }
            catch (ObjectDisposedException)
            {
                // This callback is invoked while the listener socket is closing.
                // The call to EndAccept will throw this exception when that occurs.
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"Server: Failed to accept connection. {e}");
            }

            state.Listener.BeginAccept(OnAccept, state);
        }
    }
}
