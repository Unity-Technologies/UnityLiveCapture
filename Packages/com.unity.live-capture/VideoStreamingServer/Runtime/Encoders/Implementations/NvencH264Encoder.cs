#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    struct NvencH264EncoderPlugin
    {
#if UNITY_EDITOR
        const string k_NvEncLib = "Packages/com.unity.live-capture/VideoStreamingServer/Plugins/NvEncPlugin.dll";
#elif UNITY_STANDALONE
        const string k_NvEncLib = "NvEncPlugin";
#else
        const string k_NvEncLib = "";
#endif

        [DllImport(k_NvEncLib)]
        extern public static IntPtr GetRenderEventFunc();

        [DllImport(k_NvEncLib)]
        extern public static bool EncoderIsInitialized(IntPtr id);

        [DllImport(k_NvEncLib)]
        extern public static int EncoderIsCompatible();

        [DllImport(k_NvEncLib)]
        extern public static bool BeginConsume(IntPtr id);

        [DllImport(k_NvEncLib)]
        extern public static bool EndConsume(IntPtr id);

        [DllImport(k_NvEncLib)]
        extern public unsafe static uint GetSps(IntPtr id, byte* spsData);

        [DllImport(k_NvEncLib)]
        extern public unsafe static uint GetPps(IntPtr id, byte* ppsData);

        [DllImport(k_NvEncLib)]
        extern public unsafe static uint GetEncodedData(IntPtr id, byte* imageData);

        [DllImport(k_NvEncLib)]
        extern public unsafe static ulong GetTimeStamp(IntPtr id);

        [DllImport(k_NvEncLib)]
        extern public unsafe static bool GetIsKeyFrame(IntPtr id);
    }

    /// <summary>
    /// An encoder that can convert RGB or NV12 frames to H264 video.
    /// </summary>
    class NvencH264Encoder : IHardwareEncoder
    {
        /// <summary>
        /// Determines the Nvenc command used in the Low Level Native Plugin.
        /// </summary>
        public enum ENvencRenderEvent
        {
            /// <summary>
            /// Initializes the Nvenc encoder session.
            /// </summary>
            Initialize = 0,

            /// <summary>
            /// Updates the encoder parameters using each <see cref="EncoderSettings"/> frame settings.
            /// </summary>
            Update,

            /// <summary>
            /// Encodes a frame to H264 video compression standard.
            /// </summary>
            Encode,

            /// <summary>
            /// Liberates resources and destroys the encoder session.
            /// </summary>
            Finalize,
        };

        /// <summary>
        /// The data struct sent to the Low Level Native Plugin when calling <see cref="ENvencRenderEvent.Initialize"/>
        /// or <see cref="ENvencRenderEvent.Update"/> events through a CommandBuffer object.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct EncoderSettingsID
        {
            /// <summary>
            /// The configuration options to send to the plugin.
            /// </summary>
            public EncoderSettings settings;

            /// <summary>
            /// The id used to create or retrieve the encoder instance in the plugin.
            /// </summary>
            public int encoderId;

            /// <summary>
            /// Gets the encoder supported format.
            /// </summary>
            public EncoderFormat encoderFormat;
        }

        /// <summary>
        /// The data struct sent to the Low Level Native Plugin when calling <see cref="ENvencRenderEvent.Encode"/>
        /// event through a CommandBuffer object.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct EncoderTextureID
        {
            /// <summary>
            /// The texture pointer to convert by using the encoder.
            /// </summary>
            public IntPtr renderTexture;

            /// <summary>
            /// The id used to retrieve the encoder instance in the plugin.
            /// </summary>
            public int encoderId;

            /// <summary>
            /// The frame time stamp.
            /// </summary>
            public ulong timestamp;
        }

        EncoderSettingsID m_SettingsID;
        EncoderTextureID  m_TextureID;
        static int        m_Counter = 1;
        EncoderStatus     m_EncoderStatus;
        CommandBuffer     m_CommandBuffer;

        /// <inheritdoc/>
        public EncoderFormat encoderFormat => EncoderFormat.R8G8B8;

        /// <inheritdoc/>
        public unsafe EncoderStatus initialized
        {
            get
            {
                if (m_EncoderStatus == EncoderStatus.InProgress)
                {
                    fixed(int* encoderPtr = &m_SettingsID.encoderId)
                    {
                        var status = NvencH264EncoderPlugin.EncoderIsInitialized((IntPtr)encoderPtr);
                        m_EncoderStatus = status ? EncoderStatus.Initialized : EncoderStatus.Failed;
                    }
                }
                return m_EncoderStatus;
            }
        }

        ~NvencH264Encoder()
        {
            Dispose();
        }

        /// <summary>
        /// Queues a command on the render thread to destroy the encoder session and dependent resources.
        /// </summary>
        public unsafe void Dispose()
        {
            if (m_EncoderStatus != EncoderStatus.Initialized)
                return;

            fixed(int* encoderPtr = &m_SettingsID.encoderId)
            {
                ExecuteNvencCommand(ENvencRenderEvent.Finalize, "NVENC Finalize", (IntPtr)encoderPtr);
            }

            DisposeCommandBuffer();
        }

        /// <inheritdoc/>
        public unsafe void Setup(EncoderSettings settings, EncoderFormat encoderFormat)
        {
            m_SettingsID.settings = settings;
            m_SettingsID.encoderId = ++m_Counter;
            m_SettingsID.encoderFormat = encoderFormat;

            DisposeCommandBuffer();
            m_CommandBuffer = new CommandBuffer();

            fixed(EncoderSettingsID* encoderPtr = &m_SettingsID)
            {
                ExecuteNvencCommand(ENvencRenderEvent.Initialize, "NVENC Initialize", (IntPtr)encoderPtr);
                m_EncoderStatus = EncoderStatus.InProgress;
            }
        }

        /// <inheritdoc/>
        public unsafe void UpdateSettings(in EncoderSettings settings)
        {
            if (m_EncoderStatus == EncoderStatus.Failed)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            if (m_SettingsID.settings != settings)
            {
                m_SettingsID.settings = settings;

                fixed(EncoderSettingsID* encoderPtr = &m_SettingsID)
                {
                    ExecuteNvencCommand(ENvencRenderEvent.Update, "NVENC Update", (IntPtr)encoderPtr);
                }
            }
        }

        /// <inheritdoc/>
        public unsafe void Encode(RenderTexture renderTexture, ulong timestamp)
        {
            if (m_EncoderStatus == EncoderStatus.Failed)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            fixed(EncoderTextureID* encoderPtr = &m_TextureID)
            {
                m_TextureID.encoderId = m_SettingsID.encoderId;
                m_TextureID.renderTexture = renderTexture.GetNativeTexturePtr();
                m_TextureID.timestamp = timestamp;

                ExecuteNvencCommand(ENvencRenderEvent.Encode, "NVENC Encode", (IntPtr)encoderPtr);
            }
        }

        /// <inheritdoc/>
        public unsafe bool ConsumeData(H264EncodedFrame frame, out ulong timestamp)
        {
            if (m_EncoderStatus == EncoderStatus.Failed)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            fixed(int* encoderPtr = &m_SettingsID.encoderId)
            {
                timestamp = 0;

                var existingFrame = NvencH264EncoderPlugin.BeginConsume((IntPtr)encoderPtr);
                if (existingFrame)
                {
                    var isKeyFrame = NvencH264EncoderPlugin.GetIsKeyFrame((IntPtr)encoderPtr);
                    var imageSize = NvencH264EncoderPlugin.GetEncodedData((IntPtr)encoderPtr, null);

                    // Allocation of Encoded Image & Retrieve data.
                    frame.SetSize(ref frame.imageNalu, (int)imageSize);
                    using (var buffer = new PinnedBufferScope(frame.imageNalu))
                    {
                        NvencH264EncoderPlugin.GetEncodedData((IntPtr)encoderPtr, buffer.pointer);
                    }

                    // Retrieve the timestamp.
                    timestamp = NvencH264EncoderPlugin.GetTimeStamp((IntPtr)encoderPtr);

                    if (isKeyFrame)
                    {
                        // Getting buffer size for pre-allocation
                        var spsSize = NvencH264EncoderPlugin.GetSps((IntPtr)encoderPtr, null);
                        var ppsSize = NvencH264EncoderPlugin.GetPps((IntPtr)encoderPtr, null);

                        // Allocation of SPS & Retrieve data.
                        frame.SetSize(ref frame.spsNalu, (int)spsSize);
                        using (var buffer = new PinnedBufferScope(frame.spsNalu))
                        {
                            NvencH264EncoderPlugin.GetSps((IntPtr)encoderPtr, buffer.pointer);
                        }

                        // Allocation of PPS & Retrieve data.
                        frame.SetSize(ref frame.ppsNalu, (int)ppsSize);
                        using (var buffer = new PinnedBufferScope(frame.ppsNalu))
                        {
                            NvencH264EncoderPlugin.GetPps((IntPtr)encoderPtr, buffer.pointer);
                        }
                    }

                    // Liberate the current encoded frame in the Plugin.
                    NvencH264EncoderPlugin.EndConsume((IntPtr)encoderPtr);
                }
                return existingFrame;
            }
        }

        /// <summary>
        /// Queues an Nvenc command on the render thread.
        /// </summary>
        /// <param name="id">The Nvenc command to execute.</param>
        /// <param name="commandName">The name of the command to execute.</param>
        /// <param name="data">The data associated to the command.</param>
        public void ExecuteNvencCommand(ENvencRenderEvent id, string commandName, IntPtr data)
        {
            m_CommandBuffer.Clear();
            m_CommandBuffer.name = commandName;

            switch (SystemInfo.graphicsDeviceType)
            {
                case GraphicsDeviceType.Direct3D11:
                case GraphicsDeviceType.Direct3D12:
                    m_CommandBuffer.IssuePluginEventAndData(NvencH264EncoderPlugin.GetRenderEventFunc(), (int)id, data);
                    break;
            }

            Graphics.ExecuteCommandBuffer(m_CommandBuffer);
        }

        void DisposeCommandBuffer()
        {
            if (m_CommandBuffer != null)
            {
                m_CommandBuffer.Release();
                m_CommandBuffer = null;
            }
        }
    }
}
#endif
