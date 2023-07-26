using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    /// <summary>
    /// Manages a connection to a server.
    /// </summary>
    sealed class RemoteSystem : IRemoteSystem, IInitializable, ILateTickable, ILateDisposable
    {
        [Inject]
        ICompanionAppHostFactory m_HostFactory;
        [Inject]
        SignalBus m_SignalBus;

        readonly NetworkClient m_Client = new NetworkClient();
        CompanionAppHost m_Host;
        float m_TimeElapsedSinceConnectionRequest;
        bool m_IsTryingToConnect;
        string m_Ip;
        int m_Port;
        string m_ClientType;

        public CompanionAppHost Host => m_Host;

        /// <summary>
        /// Connect to the server and start streaming data.
        /// </summary>
        /// <param name="connectAttemptTimeout">How long in milliseconds to wait between connection attempts.</param>
        public void Connect(string ip, int port, string clientType, int connectAttemptTimeout = 2000)
        {
            m_Ip = ip;
            m_Port = port;
            m_ClientType = clientType;
            m_IsTryingToConnect = true;
            m_TimeElapsedSinceConnectionRequest = 0;
            m_Client.ConnectAttemptTimeout = connectAttemptTimeout;
            m_Client.ConnectToServer(ip, port);
        }

        /// <summary>
        /// Close the connection with the server and stop streaming data.
        /// </summary>
        public void Disconnect()
        {
            m_Client.Stop();
        }

        public void Initialize()
        {
            m_Client.RemoteConnected += (remote) =>
            {
                var initialization = new ClientInitialization
                {
                    Name = Environment.MachineName,
                    ID = GenerateGuid(m_Client.LocalEndPoint.Address).ToString(),
                    Type = m_ClientType,
                    ScreenResolution = new Vector2Int(Screen.width, Screen.height),
                };

                var json = Encoding.UTF8.GetBytes(JsonUtility.ToJson(initialization));

                var message = Message.Get(remote, ChannelType.ReliableOrdered, json.Length);
                message.Data.Write(json, 0, json.Length);

                m_Client.SendMessage(message);
                m_Client.RegisterMessageHandler(remote, ReceiveProtocol);
            };
            m_Client.RemoteDisconnected += (remote, status) =>
            {
                m_SignalBus.Fire<RemoteDisconnectedSignal>();

                m_Host = null;
            };
        }

        void ReceiveProtocol(Message message)
        {
            try
            {
                m_Host = m_HostFactory.CreateHost(m_Client, message.Remote, message.Data);

                m_IsTryingToConnect = false;

                m_SignalBus.Fire(new RemoteConnectedSignal()
                {
                    companionAppHost = m_Host, ip = m_Ip, port = m_Port
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse network protocol! {e}");
            }
        }

        public void LateDispose()
        {
            Disconnect();
        }

        public void LateTick()
        {
            m_Client.Update();

            if (m_IsTryingToConnect)
            {
                m_TimeElapsedSinceConnectionRequest += Time.deltaTime * 1000f;

                if (m_TimeElapsedSinceConnectionRequest > m_Client.ConnectAttemptTimeout)
                {
                    m_IsTryingToConnect = false;

                    Disconnect();

                    m_SignalBus.Fire<RemoteConnectionFailedSignal>();
                }
            }
        }

        Guid GenerateGuid(IPAddress address)
        {
            // Create an id based on the mac address. This will allow identification
            // of this device between sessions.
            var physicalAddress = NetworkUtilities.GetPhysicalAddress(address);

            if (physicalAddress == null)
                return m_Client.ID;

            var mac = physicalAddress.GetAddressBytes();
            var buffer = new byte[Marshal.SizeOf<Guid>()];
            Array.Copy(mac, buffer, Mathf.Min(buffer.Length, mac.Length));

            return new Guid(buffer);
        }
    }
}
