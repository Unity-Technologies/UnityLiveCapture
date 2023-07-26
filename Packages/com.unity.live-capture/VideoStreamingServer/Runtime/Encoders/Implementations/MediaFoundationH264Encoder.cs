#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    struct MediaFoundationH264EncoderPlugin
    {
        [DllImport("H264Encoder", EntryPoint = "Create")]
        extern public static IntPtr CreateEncoder(uint width, uint height, uint frameRateNumerator, uint frameRateDenominator, uint averageBitRate, uint gopSize);

        [DllImport("H264Encoder", EntryPoint = "Destroy")]
        [return : MarshalAs(UnmanagedType.U1)]
        extern public static bool DestroyEncoder(IntPtr encoder);

        [DllImport("H264Encoder", EntryPoint = "Encode")]
        [return : MarshalAs(UnmanagedType.U1)]
        extern public unsafe static bool EncodeFrame(IntPtr encoder, byte* pixelData, ulong timeStampNs);

        [DllImport("H264Encoder", EntryPoint = "BeginConsume")]
        [return : MarshalAs(UnmanagedType.U1)]
        extern public static bool BeginConsumeEncodedBuffer(IntPtr encoder, out uint sizeOut);

        [DllImport("H264Encoder", EntryPoint = "EndConsume")]
        [return : MarshalAs(UnmanagedType.U1)]
        extern public unsafe static bool EndConsumeEncodedBuffer(
            IntPtr encoder,
            byte* dst,
            out ulong timeStampNs,
            [MarshalAs(UnmanagedType.U1)] out bool isKeyFrame);

        [DllImport("H264Encoder", EntryPoint = "GetSps")]
        extern public unsafe static uint GetSpsNAL(IntPtr encoder, byte* spsData);

        [DllImport("H264Encoder", EntryPoint = "GetPps")]
        extern public unsafe static uint GetPpsNAL(IntPtr encoder, byte* ppsData);
    }

    /// <summary>
    /// An encoder that can convert NV12 frames to H264 video.
    /// </summary>
    class MediaFoundationH264Encoder : ISoftwareEncoder
    {
        EncoderSettings m_Settings;
        IntPtr m_Encoder;

        /// <inheritdoc/>
        public EncoderStatus initialized { get; private set; } = EncoderStatus.NotInitialized;

        /// <inheritdoc/>
        public EncoderFormat encoderFormat => EncoderFormat.NV12;

        ~MediaFoundationH264Encoder()
        {
            Dispose();
        }

        /// <summary>
        /// Destroys the native encoder instance.
        /// </summary>
        public void Dispose()
        {
            if (m_Encoder != IntPtr.Zero)
            {
                MediaFoundationH264EncoderPlugin.DestroyEncoder(m_Encoder);
                m_Encoder = IntPtr.Zero;
                initialized = EncoderStatus.NotInitialized;
            }
        }

        /// <inheritdoc/>
        public void Setup(EncoderSettings settings, EncoderFormat format)
        {
        }

        /// <inheritdoc/>
        public void UpdateSettings(in EncoderSettings settings)
        {
            if (m_Settings != settings)
                Dispose();

            if (m_Encoder == IntPtr.Zero)
            {
                m_Settings = settings;
                m_Encoder = MediaFoundationH264EncoderPlugin.CreateEncoder(
                    (uint)settings.width,
                    (uint)settings.height,
                    (uint)settings.frameRate, 1,
                    (uint)settings.bitRate * 1000,
                    (uint)settings.gopSize);

                initialized = m_Encoder != IntPtr.Zero ? EncoderStatus.Initialized : EncoderStatus.Failed;
            }
        }

        /// <inheritdoc/>
        public void Encode(in NativeArray<byte> imageData, ulong timeStamp, H264EncodedFrame frame)
        {
            if (m_Encoder == IntPtr.Zero)
                throw new InvalidOperationException("Encoder is disposed and needs to be setup before encoding a frame.");

            var expectedSize = (m_Settings.width * m_Settings.height * 3) / 2;

            if (imageData.Length != expectedSize)
                throw new ArgumentException($"NV12 image buffer is {imageData.Length} bytes long, but the encoder expects {expectedSize} bytes.", nameof(imageData));

            var success = EncodeFrame(imageData, timeStamp, frame);

            if (!success)
                Debug.LogError($"Error encoding frame at t = {timeStamp / 1000000} ms");
        }

        unsafe bool EncodeFrame(in NativeArray<byte> imageData, ulong timeStamp, H264EncodedFrame frame)
        {
            Profiler.BeginSample("EncodeFrame");
            var success = MediaFoundationH264EncoderPlugin.EncodeFrame(m_Encoder, (byte*)imageData.GetUnsafeReadOnlyPtr(), timeStamp);
            Profiler.EndSample();

            if (!success)
                return false;

            Profiler.BeginSample("BeginConsumeEncodedBuffer");
            success = MediaFoundationH264EncoderPlugin.BeginConsumeEncodedBuffer(m_Encoder, out var bufferSize);
            Profiler.EndSample();

            if (!success)
                return false;

            frame.SetSize(ref frame.imageNalu, (int)bufferSize);
            bool isKeyFrame;

            using (var buffer = new PinnedBufferScope(frame.imageNalu))
            {
                Profiler.BeginSample("EndConsumeEncodedBuffer");
                success = MediaFoundationH264EncoderPlugin.EndConsumeEncodedBuffer(m_Encoder, buffer.pointer, out var bufferTimeStampNs, out isKeyFrame);
                Profiler.EndSample();
            }

            if (!success)
                return false;

            if (isKeyFrame)
            {
                var sz = MediaFoundationH264EncoderPlugin.GetSpsNAL(m_Encoder, (byte*)0);
                frame.SetSize(ref frame.spsNalu, (int)sz);
                using (var buffer = new PinnedBufferScope(frame.spsNalu))
                {
                    MediaFoundationH264EncoderPlugin.GetSpsNAL(m_Encoder, buffer.pointer);
                }

                sz = MediaFoundationH264EncoderPlugin.GetPpsNAL(m_Encoder, (byte*)0);
                frame.SetSize(ref frame.ppsNalu, (int)sz);
                using (var buffer = new PinnedBufferScope(frame.ppsNalu))
                {
                    MediaFoundationH264EncoderPlugin.GetPpsNAL(m_Encoder, buffer.pointer);
                }
            }
            else
            {
                frame.SetSize(ref frame.spsNalu, 0);
                frame.SetSize(ref frame.ppsNalu, 0);
            }

            return true;
        }
    }
}
#endif
