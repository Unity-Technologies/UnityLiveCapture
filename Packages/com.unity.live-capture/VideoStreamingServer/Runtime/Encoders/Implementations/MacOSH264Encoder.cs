#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    struct MacOSH264EncoderPlugin
    {
        internal const string MacOSLib = "MacOSEncoderBundle";

        [DllImport(MacOSLib)]
        extern public static IntPtr GetRenderEventFunc();

        [DllImport(MacOSLib)]
        extern public static bool EncoderIsInitialized(IntPtr id);

        [DllImport(MacOSLib)]
        extern public static int EncoderIsCompatible();

        [DllImport(MacOSLib)]
        extern public static bool BeginConsume(IntPtr encoder);

        [DllImport(MacOSLib)]
        extern public static bool EndConsume(IntPtr encoder);

        [DllImport(MacOSLib)]
        extern public unsafe static uint GetSps(IntPtr encoder, byte* spsData);

        [DllImport(MacOSLib)]
        extern public unsafe static uint GetPps(IntPtr encoder, byte* ppsData);

        [DllImport(MacOSLib)]
        extern public unsafe static uint GetEncodedData(IntPtr encoder, byte* imageData);

        [DllImport(MacOSLib)]
        extern public unsafe static ulong GetTimeStamp(IntPtr encoder);

        [DllImport(MacOSLib)]
        extern public unsafe static bool GetIsKeyFrame(IntPtr encoder);
    }

    /// <summary>
    /// An encoder that can convert RGB or NV12 frames to H264 video.
    /// </summary>
    class MacOSH264Encoder : IHardwareEncoder
    {
        static int m_Counter = 1;

        /// <summary>
        /// Determines the Mac OS command used in the Low Level Native Plugin.
        /// </summary>
        public enum EMacOSRenderEvent
        {
            /// <summary>
            /// Initializes the Mac OS encoder session.
            /// </summary>
            Initialize = 0,

            /// <summary>
            /// Updates the encoder parameters using each <see cref="H264EncoderSettings"/> frame settings.
            /// </summary>
            Update,

            /// <summary>
            /// Encodes a frame to H264 video compression standard.
            /// </summary>
            Encode,

            /// <summary>
            /// Liberates resources and destroys the encoder session.
            /// </summary>
            Finalize
        };

        /// <summary>
        /// The data struct sent to the Low Level Native Plugin when calling <see cref="EMacOSRenderEvent.Initialize"/>
        /// or <see cref="EMacOSRenderEvent.Update"/> events through a CommandBuffer object.
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

            /// <summary>
            /// Should the encoder expect SRGB textures.
            /// </summary>
            public bool useSRGB;
        }

        /// <summary>
        /// The data struct sent to the Low Level Native Plugin when calling <see cref="EMacOSRenderEvent.Encode"/>
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
        EncoderStatus     m_EncoderStatus;
        int               m_FinalizeID;
        CommandBuffer     m_CommandBuffer;

        /// <inheritdoc/>
        public EncoderFormat encoderFormat => EncoderFormat.R8G8B8;

        /// <inheritdoc/>
        unsafe public EncoderStatus initialized
        {
            get
            {
                if (m_EncoderStatus == EncoderStatus.InProgress)
                {
                    fixed(int* encoderPtr = &m_SettingsID.encoderId)
                    {
                        var status = MacOSH264EncoderPlugin.EncoderIsInitialized((IntPtr)encoderPtr);
                        m_EncoderStatus = (status) ? EncoderStatus.Initialized : EncoderStatus.Failed;
                    }
                }
                return m_EncoderStatus;
            }
        }

        ~MacOSH264Encoder()
        {
            Dispose();
        }

        /// <summary>
        /// Destroy the encoder session and dependent resources.
        /// </summary>
        unsafe public void Dispose()
        {
            if (m_EncoderStatus != EncoderStatus.Initialized)
                return;

            m_FinalizeID = m_SettingsID.encoderId;

            fixed(int* id = &m_FinalizeID)
            {
                ExecuteMacOSCommand(EMacOSRenderEvent.Finalize, "Mac OS Encoder Finalize", (IntPtr)id);
            }

            DisposeCommandBuffer();
        }

        /// <summary>
        /// Queues a command on the render thread to initialize the encoder session.
        /// </summary>
        /// <param name="settings">The H264 settings used to create the encoder.</param>
        unsafe public void Setup(EncoderSettings settings, EncoderFormat encoderFormat)
        {
            m_SettingsID.settings = settings;
            m_SettingsID.encoderId = m_Counter++;
            m_SettingsID.encoderFormat = encoderFormat;
            m_SettingsID.useSRGB = QualitySettings.activeColorSpace != ColorSpace.Gamma;

            if (m_CommandBuffer != null)
                DisposeCommandBuffer();

            m_CommandBuffer = new CommandBuffer();

            fixed(EncoderSettingsID* encoderPtr = &m_SettingsID)
            {
                ExecuteMacOSCommand(EMacOSRenderEvent.Initialize, "Mac OS Encoder Initialize", (IntPtr)encoderPtr);
                m_EncoderStatus = EncoderStatus.InProgress;
            }
        }

        /// <summary>
        /// Queues a command on the render thread to update the Mac OS encoder parameters.
        /// </summary>
        /// <param name="settings">The H264 settings used to update the encoder.</param>
        unsafe public void UpdateSettings(in EncoderSettings settings)
        {
            if (m_EncoderStatus == EncoderStatus.Failed)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            if (m_SettingsID.settings != settings)
            {
                Dispose();
                Setup(settings, m_SettingsID.encoderFormat);
            }
        }

        /// <inheritdoc/>
        unsafe public void Encode(RenderTexture renderTexture, ulong timestamp)
        {
            if (m_EncoderStatus == EncoderStatus.Failed)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            fixed(EncoderTextureID* encoderPtr = &m_TextureID)
            {
                m_TextureID.encoderId = m_SettingsID.encoderId;
                m_TextureID.renderTexture = renderTexture.GetNativeTexturePtr();
                m_TextureID.timestamp = timestamp;

                ExecuteMacOSCommand(EMacOSRenderEvent.Encode, "Mac OS Encoder Encode", (IntPtr)encoderPtr);
            }
        }

        /// <inheritdoc/>
        unsafe public bool ConsumeData(H264EncodedFrame frame, out ulong timestamp)
        {
            if (m_EncoderStatus == EncoderStatus.Failed)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            fixed(int* encoderPtr = &m_SettingsID.encoderId)
            {
                timestamp = 0;

                var existingFrame = MacOSH264EncoderPlugin.BeginConsume((IntPtr)encoderPtr);
                if (existingFrame)
                {
                    var isKeyFrame = MacOSH264EncoderPlugin.GetIsKeyFrame((IntPtr)encoderPtr);
                    var imageSize = MacOSH264EncoderPlugin.GetEncodedData((IntPtr)encoderPtr, null);

                    // Allocation of Encoded Image & Retrieve data.
                    frame.SetSize(ref frame.imageNalu, (int)imageSize);
                    using (var buffer = new PinnedBufferScope(frame.imageNalu))
                    {
                        MacOSH264EncoderPlugin.GetEncodedData((IntPtr)encoderPtr, buffer.pointer);
                    }

                    // Retrieve the timestamp.
                    timestamp = MacOSH264EncoderPlugin.GetTimeStamp((IntPtr)encoderPtr);

                    if (isKeyFrame)
                    {
                        // Getting buffer size for pre-allocation
                        var spsSize = MacOSH264EncoderPlugin.GetSps((IntPtr)encoderPtr, null);
                        var ppsSize = MacOSH264EncoderPlugin.GetPps((IntPtr)encoderPtr, null);


                        // Allocation of SPS & Retrieve data.
                        frame.SetSize(ref frame.spsNalu, (int)spsSize);
                        using (var buffer = new PinnedBufferScope(frame.spsNalu))
                        {
                            MacOSH264EncoderPlugin.GetSps((IntPtr)encoderPtr, buffer.pointer);
                        }

                        // Allocation of PPS & Retrieve data.
                        frame.SetSize(ref frame.ppsNalu, (int)ppsSize);
                        using (var buffer = new PinnedBufferScope(frame.ppsNalu))
                        {
                            MacOSH264EncoderPlugin.GetPps((IntPtr)encoderPtr, buffer.pointer);
                        }
                    }

                    // Liberate the current encoded frame in the Plugin.
                    MacOSH264EncoderPlugin.EndConsume((IntPtr)encoderPtr);
                }
                return existingFrame;
            }
        }

        /// <summary>
        /// Queues a Mac OS command on the render thread.
        /// </summary>
        /// <param name="id"> The Mac OS command to execute.</param>
        /// <param name="data"> The data associated to the command.</param>
        public void ExecuteMacOSCommand(EMacOSRenderEvent id, string commandName, IntPtr data)
        {
            m_CommandBuffer.Clear();
            m_CommandBuffer.name = commandName;

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
            {
                m_CommandBuffer.IssuePluginEventAndData(MacOSH264EncoderPlugin.GetRenderEventFunc(), (int)id, data);
            }
            else
                throw new InvalidOperationException("Graphics device is not supported.");

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
