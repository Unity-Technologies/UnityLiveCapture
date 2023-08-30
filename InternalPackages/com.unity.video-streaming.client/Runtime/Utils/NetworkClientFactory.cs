using System.Net.Sockets;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Client.Utils
{
    static class NetworkClientFactory
    {
        private const int TcpReceiveBufferDefaultSize = 64 * 1024;
        private const int SIO_UDP_CONNRESET = -1744830452;
        private static readonly byte[] EmptyOptionInValue = { 0, 0, 0, 0 };

        public static Socket CreateTcpClient()
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                SendBufferSize = 4 * 1024,
                ReceiveBufferSize = TcpReceiveBufferDefaultSize,
                DualMode = true,
                NoDelay = true
            };
            return socket;
        }

        public static Socket CreateUdpClient()
        {
            //Debug.Log("NetworkClientFactory.CreateUdpClient");
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)
            {
                DualMode = true
            };
            if (socket == null)
                Debug.LogError("UDP client creation failed");
            else
            {
                //Debug.Log("UDP client creation succeeded");
            }

#if !UNITY_IPHONE && !UNITY_STANDALONE_OSX
            // This throws an exception on Apple platforms. Doesn't seem to be needed in there.
            socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, EmptyOptionInValue, null);
#endif
            return socket;
        }
    }
}
