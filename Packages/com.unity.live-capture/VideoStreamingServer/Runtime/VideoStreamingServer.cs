using System;
using System.Collections.Concurrent;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    enum VideoEncoder
    {
        /// <summary>
        /// No supported or compatible encoders are available on the machine.
        /// </summary>
        [InspectorName("No Encoder")]
        NoEncoder = 0,

        /// <summary>
        /// Media Foundation encoder, using H264 video compression standard.
        /// </summary>
        [InspectorName("Windows Media Foundation H.264")]
        MediaFoundationH264 = 10,

        /// <summary>
        /// NVIDIA Nvenc encoder, using H264 video compression standard.
        /// </summary>
        [InspectorName("NVENC H.264")]
        NvencH264 = 20,

        /// <summary>
        /// Apple Video Toolbox encoder, using H264 video compression standard.
        /// </summary>
        [InspectorName("Video Toolbox H.264")]
        VideoToolboxH264 = 30,
    }

    /// <summary>
    /// A server that takes images and encodes them to a video stream transported to clients via RTSP.
    /// </summary>
    class VideoStreamingServer : IDisposable
    {
        /// <summary>
        /// A lock that ensures access occurs in a FIFO manner.
        /// </summary>
        class QueuedLock
        {
            readonly object m_Lock = new object();
            volatile int queueNumber = 0;
            volatile int nextNumber = 1;

            public void Enter()
            {
                var number = Interlocked.Increment(ref queueNumber);
                Monitor.Enter(m_Lock);

                while (true)
                {
                    if (number == nextNumber)
                    {
                        return;
                    }

                    Monitor.Wait(m_Lock);
                }
            }

            public void Exit()
            {
                Interlocked.Increment(ref nextNumber);
                Monitor.PulseAll(m_Lock);
                Monitor.Exit(m_Lock);
            }
        }

        /// <summary>
        /// Checks if the video streaming server is supported on the current platform.
        /// </summary>
        /// <returns>True if supported; false otherwise.</returns>
        public static bool IsSupported()
        {
            return EncoderUtilities.FindFirstSupportedEncoder() != VideoEncoder.NoEncoder;
        }

        /// <summary>
        /// The maximum number of frames which can be waiting to be encoded on the
        /// encoder thread. Larger vales prioritize stream continuity over latency
        /// when encoding becomes a bottleneck.
        /// </summary>
        const int k_MaxBufferedFrameCount = 3;

        // we want a low gop size to recover quickly if packets are dropped
        const int k_GopSize = 2;

        struct BufferedFrame
        {
            public EncoderSettings settings;
            public EncoderFormat encoderFormat;
            public NativeArray<byte> data;
            public ulong timestamp;
        }

        VideoEncoder m_RequestedEncoder = VideoEncoder.NoEncoder;
        VideoEncoder m_ActiveEncoder = VideoEncoder.NoEncoder;
        IEncoder m_Encoder = null;
        readonly QueuedLock m_EncoderLock = new QueuedLock();
        bool m_Disposed;

        Thread m_Thread;
        RtspServer m_Server;
        BlockingCollection<BufferedFrame> m_BufferedFrames;

        /// <summary>
        /// Gets if the server is currently running.
        /// </summary>
        public bool isRunning { get; private set; }

        /// <summary>
        /// The port the server is running on.
        /// </summary>
        public int port => isRunning ? m_Server.port : 0;

        /// <summary>
        /// Does the active encoder read the input texture from the GPU instead of the CPU.
        /// </summary>
        public bool usesDirectAccess => m_Encoder is IHardwareEncoder;

        /// <summary>
        /// The texture format the encoder uses for input.
        /// </summary>
        public EncoderFormat frameFormat => m_Encoder?.encoderFormat ?? default;

        /// <summary>
        /// The encoder that the user requests.
        /// </summary>
        public VideoEncoder requestedEncoder
        {
            get => m_RequestedEncoder;
            set
            {
                if (m_RequestedEncoder != value)
                {
                    m_RequestedEncoder = value;
                    activeEncoder = value;
                }
            }
        }

        /// <summary>
        /// The encoder that the system is using.
        /// </summary>
        public VideoEncoder activeEncoder
        {
            get => m_ActiveEncoder;
            private set
            {
                var encoderToUse = EncoderUtilities.IsSupported(value) == EncoderSupport.Supported
                    ? value
                    : EncoderUtilities.FindFirstSupportedEncoder();

                if (m_ActiveEncoder != encoderToUse)
                {
                    m_ActiveEncoder = encoderToUse;

                    try
                    {
                        m_EncoderLock.Enter();

                        m_Encoder?.Dispose();
                        m_Encoder = EncoderUtilities.InitializeEncoder(encoderToUse);
                    }
                    finally
                    {
                        m_EncoderLock.Exit();
                    }

                    DisposeFramesQueue();
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="VideoStreamingServer"/> instance.
        /// </summary>
        public VideoStreamingServer()
        {
            if (!IsSupported())
                throw new PlatformNotSupportedException($"{nameof(VideoStreamingServer)} is not supported on the current platform!");
        }

        /// <summary>
        /// The finalizer which disposes the server if <see cref="Dispose"/> is not called on the instance.
        /// </summary>
        ~VideoStreamingServer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Starts the video server.
        /// </summary>
        /// <param name="port">The TCP port to listen for connecting clients on. If 0 is provided, the server lets
        /// the OS pick a free port.</param>
        public void Start(int port = 0)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(VideoStreamingServer));

            Debug.Assert(!isRunning);

            m_BufferedFrames = new BlockingCollection<BufferedFrame>();

            try
            {
                m_Server = new RtspServer(port, null, null);
                m_Server.StartListen();

                isRunning = true;

                m_Thread = new Thread(ServerLoop);
                m_Thread.Start();
            }
            catch (Exception)
            {
                DisposeServer();
                throw;
            }
        }

        /// <summary>
        /// Shuts down the video server.
        /// </summary>
        public void Stop()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(VideoStreamingServer));

            DisposeServer();
        }

        /// <summary>
        /// Releases the resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            DisposeServer();

            try
            {
                m_EncoderLock.Enter();

                m_Encoder?.Dispose();
            }
            finally
            {
                m_EncoderLock.Exit();
            }

            m_Disposed = true;
        }

        void DisposeServer()
        {
            isRunning = false;
            m_BufferedFrames.CompleteAdding();

            m_Thread?.Join();
            m_Thread = null;

            m_Server?.Dispose();
            m_Server = null;

            DisposeFramesQueue();
        }

        void DisposeFramesQueue()
        {
            while (m_BufferedFrames != null && m_BufferedFrames.TryTake(out var frame))
            {
                frame.data.Dispose();
            }
        }

        /// <summary>
        /// Enqueue a frame for encoding into the stream.
        /// </summary>
        /// <param name="frame">The frame to encode.</param>
        /// <param name="frameRate">The frame rate in Hz of the video stream.</param>
        /// <param name="bitRate">The target bit rate of the video stream in kilobits per second.</param>
        public void EnqueueFrame(AsyncGPUVideoFrameRequest frame, int frameRate, int bitRate)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(VideoStreamingServer));
            if (!isRunning)
                return;

            if (m_Encoder is ISoftwareEncoder)
            {
                m_BufferedFrames.Add(new BufferedFrame
                {
                    settings = new EncoderSettings
                    {
                        width = frame.width,
                        height = frame.height,
                        frameRate = frameRate,
                        bitRate = bitRate,
                        gopSize = k_GopSize,
                    },
                    encoderFormat = frame.format,
                    // We need to copy the frame data, since the request data could be cleared if the frame ends
                    // before the encoder finishes using the data.
                    data = new NativeArray<byte>(frame.GetData(), Allocator.Persistent),
                    timestamp = (ulong)(frame.elapsedTime * 1000000000),
                });

                // This is required so that if the encoding is slower than the rate at which
                // frames are generated the queue does not grow infinitely and the stream become
                // increasingly delayed.
                while (m_BufferedFrames.Count >= k_MaxBufferedFrameCount && m_BufferedFrames.TryTake(out var f))
                {
                    f.data.Dispose();
                }
            }
        }

        /// <summary>
        /// Enqueue a frame for encoding into the stream.
        /// </summary>
        /// <param name="frame">The frame to encode.</param>
        /// <param name="frameRate">The frame rate in Hz of the video stream.</param>
        /// <param name="bitRate">The target bit rate of the video stream in kilobits per second.</param>
        public void EnqueueFrame(DirectAccessVideoFrameRequest frame, int frameRate, int bitRate)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(VideoStreamingServer));
            if (!isRunning)
                return;

            var settings = new EncoderSettings
            {
                width = frame.width,
                height = frame.height,
                frameRate = frameRate,
                bitRate = bitRate,
                gopSize = k_GopSize,
            };
            var texture = frame.renderTexture;
            var timestamp = (ulong)(frame.elapsedTime * 1000000000);

            try
            {
                m_EncoderLock.Enter();

                if (m_Encoder is IHardwareEncoder encoder)
                {
                    Profiler.BeginSample($"Encode Frame");

                    switch (encoder.initialized)
                    {
                        case EncoderStatus.NotInitialized:
                            encoder.Setup(settings, frame.format);
                            break;
                        case EncoderStatus.Failed:
                            throw new InvalidOperationException("Encoder failed during initialization.");
                    }

                    encoder.UpdateSettings(settings);
                    encoder.Encode(texture, timestamp);

                    Profiler.EndSample();
                }
            }
            finally
            {
                m_EncoderLock.Exit();
            }

            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
            }
        }

        void ServerLoop()
        {
            Profiler.BeginThreadProfiling("Video Streaming Servers", $"Server Port: {port}");

            try
            {
                var encodedFrame = new H264EncodedFrame();

                while (isRunning)
                {
                    if (m_Server.RefreshConnectionList())
                    {
                        try
                        {
                            m_EncoderLock.Enter();

                            switch (m_Encoder)
                            {
                                case ISoftwareEncoder softwareEncoder:
                                    ProcessSoftwareEncoderFrames(softwareEncoder, encodedFrame);
                                    break;
                                case IHardwareEncoder hardwareEncoder:
                                    ProcessHardwareEncoderFrames(hardwareEncoder, encodedFrame);
                                    break;
                            }
                        }
                        finally
                        {
                            m_EncoderLock.Exit();
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Thrown then the thead is stopped
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Profiler.EndThreadProfiling();
            }
        }

        void ProcessSoftwareEncoderFrames(ISoftwareEncoder softwareEncoder, H264EncodedFrame encodedFrame)
        {
            while (!m_BufferedFrames.IsCompleted && m_BufferedFrames.TryTake(out var frame))
            {
                Profiler.BeginSample($"Encode Frame");

                switch (softwareEncoder.initialized)
                {
                    case EncoderStatus.NotInitialized:
                        softwareEncoder.Setup(frame.settings, frame.encoderFormat);
                        break;
                    case EncoderStatus.Failed:
                        throw new InvalidOperationException("Encoder failed during initialization.");
                }

                softwareEncoder.UpdateSettings(frame.settings);
                softwareEncoder.Encode(frame.data, frame.timestamp, encodedFrame);

                Profiler.EndSample();
                Profiler.BeginSample($"Send NALUs");

                m_Server.SendNALUs(
                    frame.timestamp,
                    encodedFrame.spsNalu,
                    encodedFrame.ppsNalu,
                    encodedFrame.imageNalu
                );

                frame.data.Dispose();

                Profiler.EndSample();
            }
        }

        void ProcessHardwareEncoderFrames(IHardwareEncoder hardwareEncoder, H264EncodedFrame encodedFrame)
        {
            while (hardwareEncoder.ConsumeData(encodedFrame, out var timestamp))
            {
                Profiler.BeginSample($"Send NALUs");

                m_Server.SendNALUs(
                    timestamp,
                    encodedFrame.spsNalu,
                    encodedFrame.ppsNalu,
                    encodedFrame.imageNalu
                );

                Profiler.EndSample();
            }
        }
    }
}
