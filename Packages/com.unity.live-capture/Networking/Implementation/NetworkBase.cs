using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A class containing networking functionality shared by clients and servers.
    /// </summary>
    abstract class NetworkBase
    {
        /// <summary>
        /// Used by the <see cref="m_ConnectionEvents"/> queue to perform actions that modify
        /// connection state in the <see cref="Update"/> method in the same order the events were
        /// submitted.
        /// </summary>
        readonly struct ConnectionEvent
        {
            public enum Type
            {
                /// <summary>
                /// A connection was established and needs set as the current connection for
                /// the corresponding remote.
                /// </summary>
                NewConnection,
                /// <summary>
                /// The connection needs to be closed immediately.
                /// </summary>
                TerminateConnection,
            }

            /// <summary>
            /// The connection this event applies to.
            /// </summary>
            public Connection Connection { get; }

            /// <summary>
            /// The type of connection event.
            /// </summary>
            public Type EventType { get; }

            /// <summary>
            /// Creates a new <see cref="ConnectionEvent"/> instance.
            /// </summary>
            /// <param name="connection">The connection this event applies to.</param>
            /// <param name="type">The type of connection event.</param>
            public ConnectionEvent(Connection connection, Type type)
            {
                Connection = connection;
                EventType = type;
            }
        }

        /// <summary>
        /// Is the networking started.
        /// </summary>
        protected volatile bool m_IsRunning;

        readonly ConcurrentDictionary<Remote, Connection> m_RemoteToConnection = new ConcurrentDictionary<Remote, Connection>();
        readonly ConcurrentQueue<ConnectionEvent> m_ConnectionEvents = new ConcurrentQueue<ConnectionEvent>();
        readonly Dictionary<Remote, Action<Message>> m_MessageHandlers = new Dictionary<Remote, Action<Message>>();
        readonly Dictionary<Remote, Queue<Message>> m_BufferedMessages = new Dictionary<Remote, Queue<Message>>();

        /// <summary>
        /// The version of the networking protocol.
        /// </summary>
        public Version ProtocolVersion { get; } = new Version(0, 1, 1, 0);

        /// <summary>
        /// The networking channels which are supported by this network implementation.
        /// </summary>
        public ChannelType[] SupportedChannels { get; } =
        {
            ChannelType.ReliableOrdered,
            ChannelType.UnreliableUnordered,
        };

        /// <summary>
        /// The ID of this instance, used to identify this remote on the network for its entire life span.
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

        /// <summary>
        /// Is the networking started.
        /// </summary>
        public bool IsRunning => m_IsRunning;

        /// <summary>
        /// The number of connected remotes.
        /// </summary>
        public int RemoteCount => m_RemoteToConnection.Count;

        /// <summary>
        /// Gets a new list containing all the remotes that this instance can send messages
        /// to or receive from.
        /// </summary>
        public List<Remote> Remotes
        {
            get
            {
                var result = new List<Remote>(m_RemoteToConnection.Count);

                foreach (var remoteConnection in m_RemoteToConnection)
                    result.Add(remoteConnection.Key);

                return result;
            }
        }

        /// <summary>
        /// Invoked when the networking is successfully started.
        /// </summary>
        public event Action Started = delegate { };

        /// <summary>
        /// Invoked after the networking has been shut down.
        /// </summary>
        public event Action Stopped = delegate { };

        /// <summary>
        /// Invoked when a connection to a remote is established.
        /// </summary>
        public event Action<Remote> RemoteConnected = delegate { };

        /// <summary>
        /// Invoked when a remote has disconnected.
        /// </summary>
        /// <remarks>
        /// In case of a non-graceful disconnect, the networked instances will attempt to reconnect automatically.
        /// </remarks>
        public event Action<Remote, DisconnectStatus> RemoteDisconnected = delegate { };

        /// <summary>
        /// Creates a new <see cref="NetworkBase"/> instance.
        /// </summary>
        protected NetworkBase()
        {
            // when using sockets, we need to be very careful to close them before trying to unload the domain
            Application.quitting += () => Stop(false);
#if UNITY_EDITOR
            EditorApplication.quitting += () => Stop(false);
            AssemblyReloadEvents.beforeAssemblyReload += () => Stop(false);
#endif
        }

        /// <summary>
        /// Checks if a remote is connected to this network instance.
        /// </summary>
        /// <param name="remote">The remote to check if connected.</param>
        /// <returns>True if the remote is connected; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="remote"/> is null.</exception>
        public bool IsConnected(Remote remote)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            return m_RemoteToConnection.ContainsKey(remote);
        }

        /// <summary>
        /// Attempts to register a handler for the messages received from the provided remote.
        /// Only one handler can be registered for each remote at a time. The message handler for a
        /// remote is automatically deregistered when the remote disconnects, or when the networking
        /// is stopped. <see cref="Remote.All"/> is not valid here, each remote must have a handler
        /// registered explicitly.
        /// </summary>
        /// <param name="remote">The remote to receive messages from.</param>
        /// <param name="messageHandler">The handler for the received messages. It is responsible for
        /// disposing the received messages.</param>
        /// <param name="handleBufferedMessages">Will messages from the remote which have not been read yet
        /// immediately be passed to the new handler. If the messages are not handled, they are disposed of
        /// without being read.</param>
        /// <returns>True if the handler was successfully registered.</returns>
        public bool RegisterMessageHandler(Remote remote, Action<Message> messageHandler, bool handleBufferedMessages = true)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));
            if (remote == Remote.All)
                throw new ArgumentException($"{nameof(Remote)}.{nameof(Remote.All)} cannot be used, message handlers must be registered per remote.", nameof(remote));
            if (!m_RemoteToConnection.ContainsKey(remote))
                throw new ArgumentException($"Remote {remote} is currently not connected to this instance.", nameof(remote));
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            try
            {
                if (m_MessageHandlers.TryGetValue(remote, out var currentHandler))
                    return currentHandler == messageHandler;

                m_MessageHandlers.Add(remote, messageHandler);

                // if we are not handling buffered messages, they must be disposed and discarded
                if (m_BufferedMessages.TryGetValue(remote, out var messages))
                {
                    foreach (var message in messages)
                    {
                        if (handleBufferedMessages)
                        {
                            try
                            {
                                messageHandler.Invoke(message);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                        else
                        {
                            message.Dispose();
                        }
                    }

                    messages.Clear();
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to register message handler: {e}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to deregister the handler for messages received from the provided remote.
        /// <see cref="Remote.All"/> is not valid here, each remote must be deregistered explicitly.
        /// </summary>
        /// <param name="remote">The remote to stop receiving messages from.</param>
        /// <returns>True if the provided handler was successfully deregistered.</returns>
        public bool DeregisterMessageHandler(Remote remote)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));
            if (remote == Remote.All)
                throw new ArgumentException($"{nameof(Remote)}.{nameof(Remote.All)} cannot be used, message handlers must be deregistered per remote.", nameof(remote));
            if (!m_RemoteToConnection.ContainsKey(remote))
                throw new ArgumentException($"Remote {remote} is currently not connected to this instance.", nameof(remote));

            try
            {
                return !m_MessageHandlers.ContainsKey(remote) || m_MessageHandlers.Remove(remote);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deregister message handler: {e}");
                return false;
            }
        }

        /// <summary>
        /// Sends a message over the network. This is thread safe. Messages are only guaranteed
        /// to be sent in order for messages sent from the same thread.
        /// </summary>
        /// <param name="message">The message to send. The caller is not responsible for disposing the
        /// message, and should immediately remove any references to the message to ensure full transfer
        /// of ownership, as the message will be pooled and reused after it has been sent. To efficiently
        /// send a message to all remotes, specify <see cref="Remote.All"/> as the remote when creating a
        /// message.</param>
        /// <returns>True if the message was successfully sent.</returns>
        public bool SendMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                var packet = new Packet(message, Packet.Type.Generic);
                var remote = message.Remote;

                if (remote == Remote.All)
                {
                    foreach (var remoteConnection in m_RemoteToConnection)
                        remoteConnection.Value.Send(packet, false);
                }
                else
                {
                    if (!m_RemoteToConnection.TryGetValue(remote, out var connection))
                        throw new Exception($"There is currently no connection to remote {remote}.");

                    connection.Send(packet, false);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message: {e}");
                return false;
            }
        }

        /// <summary>
        /// Called to invoke the events and callbacks.
        /// </summary>
        public void Update()
        {
            if (!IsRunning)
                return;

            while (m_ConnectionEvents.TryDequeue(out var e))
            {
                switch (e.EventType)
                {
                    case ConnectionEvent.Type.NewConnection:
                    {
                        var remote = e.Connection.Remote;

                        if (m_RemoteToConnection.TryGetValue(remote, out var oldConnection))
                            oldConnection.Close(DisconnectStatus.Reconnected);

                        m_RemoteToConnection.TryAdd(remote, e.Connection);

                        try
                        {
                            RemoteConnected?.Invoke(remote);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                        break;
                    }
                    case ConnectionEvent.Type.TerminateConnection:
                    {
                        e.Connection.Close(DisconnectStatus.Error);
                        break;
                    }
                }
            }

            foreach (var remoteConnection in m_RemoteToConnection)
            {
                try
                {
                    remoteConnection.Value.Update();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to update connection: {e}");
                }
            }
        }

        /// <summary>
        /// Stop running the networking, closing any connections. All message handlers will be
        /// deregistered and buffered messages from remotes with no registered handler will be discarded.
        /// </summary>
        /// <param name="graceful">Finish send/receiving buffered messages and disconnect gracefully.
        /// This may block for many seconds in the worst case.</param>
        public virtual void Stop(bool graceful = true)
        {
            if (!m_IsRunning)
                return;

            m_IsRunning = false;

            DisconnectInternal(Remote.All, graceful);

            // ensure all the collections are reset
            while (m_ConnectionEvents.TryDequeue(out _)) { }
            m_RemoteToConnection.Clear();
            m_BufferedMessages.Clear();
            m_MessageHandlers.Clear();

            try
            {
                Stopped?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            // truncate the GUID to keep it a readable length
            return $"{{{GetType().Name}, ID: {ID.ToString("N").Substring(0, 8)}}}";
        }

        /// <summary>
        /// Closes the connection to a given remote.
        /// </summary>
        /// <param name="remote">The remote to close the connection for.</param>
        /// <param name="graceful">Wait for the remote to acknowledge the disconnection.
        /// This may block for many seconds in the worst case.</param>
        internal void DisconnectInternal(Remote remote, bool graceful)
        {
            if (remote == Remote.All)
            {
                foreach (var remoteConnection in m_RemoteToConnection)
                    Disconnect(remoteConnection.Value, graceful);
            }
            else if (m_RemoteToConnection.TryGetValue(remote, out var connection))
            {
                Disconnect(connection, graceful);
            }
        }

        void Disconnect(Connection connection, bool graceful)
        {
            try
            {
                // If disconnecting gracefully we need to send the disconnect message synchronously,
                // so we can guarantee it has been successfully received before we close the connection
                // to the remote.
                if (graceful)
                {
                    var message = Message.Get(connection.Remote, ChannelType.ReliableOrdered);
                    var packet = new Packet(message, Packet.Type.Disconnect);

                    connection.Send(packet, true);
                }

                // ensure all available messages are read before they are disposed
                connection.Update();
                connection.Close(DisconnectStatus.Graceful);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to disconnect remote: {e}");
            }
        }

        /// <summary>
        /// Executes the registered message handler for a message.
        /// </summary>
        /// <param name="message">The message to receive.</param>
        internal void ReceiveMessage(Message message)
        {
            var remote = message.Remote;

            // Check if there is message handler which receives the messages
            // from this remote. If there is none, we should buffer them
            // until there is a handler.
            if (m_MessageHandlers.TryGetValue(remote, out var handler))
            {
                try
                {
                    handler?.Invoke(message);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (!m_BufferedMessages.TryGetValue(remote, out var messages))
                {
                    messages = new Queue<Message>();
                    m_BufferedMessages.Add(remote, messages);
                }
                messages.Enqueue(message);
            }
        }

        /// <summary>
        /// Adds a new connection. This is thread-safe.
        /// </summary>
        /// <param name="connection">The new connection.</param>
        internal void RegisterConnection(Connection connection)
        {
            // register the connection on the next update so we can invoke the remote connected event
            // on the correct thread and ensure there are no ordering issues compared to terminated
            // connections.
            m_ConnectionEvents.Enqueue(new ConnectionEvent(connection, ConnectionEvent.Type.NewConnection));
        }

        /// <summary>
        /// Removes a connection.
        /// </summary>
        /// <param name="connection">The connection to remove.</param>
        /// <param name="status">How the connection was terminated.</param>
        internal void DeregisterConnection(Connection connection, DisconnectStatus status)
        {
            var remote = connection.Remote;

            m_RemoteToConnection.TryRemove(remote, out _);

            if (m_BufferedMessages.TryGetValue(remote, out var bufferedMessages))
            {
                foreach (var message in bufferedMessages)
                    message.Dispose();

                bufferedMessages.Clear();
            }
            m_BufferedMessages.Remove(remote);
            m_MessageHandlers.Remove(remote);

            try
            {
                RemoteDisconnected?.Invoke(remote, status);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Called to handle closing a connection that has encountered a fatal error.
        /// This is thread safe.
        /// </summary>
        /// <param name="connection">The connection to close.</param>
        internal void OnSocketError(Connection connection)
        {
            // We can't close connections on other threads, so we close the connection
            // on the next update.
            m_ConnectionEvents.Enqueue(new ConnectionEvent(connection, ConnectionEvent.Type.TerminateConnection));
        }

        /// <summary>
        /// Notify listeners that the networking is now running.
        /// </summary>
        protected void RaiseStartedEvent()
        {
            try
            {
                Started?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Sends the data used to initialize a connection to a remote on sockets
        /// that have just connected. This is thread-safe.
        /// </summary>
        /// <param name="tcp">A TCP socket that has just finished connecting.</param>
        /// <param name="udp">The UDP socket to use for this connection.</param>
        protected void DoHandshake(NetworkSocket tcp, NetworkSocket udp)
        {
            var remote = new Remote(Guid.Empty, tcp.RemoteEndPoint, null);
            var message = Message.Get(remote, ChannelType.ReliableOrdered);
            var packet = new Packet(message, Packet.Type.Initialization);

            message.Data.WriteStruct(new VersionData(ProtocolVersion));
            message.Data.WriteStruct(new RemoteData(ID, tcp.LocalEndPoint, udp.LocalEndPoint));

            tcp.Send(packet, false);
        }
    }
}
