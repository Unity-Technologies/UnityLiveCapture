using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames.Video;
using Unity.LiveCapture.VideoStreaming.Client.Rtsp;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Client
{
    /// <summary>
    /// An RTSP client that can decode a networked video stream to a texture.
    /// </summary>
    public class VideoStreamingClient : IDisposable
    {
        /// <summary>
        /// Checks if the video streaming client is supported on the current platform.
        /// </summary>
        public static bool IsSupported()
        {
#if UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        H264Decoder m_Decoder = new H264Decoder();

        Task m_ConnectTask;
        CancellationTokenSource m_ConnectionTokenSource;

        /// <summary>
        /// The texture the video is viewed using.
        /// </summary>
        public Texture2D texture => m_Decoder.texture;

        /// <summary>
        /// Prioritize minimizing latency over performance impact.
        /// </summary>
        public bool lowLatencyMode { get; set; } = true;

        ~VideoStreamingClient()
        {
            Dispose();
        }

        /// <summary>
        /// Releases the resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            Disconnect();

            m_Decoder?.Dispose();
            m_Decoder = null;
        }

        /// <summary>
        /// Connects the client to a RTSP server.
        /// </summary>
        /// <param name="url">The url of the RTSP server.</param>
        public void Connect(string url)
        {
            var connectionParameters = new ConnectionParameters(new Uri(url))
            {
                RtpTransport = RtpTransportProtocol.UDP,
                CancelTimeout = TimeSpan.Zero,
                RequiredTracks = RequiredTracks.Video
            };

            m_ConnectionTokenSource = new CancellationTokenSource();
            m_ConnectTask = ConnectAsync(connectionParameters, m_ConnectionTokenSource.Token);
        }

        /// <summary>
        /// Disconnects the client from the video streaming server.
        /// </summary>
        public void Disconnect()
        {
            m_Decoder?.Stop();

            m_ConnectionTokenSource?.Cancel();
            m_ConnectionTokenSource = null;
        }

        /// <summary>
        /// Updates the video texture from the stream.
        /// </summary>
        public void Update()
        {
            m_Decoder.ConsumeFrame(false);
        }

        async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
        {
            var rtspClient = new RtspClient(connectionParameters);
            rtspClient.FrameReceived += HandleFrame;

            try
            {
                while (true)
                {
                    Debug.Log("Connecting...");
                    try
                    {
                        await rtspClient.ConnectAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (RtspClientException e)
                    {
                        Debug.Log("RTSP connection exception: " + e.ToString());
                        continue;
                    }

                    Debug.Log("Connected.");
                    try
                    {
                        await rtspClient.ReceiveAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (RtspClientException e)
                    {
                        Debug.LogError("RTSP client exception: " + e.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Processing cancellation.");
            }
            finally
            {
                rtspClient.FrameReceived -= HandleFrame;
                rtspClient.Dispose();
            }
        }

        void HandleFrame(object sender, RawFrame rawFrame)
        {
            if (m_ConnectionTokenSource.IsCancellationRequested)
                return;

            if (rawFrame.Type != FrameType.Video)
                return;

            var h264Frame = rawFrame as RawH264Frame;
            if (h264Frame == null)
                return;

            if (h264Frame is RawH264IFrame rawH264IFrame && rawH264IFrame.SpsPpsSegment.Array != null)
                m_Decoder.Configure(rawH264IFrame.SpsPpsSegment, rawH264IFrame.SpsSize);

            m_Decoder.Decode(h264Frame.FrameSegment, h264Frame.Timestamp.Ticks, (int)TimeSpan.TicksPerSecond);

            if (lowLatencyMode)
                m_Decoder.ConsumeFrame(true);
        }
    }
}
