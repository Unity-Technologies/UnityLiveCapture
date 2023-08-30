using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

// RTSP Server Example (c) Roger Hardiman, 2016, 2018
// Released uder the MIT Open Source Licence
//
// Re-uses some code from the Multiplexer example of SharpRTSP
//
// This example simulates a live RTSP video stream, for example a CCTV Camera
// It creates a Video Source (a test card) that creates a YUV Image
// The image is then encoded as H264 data using a very basic H264 Encoder
// The H264 data (the NALs) are sent to the RTSP clients
// Video is sent in UDP Mode or TCP Mode (ie RTP over RTSP mode)

// The Tiny H264 Encoder is a 100% .NET encoder which is lossless and creates large bitstreams as
// there is no compression. It is limited to 128x96 resolution. However it makes it easy to write a quick
// demo without needing native APIs or cross compiled C libraries for H264
namespace Unity.LiveCapture.VideoStreaming.Server
{
    class RtspServer : IDisposable
    {
        const int h264_width = 224; // Tiny needs 128x96
        const int h264_height = 224;
        const int h264_fps = 25;

        // Was 65000 in original code. 64593 seems to be the max possible because of 7 more bytes added elsewhere.
        // 1400 would seem more network-friendly but we're experimenting with LAN for now.
        const int kMaxNalUnitSize = 64593;

        const uint global_ssrc = 0x4321FADE; // 8 hex digits

        private TcpListener _RTSPServerListener;
        private ManualResetEvent _Stopping;
        private Thread _ListenTread;

        private ArraySegment<byte> raw_video_nal;

        List<RTSPConnection> rtsp_list = new List<RTSPConnection>(); // list of RTSP Listeners

        System.Random rnd = new System.Random();
        int session_handle = 1;

        Authentication auth = null;

        public int port => _RTSPServerListener.LocalEndpoint is IPEndPoint endPoint ? endPoint.Port : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RTSPServer"/> class.
        /// </summary>
        /// <param name="aPortNumber">A numero port.</param>
        /// <param name="username">username.</param>
        /// <param name="password">password.</param>
        public RtspServer(int portNumber, string username, string password)
        {
            raw_video_nal = new ArraySegment<byte>(new byte[kMaxNalUnitSize]);

            if (portNumber < IPEndPoint.MinPort || portNumber > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(portNumber), portNumber, "Port number must be between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort");

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                auth = new Authentication(username, password, "SharpRTSPServer", Authentication.Type.Digest);
            }
            else
            {
                auth = null;
            }

            RtspUtils.RegisterUri();
            _RTSPServerListener = new TcpListener(IPAddress.Any, portNumber);
        }

        public bool StartListen()
        {
            if (_ListenTread != null)
            {
                Debug.LogError("RTSP server already started.");
                return false;
            }

            _RTSPServerListener.Start();
            _Stopping = new ManualResetEvent(false);
            _ListenTread = new Thread(new ThreadStart(AcceptConnection));
            _ListenTread.Start();
            return true;
        }

        /// <summary>
        /// Accepts the connection.
        /// </summary>
        private void AcceptConnection()
        {
            try
            {
                Profiler.BeginThreadProfiling("RTSP", "RTSPServer.AcceptConnection");
                while (!_Stopping.WaitOne(0))
                {
                    // Wait for an incoming TCP Connection
                    TcpClient oneClient = _RTSPServerListener.AcceptTcpClient();

                    Profiler.BeginSample("Add connection");

                    //Debug.Log("Connection from " + oneClient.Client.RemoteEndPoint.ToString());

                    // Hand the incoming TCP connection over to the RTSP classes
                    var rtsp_socket = new RtspTcpTransport(oneClient);
                    RtspListener newListener = new RtspListener(rtsp_socket);
                    newListener.MessageReceived += RTSP_Message_Received;

                    //RTSPDispatcher.Instance.AddListener(newListener);

                    // Add the RtspListener to the RTSPConnections List
                    RTSPConnection new_connection = new RTSPConnection();
                    new_connection.listener = newListener;
                    new_connection.client_hostname = newListener.RemoteAdress.Split(':')[0];
                    new_connection.ssrc = global_ssrc;

                    new_connection.time_since_last_rtsp_keepalive = DateTime.UtcNow;
                    new_connection.video_time_since_last_rtcp_keepalive = DateTime.UtcNow;

                    lock (rtsp_list)
                        rtsp_list.Add(new_connection);

                    newListener.Start();

                    Profiler.EndSample();
                }
            }
            catch (SocketException /*error*/)
            {
                // _logger.Warn("Got an error listening, I have to handle the stopping which also throw an error", error);
            }
            catch (Exception /*error*/)
            {
                // _logger.Error("Got an error listening...", error);
                throw;
            }
        }

