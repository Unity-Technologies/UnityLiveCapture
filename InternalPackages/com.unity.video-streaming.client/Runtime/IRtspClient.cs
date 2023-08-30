using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames;
using Unity.LiveCapture.VideoStreaming.Client.Rtsp;

namespace Unity.LiveCapture.VideoStreaming.Client
{
    public interface IRtspClient : IDisposable
    {
        ConnectionParameters ConnectionParameters { get; }

        event EventHandler<RawFrame> FrameReceived;

        /// <summary>
        /// Connect to endpoint and start RTSP session
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="InvalidCredentialException"></exception>
        /// <exception cref="RtspClientException"></exception>
        Task ConnectAsync(CancellationToken token);

        /// <summary>
        /// Receive frames.
        /// Should be called after successful connection to endpoint or <exception cref="InvalidOperationException"></exception> will be thrown
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="RtspClientException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        Task ReceiveAsync(CancellationToken token);
    }
}
