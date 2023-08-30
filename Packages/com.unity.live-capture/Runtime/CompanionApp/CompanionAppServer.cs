using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Discovery;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// The server used to communicate with the companion apps.
    /// </summary>
    [CreateConnectionMenuItem("Companion App Server")]
    public class CompanionAppServer : Connection
    {
        const int k_DefaultPort = 9000;

        /// <summary>
        /// The server executes this event when a client has connected.
        /// </summary>
        public static event Action<ICompanionAppClient> ClientConnected = delegate { };

        /// <summary>
        /// The server executes this event when a client has disconnected.
        /// </summary>
        public static event Action<ICompanionAppClient> ClientDisconnected = delegate { };

        struct ConnectHandler
        {
            public string Name;
            public DateTime Time;
            public Func<ICompanionAppClient, bool> Handler;
        }

        static readonly Dictionary<string, Type> s_TypeToClientType = new Dictionary<string, Type>();
        static readonly List<ConnectHandler> s_ClientConnectHandlers = new List<ConnectHandler>();

        /// <summary>
        /// Adds a callback used to take ownership of a client that has connected.
        /// </summary>
        /// <param name="handler">The callback function. It must return true if it takes ownership of a client.</param>
        /// <param name="name">The name of the client to prefer. If set, this handler has priority over clients that have the given name.</param>
        /// <param name="time">The time used to determine the priority of handlers when many are listening for the same
        /// client <paramref name="name"/>. More recent values have higher priority.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
        public static void RegisterClientConnectHandler(Func<ICompanionAppClient, bool> handler, string name, DateTime time)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            DeregisterClientConnectHandler(handler);

            s_ClientConnectHandlers.Add(new ConnectHandler
            {
                Name = name,
                Time = time,
                Handler = handler,
            });
        }

        /// <summary>
        /// Removes a client connection callback.
        /// </summary>
        /// <param name="handler">The callback to remove.</param>>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
        public static void DeregisterClientConnectHandler(Func<ICompanionAppClient, bool> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            for (var i = 0; i < s_ClientConnectHandlers.Count; i++)
            {
                if (s_ClientConnectHandlers[i].Handler == handler)
                {
                    s_ClientConnectHandlers.RemoveAt(i);
                }
            }
        }

        static CompanionAppServer()
        {
            foreach (var (type, attributes) in AttributeUtility.GetAllTypes<ClientAttribute>())
            {
                if (!typeof(CompanionAppClient).IsAssignableFrom(type))
                {
                    Debug.LogError($"{type.FullName} must be assignable from {nameof(CompanionAppClient)} to use the {nameof(ClientAttribute)} attribute.");
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    s_TypeToClientType[attribute.Type] = type;
                }
            }
        }

#if UNITY_EDITOR
#pragma warning disable 414
        [SerializeField, HideInInspector]
        bool m_InterfacesExpanded = true;
        [SerializeField, HideInInspector]
        bool m_ClientsExpanded = true;
#pragma warning restore 414
#endif

        [SerializeField, Tooltip("The TCP port on which the server will listen for incoming connections. Changes to the port only take effect after restarting the server.")]
        int m_Port = k_DefaultPort;
        [SerializeField, Tooltip("Start the server automatically after entering play mode.")]
        bool m_AutoStartOnPlay = true;

        readonly DiscoveryServer m_Discovery = new DiscoveryServer();
        readonly NetworkServer m_Server = new NetworkServer();
        readonly Dictionary<Remote, ICompanionAppClient> m_RemoteToClient = new Dictionary<Remote, ICompanionAppClient>();

        /// <summary>
        /// The TCP port on which the server will listen for incoming connections.
        /// </summary>
        /// <remarks>
        /// Changes to the port only take effect after restarting the server.
        /// </remarks>
        public int Port
        {
            get => m_Port;
            set
            {
                if (m_Port != value)
                {
                    m_Port = value;
                    OnChanged(true);
                }
            }
        }

        /// <summary>
        /// Start the server automatically after entering play mode.
        /// </summary>
        public bool AutoStartOnPlay
        {
            get => m_AutoStartOnPlay;
            set
            {
                if (m_AutoStartOnPlay != value)
                {
                    m_AutoStartOnPlay = value;
                    OnChanged(true);
                }
            }
        }

        /// <summary>
        /// Are clients able to connect to the server.
        /// </summary>
        /// <returns>True if the server is enabled.</returns>
        public override bool IsEnabled()
        {
            return m_Server.IsRunning;
        }

        /// <summary>
        /// Starts/stops the server.
        /// </summary>
        /// <param name="enabled">The new state of the server.</param>
        public override void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                StartServer();
            }
            else
            {
                StopServer();
            }
        }

        /// <summary>
        /// The number of clients currently connected to the server.
        /// </summary>
        public int ClientCount => m_RemoteToClient.Count;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Server.RemoteConnected += OnClientConnected;
            m_Server.RemoteDisconnected += OnClientDisconnected;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            m_Discovery.Stop();
            m_Server.Stop();

            m_Server.RemoteConnected -= OnClientConnected;
            m_Server.RemoteDisconnected -= OnClientDisconnected;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                {
                    if (m_AutoStartOnPlay)
                    {
                        StartServer();
                    }
                    break;
                }
            }
        }