        virtual public void StopListen()
        {
            _RTSPServerListener.Stop();
            _Stopping?.Set();
            _ListenTread?.Join();
        }

        #region IDisposable Membres

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopListen();
                _Stopping?.Dispose();
            }
        }

        #endregion

        // Process each RTSP message that is received
        private void RTSP_Message_Received(object sender, RtspChunkEventArgs e)
        {
            // Cast the 'sender' and 'e' into the RTSP Listener (the Socket) and the RTSP Message
            RtspListener listener = sender as RtspListener;
            Messages.RtspMessage message = e.Message as Messages.RtspMessage;

            // Check if the RTSP Message has valid authentication (validating against username,password,realm and nonce)
            if (auth != null)
            {
                bool authorized = false;
                if (message.Headers.ContainsKey("Authorization") == true)
                {
                    // The Header contained Authorization
                    // Check the message has the correct Authorization
                    // If it does not have the correct Authorization then close the RTSP connection
                    authorized = auth.IsValid(message);

                    if (authorized == false)
                    {
                        // Send a 401 Authentication Failed reply, then close the RTSP Socket
                        Messages.RtspResponse authorization_response = (e.Message as Messages.RtspRequest).CreateResponse();
                        authorization_response.AddHeader("WWW-Authenticate: " + auth.GetHeader());
                        authorization_response.ReturnCode = 401;
                        listener.SendMessage(authorization_response);

                        lock (rtsp_list)
                        {
                            foreach (RTSPConnection connection in rtsp_list.ToArray())
                            {
                                if (connection.listener == listener)
                                {
                                    rtsp_list.Remove(connection);
                                }
                            }
                        }

                        listener.Dispose();
                        return;
                    }
                }

                if ((message.Headers.ContainsKey("Authorization") == false))
                {
                    // Send a 401 Authentication Failed with extra info in WWW-Authenticate
                    // to tell the Client if we are using Basic or Digest Authentication
                    Messages.RtspResponse authorization_response = (e.Message as Messages.RtspRequest).CreateResponse();
                    authorization_response.AddHeader("WWW-Authenticate: " + auth.GetHeader()); // 'Basic' or 'Digest'
                    authorization_response.ReturnCode = 401;
                    listener.SendMessage(authorization_response);
                    return;
                }
            }

            // Update the RTSP Keepalive Timeout
            // We could check that the message is GET_PARAMETER or OPTIONS for a keepalive but instead we will update the timer on any message
            lock (rtsp_list)
            {
                foreach (RTSPConnection connection in rtsp_list)
                {
                    if (connection.listener.RemoteAdress.Equals(listener.RemoteAdress))
                    {
                        // found the connection
                        connection.time_since_last_rtsp_keepalive = DateTime.UtcNow;
                        break;
                    }
                }
            }


            // Handle OPTIONS message
            if (message is Messages.RtspRequestOptions)
            {
                // Create the reponse to OPTIONS
                Messages.RtspResponse options_response = (e.Message as Messages.RtspRequestOptions).CreateResponse();
                listener.SendMessage(options_response);
            }

            // Handle DESCRIBE message
            if (message is Messages.RtspRequestDescribe)
            {
                String requested_url = (message as Messages.RtspRequestDescribe).RtspUri.ToString();

                //Debug.Log("Request for " + requested_url);

                // TODO. Check the requsted_url is valid. In this example we accept any RTSP URL

                // Make the Base64 SPS and PPS
                String sps_str = Convert.ToBase64String(new byte[0]);
                String pps_str = Convert.ToBase64String(new byte[0]);

                //Debug.LogFormat("sps: {0}, pps: {1}", sps_str, pps_str);
                StringBuilder sdp = new StringBuilder();

                // Generate the SDP
                // The sprop-parameter-sets provide the SPS and PPS for H264 video
                // The packetization-mode defines the H264 over RTP payloads used but is Optional
                sdp.Append("v=0\n");
                sdp.Append("o=user 123 0 IN IP4 0.0.0.0\n");
                sdp.Append("s=SharpRTSP Test Camera\n");
                sdp.Append("m=video 0 RTP/AVP 96\n");
                sdp.Append("c=IN IP4 0.0.0.0\n");
                sdp.Append("a=control:trackID=0\n");
                sdp.Append("a=rtpmap:96 H264/90000\n");
                sdp.Append("a=fmtp:96 profile-level-id=42A01E; sprop-parameter-sets=" + sps_str + "," + pps_str + ";\n");

                byte[] sdp_bytes = Encoding.ASCII.GetBytes(sdp.ToString());

                // Create the reponse to DESCRIBE
                // This must include the Session Description Protocol (SDP)
                Messages.RtspResponse describe_response = (e.Message as Messages.RtspRequestDescribe).CreateResponse();

                describe_response.AddHeader("Content-Base: " + requested_url);
                describe_response.AddHeader("Content-Type: application/sdp");
                describe_response.Data = sdp_bytes;
                describe_response.AdjustContentLength();
                listener.SendMessage(describe_response);
            }

            // Handle SETUP message
            if (message is Messages.RtspRequestSetup)
            {
                //
                var setupMessage = message as Messages.RtspRequestSetup;

                // Check the RTSP transport
                // If it is UDP or Multicast, create the sockets
                // If it is RTP over RTSP we send data via the RTSP Listener

                // FIXME client may send more than one possible transport.
                // very rare
                Messages.RtspTransport transport = setupMessage.GetTransports()[0];


                // Construct the Transport: reply from the Server to the client
                Messages.RtspTransport transport_reply = new Messages.RtspTransport();
                transport_reply.SSrc = global_ssrc.ToString("X8"); // Convert to Hex, padded to 8 characters

                if (transport.LowerTransport == Messages.RtspTransport.LowerTransportType.TCP)
                {
                    // RTP over RTSP mode}
                    transport_reply.LowerTransport = Messages.RtspTransport.LowerTransportType.TCP;
                    transport_reply.Interleaved = new Messages.PortCouple(transport.Interleaved.First, transport.Interleaved.Second);
                }

                UDPSocket udp_pair = null;
                if (transport.LowerTransport == Messages.RtspTransport.LowerTransportType.UDP
                    && transport.IsMulticast == false)
                {
                    // RTP over UDP mode.
                    // Create a pair of UDP sockets - One is for the Video, one is for the RTCP.
                    lock (rtsp_list)
                    {
                        foreach (RTSPConnection connection in rtsp_list)
                        {
                            if (connection.listener.RemoteAdress.Equals(listener.RemoteAdress))
                            {
                                // Give a range of 500 pairs (1000 addresses) to try incase some address are in use.
                                // Also pass in the local IP address, so the socket is bound on the right interface
                                var localAddress = IPAddress.Parse(connection.listener.LocalAddress.Split(':')[0]);
                                udp_pair = new UDPSocket(50000, 51000, localAddress);
                                break;
                            }
                        }
                    }

                    udp_pair.Start(); // start listening for data on the UDP ports

                    // Pass the Port of the two sockets back in the reply
                    transport_reply.LowerTransport = Messages.RtspTransport.LowerTransportType.UDP;
                    transport_reply.IsMulticast = false;
                    transport_reply.ServerPort = new Messages.PortCouple(udp_pair.data_port, udp_pair.control_port);
                }

                if (transport.LowerTransport == Messages.RtspTransport.LowerTransportType.UDP
                    && transport.IsMulticast == true)
                {
                    // RTP over Multicast UDP mode}
                    // Create a pair of UDP sockets in Multicast Mode
                    // Pass the Ports of the two sockets back in the reply
                    transport_reply.LowerTransport = Messages.RtspTransport.LowerTransportType.UDP;
                    transport_reply.IsMulticast = true;
                    transport_reply.Port = new Messages.PortCouple(7000, 7001); // FIX

                    // for now until implemented
                    transport_reply = null;
                }


                if (transport_reply != null)
                {
                    // Update the session with transport information
                    String copy_of_session_id = "";
                    lock (rtsp_list)
                    {
                        foreach (RTSPConnection connection in rtsp_list)
                        {
                            if (connection.listener.RemoteAdress.Equals(listener.RemoteAdress))
                            {
                                // ToDo - Check the Track ID to determine if this is a SETUP for the Video Stream
                                // or a SETUP for an Audio Stream.
                                // In the SDP the H264 video track is TrackID 0


                                // found the connection
                                // Add the transports to the connection
                                connection.video_client_transport = transport;
                                connection.video_transport_reply = transport_reply;

                                // If we are sending in UDP mode, add the UDP Socket pair and the Client Hostname
                                connection.video_udp_pair = udp_pair;


                                connection.video_session_id = session_handle.ToString();
                                session_handle++;


                                // Copy the Session ID
                                copy_of_session_id = connection.video_session_id;
                                break;
                            }
                        }
                    }

                    Messages.RtspResponse setup_response = setupMessage.CreateResponse();
                    setup_response.Headers[Messages.RtspHeaderNames.Transport] = transport_reply.ToString();
                    setup_response.Session = copy_of_session_id;
                    listener.SendMessage(setup_response);
                }
                else
                {
                    Messages.RtspResponse setup_response = setupMessage.CreateResponse();

                    // unsuported transport
                    setup_response.ReturnCode = 461;
                    listener.SendMessage(setup_response);
                }
            }

            // Handle PLAY message (Sent with a Session ID)
            if (message is Messages.RtspRequestPlay)
            {
                lock (rtsp_list)
                {
                    // Search for the Session in the Sessions List. Change the state to "PLAY"
                    bool session_found = false;
                    foreach (RTSPConnection connection in rtsp_list)
                    {
                        if (message.Session == connection.video_session_id) /* OR AUDIO_SESSION_ID */
                        {
                            // found the session
                            session_found = true;
                            connection.play = true; // ACTUALLY YOU COULD PAUSE JUST THE VIDEO (or JUST THE AUDIO)

                            string range = "npt=0-"; // Playing the 'video' from 0 seconds until the end
                            string rtp_info = "url=" + ((Messages.RtspRequestPlay)message).RtspUri + ";seq=" + connection.video_sequence_number; // TODO Add rtptime  +";rtptime="+session.rtp_initial_timestamp;

                            // Send the reply
                            Messages.RtspResponse play_response = (e.Message as Messages.RtspRequestPlay).CreateResponse();
                            play_response.AddHeader("Range: " + range);
                            play_response.AddHeader("RTP-Info: " + rtp_info);
                            listener.SendMessage(play_response);

                            break;
                        }
                    }

                    if (session_found == false)
                    {
                        // Session ID was not found in the list of Sessions. Send a 454 error
                        Messages.RtspResponse play_failed_response = (e.Message as Messages.RtspRequestPlay).CreateResponse();
                        play_failed_response.ReturnCode = 454; // Session Not Found
                        listener.SendMessage(play_failed_response);
                    }
                }
            }

            // Handle PAUSE message (Sent with a Session ID)
            if (message is Messages.RtspRequestPause)
            {
                lock (rtsp_list)
                {
                    // Search for the Session in the Sessions List. Change the state of "PLAY"
                    foreach (RTSPConnection connection in rtsp_list)
                    {
                        if (message.Session == connection.video_session_id /* OR AUDIO SESSION ID */)
                        {
                            // found the session
                            connection.play = false; // COULD HAVE PLAY/PAUSE FOR VIDEO AND AUDIO
                            break;
                        }
                    }
                }

                // ToDo - only send back the OK response if the Session in the RTSP message was found
                Messages.RtspResponse pause_response = (e.Message as Messages.RtspRequestPause).CreateResponse();
                listener.SendMessage(pause_response);
            }


            // Handle GET_PARAMETER message, often used as a Keep Alive
            if (message is Messages.RtspRequestGetParameter)
            {
                // Create the reponse to GET_PARAMETER
                Messages.RtspResponse getparameter_response = (e.Message as Messages.RtspRequestGetParameter).CreateResponse();
                listener.SendMessage(getparameter_response);
            }


            // Handle TEARDOWN (sent with a Session ID)
            if (message is Messages.RtspRequestTeardown)
            {
                lock (rtsp_list)
                {
                    // Search for the Session in the Sessions List.
                    foreach (RTSPConnection connection in rtsp_list.ToArray()) // Convert to ToArray so we can delete from the rtp_list
                    {
                        if (message.Session == connection.video_session_id) // SHOULD HAVE AN AUDIO TEARDOWN AS WELL
                        {
                            // If this is UDP, close the transport
                            // For TCP there is no transport to close (as RTP packets were interleaved into the RTSP connection)
                            if (connection.video_udp_pair != null)
                            {
                                connection.video_udp_pair.Stop();
                                connection.video_udp_pair = null;
                            }

                            rtsp_list.Remove(connection);

                            // Close the RTSP socket
                            listener.Dispose();
                        }
                    }
                }
            }
        }

        private void AddSTAPANalu(ArraySegment<byte> nalu, int naluStartByteIdx, int naluEndByteIdx, List<byte> rtp_packet, bool includeSize = true)
        {
            //Debug.Log($"Found NALU {nalu[naluStartByteIdx] & 0x1F}, NRI = {(nalu[naluStartByteIdx] & 0x60) >> 5}, at [{naluStartByteIdx}, {naluEndByteIdx}]");

            if (includeSize)
            {
                ushort naluSz = (ushort)(naluEndByteIdx - naluStartByteIdx);
                rtp_packet.Add((byte)(naluSz >> 8));
                rtp_packet.Add((byte)(naluSz & 0x00FF));
            }

            for (var i = naluStartByteIdx; i < naluEndByteIdx; ++i)
                rtp_packet.Add(nalu.Array[i]);
        }

        public void SendNALUs(ulong timeStampNs, ArraySegment<byte> spsNalu, ArraySegment<byte> ppsNalu, ArraySegment<byte> imageNalu)
        {
            UInt32 rtp_timestamp = (UInt32)(timeStampNs * 9 / 100000); // 90kHz clock

            // Build a list of 1 or more RTP packets
            // The last packet will have the M bit set to '1'
            List<byte[]> rtp_packets = new List<byte[]>();

            var nal_array = new[] { spsNalu, ppsNalu, imageNalu };

            for (int x = 0; x < nal_array.Length; x++)
            {
                var raw_nal = nal_array[x];

                if (raw_nal == null || raw_nal.Count == 0)
                    continue;

                Boolean last_nal = false;
                if (x == nal_array.Length - 1)
                {
                    last_nal = true; // last NAL in our nal_array
                }

                // The H264 Payload could be sent as one large RTP packet (assuming the receiver can handle it)
                // or as a Fragmented Data, split over several RTP packets with the same Timestamp.
                bool fragmenting = false;
                int packetMTU = kMaxNalUnitSize;
                if (raw_nal.Count > packetMTU) fragmenting = true;

                //Debug.LogFormat("Sending NALU {0}, size = {1}, fragmenting={2}", x, raw_nal.Length, fragmenting);

                if (fragmenting == false)
                {
                    Profiler.BeginSample("Create unfragmented RTP packet");

                    // Put the whole NAL into one RTP packet.
                    // Note some receivers will have maximum buffers and be unable to handle large RTP packets.
                    // Also with RTP over RTSP there is a limit of 65535 bytes for the RTP packet.
                    var stapHeaderSize = 0; // Disabling stap logic to try slicing instead // last_nal ? 1 : 0;
                    byte[] rtp_packet = new byte[12 + stapHeaderSize + raw_nal.Count]; // 12 is header size when there are no CSRCs or extensions

                    // Create an single RTP fragment

                    // RTP Packet Header
                    // 0 - Version, P, X, CC, M, PT and Sequence Number
                    //32 - Timestamp. H264 uses a 90kHz clock
                    //64 - SSRC
                    //96 - CSRCs (optional)
                    //nn - Extension ID and Length
                    //nn - Extension header

                    int rtp_version = 2;
                    int rtp_padding = 0;
                    int rtp_extension = 0;
                    int rtp_csrc_count = 0;
                    int rtp_marker = (last_nal == true ? 1 : 0); // set to 1 if the last NAL in the array
                    int rtp_payload_type = 96;

                    RTPPacketUtil.WriteHeader(rtp_packet, rtp_version, rtp_padding, rtp_extension, rtp_csrc_count, rtp_marker, rtp_payload_type);

                    UInt32 empty_sequence_id = 0;
                    RTPPacketUtil.WriteSequenceNumber(rtp_packet, empty_sequence_id);

                    RTPPacketUtil.WriteTS(rtp_packet, rtp_timestamp);

                    UInt32 empty_ssrc = 0;
                    RTPPacketUtil.WriteSSRC(rtp_packet, empty_ssrc);

                    bool enableSTAP = true;
                    if (enableSTAP && !last_nal)
                    {
                        // First 2 NALUs are SPS/PPS. Each fits in a single rtp packet.
                        Array.Copy(raw_nal.Array, 0, rtp_packet, 12, raw_nal.Count);
                        rtp_packets.Add(rtp_packet);
                    }
                    else if (enableSTAP)
                    {
                        var rtpPacket = new List<byte>(rtp_packet.Length);
                        for (int i = 0; i < 12; ++i)
                            rtpPacket.Add(rtp_packet[i]);

                        // Encoders sometimes give us a few NALUs per packet so we have to sift through the data and
                        // transform it into STAP-A format (Single Time Aggregation Packet type A, meaning they all
                        // have the same time stamp). This may end up wasting 1 byte if there was a single NALU in
                        // the data, but as of this writing there are always many NALUs in what the Media Foundation
                        // H.264 encoder produces.
                        byte activeNri = 1 << 5; // FIXME: NRI should be the max of all NALUs. But to do it properly we'd

                        // need to go through all the NALUs beforehand... We'll see if it makes
                        // a difference.
                        byte stapAPayloadFormat = 24;
                        rtpPacket.Add((byte)(activeNri | stapAPayloadFormat));
                        int nalStartByteIdx = 0;
                        int nalEndByteIdx = 0;

                        int naluCount = 0;
                        for (var i = 0; i < raw_nal.Count;)
                        {
                            nalEndByteIdx = i;
                            if (raw_nal.Array[i++] != 0)
                                continue;
                            if (i >= raw_nal.Count || raw_nal.Array[i++] != 0)
                                continue;
                            if (i >= raw_nal.Count || raw_nal.Array[i++] != 0)
                                continue;
                            if (i >= raw_nal.Count || raw_nal.Array[i++] != 1)
                                continue;

                            // Found NALU start. Copy previous NALU into packet with its size as shown in
                            // Figure 7 of RFC 3984.
                            if (nalStartByteIdx < nalEndByteIdx)
                            {
                                AddSTAPANalu(raw_nal, nalStartByteIdx, nalEndByteIdx, rtpPacket);
                                ++naluCount;
                            }
                            else if (nalEndByteIdx > 0)
                            {
                                //Debug.Log($"Found empty NALU at {nalStartByteIdx}");
                            }

                            nalStartByteIdx = nalEndByteIdx = i;
                        }

                        if (nalStartByteIdx < (raw_nal.Count - 1))
                        {
                            //Debug.Log("Handling ending NALU");
                            AddSTAPANalu(raw_nal, nalStartByteIdx, raw_nal.Count, rtpPacket);
                            ++naluCount;
                        }

                        rtp_packet = rtpPacket.ToArray();

                        //Debug.LogFormat("STAP-A packet size: {0}, containing {1} NAL units. ==========================", rtp_packet.Length, naluCount);
                        rtp_packets.Add(rtp_packet);
                    }
                    else
                    {
                        // Encoders sometimes give us a few NALUs per packet so we have to sift through the data and
                        // transform it into multiple RTP packets, even if it adds undesirable network overhead.
                        // The STAP-A variant doesn't work as of this writing, so here's a different approach.
                        int nalStartByteIdx = 0;
                        int nalEndByteIdx = 0;
                        for (var i = 0; i < raw_nal.Count;)
                        {
                            nalEndByteIdx = i;
                            if (raw_nal.Array[i++] != 0)
                                continue;
                            if (i >= raw_nal.Count || raw_nal.Array[i++] != 0)
                                continue;
                            if (i >= raw_nal.Count || raw_nal.Array[i++] != 0)
                                continue;
                            if (i >= raw_nal.Count || raw_nal.Array[i++] != 1)
                                continue;

                            var rtpPacket = new List<byte>(12);
                            for (int j = 0; j < 12; ++j)
                                rtpPacket.Add(rtp_packet[j]);

                            // Found NALU start. Copy previous NALU into packet with its size as shown in
                            // Figure 7 of RFC 3984.
                            if (nalStartByteIdx < nalEndByteIdx)
                            {
                                AddSTAPANalu(raw_nal, nalStartByteIdx, nalEndByteIdx, rtpPacket, false);
                                rtp_packets.Add(rtpPacket.ToArray());
                            }
                            else
                            {
                                //Debug.Log($"Found empty NALU at {nalStartByteIdx}");
                            }

                            nalStartByteIdx = nalEndByteIdx = i;
                        }

                        if (nalStartByteIdx < (raw_nal.Count - 1))
                        {
                            //Debug.Log("Handling ending NALU");
                            var rtpPacket = new List<byte>(12);
                            for (int i = 0; i < 12; ++i)
                                rtpPacket.Add(rtp_packet[i]);
                            AddSTAPANalu(raw_nal, nalStartByteIdx, raw_nal.Count, rtpPacket);
                            rtp_packets.Add(rtpPacket.ToArray());
                        }
                    }
                }
                else
                {
                    Profiler.BeginSample("Create fragmented RTP packet");
                    int data_remaining = raw_nal.Count;
                    int nal_pointer = 0;
                    int start_bit = 1;
                    int end_bit = 0;

                    // consume first byte of the raw_nal. It is used in the FU header
                    byte first_byte = raw_nal.Array[0];
                    nal_pointer++;
                    data_remaining--;

                    while (data_remaining > 0)
                    {
                        int payload_size = Math.Min(packetMTU, data_remaining);
                        if (data_remaining - payload_size == 0) end_bit = 1;

                        byte[] rtp_packet = new byte[12 + 2 + payload_size]; // 12 is header size. 2 bytes for FU-A header. Then payload

                        // RTP Packet Header
                        // 0 - Version, P, X, CC, M, PT and Sequence Number
                        //32 - Timestamp. H264 uses a 90kHz clock
                        //64 - SSRC
                        //96 - CSRCs (optional)
                        //nn - Extension ID and Length
                        //nn - Extension header

                        int rtp_version = 2;
                        int rtp_padding = 0;
                        int rtp_extension = 0;
                        int rtp_csrc_count = 0;
                        int rtp_marker = (last_nal == true ? 1 : 0); // Marker set to 1 on last packet
                        int rtp_payload_type = 96;

                        RTPPacketUtil.WriteHeader(rtp_packet, rtp_version, rtp_padding, rtp_extension, rtp_csrc_count, rtp_marker, rtp_payload_type);

                        UInt32 empty_sequence_id = 0;
                        RTPPacketUtil.WriteSequenceNumber(rtp_packet, empty_sequence_id);

                        RTPPacketUtil.WriteTS(rtp_packet, rtp_timestamp);

                        UInt32 empty_ssrc = 0;
                        RTPPacketUtil.WriteSSRC(rtp_packet, empty_ssrc);

                        // Now append the Fragmentation Header (with Start and End marker) and part of the raw_nal
                        byte f_bit = 0;
                        byte nri = (byte)((first_byte >> 5) & 0x03); // Part of the 1st byte of the Raw NAL (NAL Reference ID)
                        byte type = 28; // FU-A Fragmentation

                        rtp_packet[12] = (byte)((f_bit << 7) + (nri << 5) + type);
                        rtp_packet[13] = (byte)((start_bit << 7) + (end_bit << 6) + (0 << 5) + (first_byte & 0x1F));

                        Array.Copy(raw_nal.Array, nal_pointer, rtp_packet, 14, payload_size);
                        nal_pointer = nal_pointer + payload_size;
                        data_remaining = data_remaining - payload_size;

                        rtp_packets.Add(rtp_packet);

                        start_bit = 0;
                    }
                }

                Profiler.EndSample();
            }

            Profiler.BeginSample($"Send {rtp_packets.Count} RTP packets to {rtsp_list.Count} clients.");

            lock (rtsp_list)
            {
                // Go through each RTSP connection and output the NAL on the Video Session
                foreach (RTSPConnection connection in rtsp_list.ToArray()) // ToArray makes a temp copy of the list.
                // This lets us delete items in the foreach
                // eg when there is Write Error
                {
                    // Only process Sessions in Play Mode
                    if (connection.play == false) continue;

                    // There could be more than 1 RTP packet (if the data is fragmented)
                    Boolean write_error = false;
                    foreach (byte[] rtp_packet in rtp_packets)
                    {
                        Profiler.BeginSample("Set RTP packet header fields");

                        // Add the specific data for each transmission
                        RTPPacketUtil.WriteSequenceNumber(rtp_packet, connection.video_sequence_number);
                        connection.video_sequence_number++;

                        // Add the specific SSRC for each transmission
                        RTPPacketUtil.WriteSSRC(rtp_packet, connection.ssrc);
                        Profiler.EndSample();

                        // Send as RTP over RTSP (Interleaved)
                        if (connection.video_transport_reply.LowerTransport == Messages.RtspTransport.LowerTransportType.TCP)
                        {
                            Profiler.BeginSample($"Send TCP packet ({rtp_packet.Length} bytes)");
                            int video_channel = connection.video_transport_reply.Interleaved.First; // second is for RTCP status messages)
                            object state = new object();
                            try
                            {
                                // send the whole NAL. With RTP over RTSP we do not need to Fragment the NAL (as we do with UDP packets or Multicast)
                                //session.listener.BeginSendData(video_channel, rtp_packet, new AsyncCallback(session.listener.EndSendData), state);
                                connection.listener.SendData(video_channel, rtp_packet);
                            }
                            catch
                            {
                                write_error = true;
                                break; // exit out of foreach loop
                            }

                            Profiler.EndSample();
                        }

                        // Send as RTP over UDP
                        if (connection.video_transport_reply.LowerTransport == Messages.RtspTransport.LowerTransportType.UDP && connection.video_transport_reply.IsMulticast == false)
                        {
                            Profiler.BeginSample($"Send UDP packet ({rtp_packet.Length} bytes)");
                            try
                            {
                                // send the whole NAL. ** We could fragment the RTP packet into smaller chuncks that fit within the MTU
                                // Send to the IP address of the Client
                                // Send to the UDP Port the Client gave us in the SETUP command
                                //Debug.Log($"Send {rtp_packet.Length} bytes over UDP.");
                                connection.video_udp_pair.Write_To_Data_Port(rtp_packet, connection.client_hostname, connection.video_client_transport.ClientPort.First);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("UDP Write Exception " + e.ToString());
                                Console.WriteLine("Error writing to listener " + connection.listener.RemoteAdress);
                                write_error = true;
                                break; // exit out of foreach loop
                            }

                            Profiler.EndSample();
                        }

                        // TODO. Add Multicast
                    }

                    if (write_error)
                    {
                        Console.WriteLine("Removing session " + connection.video_session_id + " due to write error");
                        connection.play = false; // stop sending data
                        if (connection.video_udp_pair != null)
                        {
                            connection.video_udp_pair.Stop();
                            connection.video_udp_pair = null;
                        }

                        connection.listener.Dispose();
                        rtsp_list.Remove(connection); // remove the session. It is dead
                    }
                }
            }

            Profiler.EndSample();
        }

        public bool RefreshConnectionList()
        {
            var now = DateTime.UtcNow;
            var current_rtp_play_count = 0;
            var current_rtp_count = 0;
            var timeout_in_seconds = 70; // must have a RTSP message every 70 seconds or we will close the connection

            lock (rtsp_list)
            {
                current_rtp_count = rtsp_list.Count;

                // iterate backwards so we can remove elements while iterating
                for (var i = rtsp_list.Count - 1; i >= 0; i--)
                {
                    var connection = rtsp_list[i];

                    // RTSP Timeout (clients receiving RTP video over the RTSP session
                    // do not need to send a keepalive (so we check for Socket write errors)
                    var sending_rtp_via_tcp = (connection.video_client_transport != null) &&
                        (connection.video_client_transport.LowerTransport == Messages.RtspTransport.LowerTransportType.TCP);

                    if (!sending_rtp_via_tcp && ((now - connection.time_since_last_rtsp_keepalive).TotalSeconds > timeout_in_seconds))
                    {
                        //Debug.Log("Removing session " + connection.video_session_id + " due to TIMEOUT");
                        connection.play = false; // stop sending data
                        if (connection.video_udp_pair != null)
                        {
                            connection.video_udp_pair.Stop();
                            connection.video_udp_pair = null;
                        }

                        connection.listener.Dispose();

                        rtsp_list.RemoveAt(i);
                    }
                    else if (connection.play)
                    {
                        current_rtp_play_count++;
                    }
                }
            }

            //Debug.Log(current_rtp_count + " RTSP clients connected. " + current_rtp_play_count + " RTSP clients in PLAY mode");
            return current_rtp_play_count > 0;
        }

        public class RTSPConnection
        {
            public RtspListener listener = null; // The RTSP client connection
            public bool play = false; // set to true when Session is in Play mode
            public DateTime time_since_last_rtsp_keepalive = DateTime.UtcNow; // Time since last RTSP message received - used to spot dead UDP clients
            public UInt32 ssrc = 0x12345678; // SSRC value used with this client connection
            public String client_hostname = ""; // Client Hostname/IP Address

            public String video_session_id = ""; // RTSP Session ID used with this client connection
            public UInt16 video_sequence_number = 1; // 16 bit RTP packet sequence number used with this client connection
            public Messages.RtspTransport video_client_transport; // Transport: string from the client to the server
            public Messages.RtspTransport video_transport_reply; // Transport: reply from the server to the client
            public UDPSocket video_udp_pair = null; // Pair of UDP sockets (data and control) used when sending via UDP
            public DateTime video_time_since_last_rtcp_keepalive = DateTime.UtcNow; // Time since last RTCP message received - used to spot dead UDP clients

            // TODO - Add Audio
        }
    }
}
