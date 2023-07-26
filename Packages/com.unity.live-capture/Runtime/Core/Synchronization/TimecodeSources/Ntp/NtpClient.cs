using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Unity.LiveCapture.Ntp
{
    /// <summary>
    /// An client which can poll an NTP server for the time.
    /// </summary>
    class NtpClient : IDisposable
    {
        /// <summary>
        /// The request timeout in milliseconds.
        /// </summary>
        const int k_Timeout = 1000;

        readonly NtpPacket m_Packet = new NtpPacket();
        Socket m_Socket;
        string m_ConnectedAddress;

        /// <summary>
        /// The finalizer attempts to dispose the client in case it is not properly disposed.
        /// </summary>
        ~NtpClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the client instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            Disconnect();
        }

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        /// <remarks>
        /// This cancels any operations currently in progress.
        /// </remarks>
        public void Disconnect()
        {
            if (m_Socket != null)
            {
                m_Socket.Dispose();
                m_Socket = null;
            }

            m_ConnectedAddress = null;
        }

        public async Task<DateTime?> PollTimeAsync(string hostOrAddress, DateTime? currentTime = null)
        {
            // if the host name has changed we need to reconnect the socket
            if (m_ConnectedAddress == null || hostOrAddress != m_ConnectedAddress)
            {
                Connect(hostOrAddress);
            }

            // check that the connection is valid
            if (m_ConnectedAddress == null)
            {
                return null;
            }

            // record the time on the client when the request starts
            var startTime = currentTime ?? DateTime.Now;
            var sw = new Stopwatch();
            sw.Start();

            // prepare the packet buffer used to send the request and read the response
            m_Packet.Reset();
            m_Packet.TransmitTimestamp = startTime;

            // send the time request
            try
            {
                var requestSegment = new ArraySegment<byte>(m_Packet.Buffer, 0, NtpConstants.PacketLength);
                await m_Socket.SendAsync(requestSegment, SocketFlags.None).ConfigureAwait(false);
            }
            catch (SocketException e)
            {
                if (ShouldLogError(e))
                {
                    Debug.LogError($"Failed to send NTP request: {e}");
                }
                return null;
            }

            // get the servers response
            try
            {
                var responseSegment = new ArraySegment<byte>(m_Packet.Buffer, 0, m_Packet.Buffer.Length);
                var responseSize = await m_Socket.ReceiveAsync(responseSegment, SocketFlags.None).ConfigureAwait(false);

                if (responseSize < NtpConstants.PacketLength || m_Packet.Mode != NtpMode.Server)
                {
                    return null;
                }
            }
            catch (SocketException e)
            {
                if (ShouldLogError(e))
                {
                    Debug.LogError($"Failed to receive NTP response: {e}");
                }
                return null;
            }

            sw.Stop();

            // We use the time on the server to calculate the time offset to apply to the client clock so that
            // it matches the server time.
            var receiveTimestamp = m_Packet.ReceiveTimestamp;
            var originateTimestamp = m_Packet.OriginateTimestamp;
            var transmitTimestamp = m_Packet.TransmitTimestamp;

            // this is the time on the client when the response with the server times is received
            var destinationTimestamp = startTime + TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

            // offset the client time using the formula given in the NTP standard doc
            var correctionOffset = ((receiveTimestamp - originateTimestamp) - (destinationTimestamp - transmitTimestamp)).Ticks / 2;
            return destinationTimestamp + TimeSpan.FromTicks(correctionOffset);
        }

        void Connect(string hostOrAddress)
        {
            // close the previous socket
            Disconnect();

            // perform basic validation of the host name
            if (hostOrAddress == null)
            {
                return;
            }

            var trimmedHost = hostOrAddress.Trim();

            if (trimmedHost.Length == 0)
            {
                return;
            }

            try
            {
                // create a UDP socket used to communicate with the named host
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    ReceiveTimeout = k_Timeout,
                };

                // Connect to the host asynchronously. UDP is connectionless, but this avoids blocking when a host name
                // is used instead of an IP which requires a DNS lookup. It is also required to call connect before using
                // certain send or receive methods.
                m_Socket.Connect(trimmedHost, NtpConstants.Port);

                // store the hostname of the connected server
                m_ConnectedAddress = hostOrAddress;
            }
            catch (SocketException e)
            {
                if (ShouldLogError(e))
                {
                    Debug.LogError($"Cannot connect to NTP server \"{hostOrAddress}\": {e}");
                }
                Disconnect();
            }
        }

        static bool ShouldLogError(SocketException e)
        {
            switch (e.SocketErrorCode)
            {
                // When the socket is disposed, suppress the error since it is expected that
                // the operation should not complete.
                case SocketError.Shutdown:
                case SocketError.Interrupted:
                case SocketError.OperationAborted:
                case SocketError.ConnectionAborted:
                case SocketError.Disconnecting:
                    return false;
            }
            return true;
        }
    }
}