#endif

        /// <summary>
        /// Gets the currently connected clients.
        /// </summary>
        /// <returns>A new collection containing the client handles.</returns>
        public IEnumerable<ICompanionAppClient> GetClients()
        {
            return m_RemoteToClient.Values;
        }

        /// <inheritdoc />
        public override string GetName() => "Companion App Server";

        /// <summary>
        /// Start listening for clients connections.
        /// </summary>
        public void StartServer()
        {
            if (!NetworkUtilities.IsPortAvailable(m_Port))
            {
                Debug.LogError($"Unable to start server: Port {m_Port} is in use by another program! Close the other program, or assign a free port using the Live Capture Window.");
                return;
            }

            if (m_Server.StartServer(m_Port))
            {
                var machineName = Environment.MachineName;

                // start server discovery
                var config = new ServerData(
                    "Live Capture",
                    machineName.Substring(0, Math.Min(machineName.Length, 32)),
                    m_Server.ID,
                    PackageUtility.GetVersion(LiveCaptureInfo.Version)
                );
                var endPoints = m_Server.EndPoints.ToArray();

                m_Discovery.Start(config, endPoints);
            }

            OnChanged(false);
        }

        /// <summary>
        /// Disconnects all clients and stop listening for new connections.
        /// </summary>
        public void StopServer()
        {
            m_Server.Stop();
            m_Discovery.Stop();

            OnChanged(false);
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            m_Server.Update();
            m_Discovery.Update();
        }

        void OnClientConnected(Remote remote)
        {
            m_Server.RegisterMessageHandler(remote, InitializeClient, false);
        }

        void OnClientDisconnected(Remote remote, DisconnectStatus status)
        {
            if (m_RemoteToClient.TryGetValue(remote, out var client))
            {
                try
                {
                    ClientDisconnected.Invoke(client);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                m_RemoteToClient.Remove(remote);
                OnChanged(false);
            }
        }

        void InitializeClient(Message message)
        {
            try
            {
                if (message.ChannelType != ChannelType.ReliableOrdered)
                {
                    return;
                }

                var streamReader = new StreamReader(message.Data, Encoding.UTF8);
                var json = streamReader.ReadToEnd();
                var data = default(ClientInitialization);

                try
                {
                    data = JsonUtility.FromJson<ClientInitialization>(json);
                }
                catch (Exception)
                {
                    Debug.LogError($"{nameof(CompanionAppServer)} failed to initialize client connection! Could not parse JSON: {json}");
                    return;
                }

                if (!s_TypeToClientType.TryGetValue(data.Type, out var clientType))
                {
                    Debug.LogError($"Unknown client type \"{data.Type}\" connected to {nameof(CompanionAppServer)}!");
                    return;
                }

                var remote = message.Remote;
                var client = Activator.CreateInstance(clientType, m_Server, remote, data) as CompanionAppClient;
                client.SendProtocol();

                m_RemoteToClient.Add(remote, client);

                AssignOwner(client);

                ClientConnected.Invoke(client);
                OnChanged(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                message.Dispose();
            }
        }

        static void AssignOwner(ICompanionAppClient client)
        {
            // connect to the registered handler that was most recently used with this client if possible
            foreach (var handler in s_ClientConnectHandlers.OrderByDescending(h => h.Time.Ticks))
            {
                try
                {
                    if (handler.Name == client.Name)
                    {
                        if (handler.Handler(client))
                            return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // fall back to the first free device that is compatible with the client
            foreach (var handler in s_ClientConnectHandlers)
            {
                try
                {
                    if (handler.Handler(client))
                        return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
