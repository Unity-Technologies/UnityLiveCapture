using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.LiveCapture.VideoStreaming.Client
{
    struct VideoTextureHandles
    {
        // Native (Metal) texture handles
        public IntPtr texture;
        public IntPtr textureCbCr;
    }

    struct H264DecoderPlugin
    {
        const string k_BundleName =
#if UNITY_EDITOR || UNITY_STANDALONE
            "H264DecoderBundle"
#elif UNITY_IOS
            "__Internal"
#endif
        ;

        [DllImport(k_BundleName, EntryPoint = "Create")]
        public static extern IntPtr Create();

        [DllImport(k_BundleName, EntryPoint = "Stop")]
        public static extern int Stop(IntPtr decoderId);

        [DllImport(k_BundleName, EntryPoint = "Destroy")]
        public static extern int Destroy(IntPtr decoderId);

        [DllImport(k_BundleName, EntryPoint = "Configure")]
        public static extern unsafe int Configure(IntPtr decoderId, byte* spsData, int spsDataLength, byte* ppsData, int ppsDataLength);

        [DllImport(k_BundleName, EntryPoint = "GetWidth")]
        public static extern int GetWidth(IntPtr decoderId);

        [DllImport(k_BundleName, EntryPoint = "GetHeight")]
        public static extern int GetHeight(IntPtr decoderId);

        [DllImport(k_BundleName, EntryPoint = "Decode")]
        public static extern unsafe int Decode(IntPtr decoderId, byte* data, int dataLength, long presentationTimeStampTicks, int tickRate, bool doNotOutputFrame);

        [DllImport(k_BundleName, EntryPoint = "IsFrameReady")]
        public static extern bool IsFrameReady(IntPtr decoderId);

        [DllImport(k_BundleName, EntryPoint = "GetVideoTextureHandles")]
        public static extern VideoTextureHandles GetVideoTextureHandles(IntPtr decoderId);
    }

    /// <summary>
    /// Decodes H.264 video to a texture, leveraging hardware acceleration if possible.
    /// </summary>
    public class H264Decoder : IDisposable
    {
        struct DecodeJob : IJob
        {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public IntPtr decoderId;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<byte> data;

            [ReadOnly]
            public long timestampTicks;

            [ReadOnly]
            public int tickRate;

            [ReadOnly]
            public bool doNotOutputFrame;

            public void Execute()
            {
                Profiler.BeginSample("H264Decoder.DecodeJob.Execute");
                Decode(decoderId, data, timestampTicks, tickRate, doNotOutputFrame);
                Profiler.EndSample();
            }
        }

        // Set NALU header length to 4, because from observation, RtspClientSharp seems to produce annexB format
        // 4-byte h.264 nalus with [0, 0, 0, 1] as the header.
        const int k_NalUnitHeaderLength = 4;

        IntPtr m_Decoder;
        JobHandle m_JobHandle;
        byte[] m_LastSpsPps;

        /// <summary>
        /// The texture the decoder outputs to.
        /// </summary>
        public Texture2D texture { get; private set; }

        void Prepare()
        {
            if (m_Decoder == IntPtr.Zero)
            {
                m_Decoder = H264DecoderPlugin.Create();

                if (m_Decoder == IntPtr.Zero)
                    throw new Exception("Error creating decoder");
            }
        }

        /// <summary>
        /// Stops the decoder, cancelling any active decoding jobs.
        /// </summary>
        public void Stop()
        {
            Complete();

            DisposeTexture();

            H264DecoderPlugin.Stop(m_Decoder);

            m_LastSpsPps = null;
        }

        /// <summary>
        /// Releases the resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            Stop();

            if (m_Decoder != IntPtr.Zero)
            {
                H264DecoderPlugin.Destroy(m_Decoder);
                m_Decoder = IntPtr.Zero;
            }
        }

        void DisposeTexture()
        {
            if (texture != null)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(texture);
                else
                    UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Reconfigures the decoder.
        /// </summary>
        /// <param name="spsPpsData">The sequence parameter set data followed by the picture parameter set data.</param>
        /// <param name="spsSize">The length in bytes of the sequence parameter set.</param>
        public void Configure(ArraySegment<byte> spsPpsData, int spsSize)
        {
            Prepare();

            Debug.Assert(m_Decoder != IntPtr.Zero);

            if (spsPpsData.Array == null)
            {
                Debug.LogError("Got null sps/pps array segment!");
                return;
            }

            if (spsPpsData.Count < 2 * k_NalUnitHeaderLength)
            {
                Debug.LogError("Sps/Pps data size smaller than NALUnit header length.");
                return;
            }

            if (m_LastSpsPps != null && m_LastSpsPps.SequenceEqual(spsPpsData))
            {
                //Debug.Log("Redundant sps/pps data, ignoring.");
                return;
            }

            m_LastSpsPps = spsPpsData.ToArray();

            using (var spsPpsDataArray = new NativeArray<byte>(m_LastSpsPps, Allocator.TempJob))
            {
                Complete();
                Configure(m_Decoder, spsPpsDataArray, spsSize, k_NalUnitHeaderLength);
            }
        }

        /// <summary>
        /// Decodes a frame.
        /// </summary>
        /// <param name="data">The frame data nalu.</param>
        /// <param name="ticks">The timestamp of the frame in the video sequence in ticks.</param>
        /// <param name="tickRate">The number of ticks per second.</param>
        public void Decode(ArraySegment<byte> data, long ticks, int tickRate)
        {
            Debug.Assert(m_Decoder != IntPtr.Zero);
            Debug.Assert(data.Array != null);

            var decodeJob = new DecodeJob
            {
                decoderId = m_Decoder,
                data = new NativeArray<byte>(data.Count, Allocator.TempJob),
                timestampTicks = ticks,
                tickRate = tickRate,
                doNotOutputFrame = !m_JobHandle.IsCompleted
            };

            NativeArray<byte>.Copy(data.Array, data.Offset, decodeJob.data, 0, data.Count);

            m_JobHandle = decodeJob.Schedule(m_JobHandle);
            JobHandle.ScheduleBatchedJobs();
        }

        /// <summary>
        /// Updates the texture with the last decoded frame's data, if any is ready.
        /// </summary>
        /// <param name="waitForFrame">Wait until the currently decoding frame (if any) is ready.</param>
        public void ConsumeFrame(bool waitForFrame)
        {
            Profiler.BeginSample($"{nameof(H264Decoder)}.{nameof(ConsumeFrame)}");

            try
            {
                // the decoder is instantiated lazily, so we need to check in case no frames have ever been decoded
                if (m_Decoder == IntPtr.Zero)
                    return;

                if (waitForFrame)
                    Complete();

                if (!H264DecoderPlugin.IsFrameReady(m_Decoder))
                    return;

                var videoTextures = H264DecoderPlugin.GetVideoTextureHandles(m_Decoder);

                var width = H264DecoderPlugin.GetWidth(m_Decoder);
                var height = H264DecoderPlugin.GetHeight(m_Decoder);

                if (texture != null &&
                    (texture.width != width || texture.height != height))
                {
                    Debug.Log($"Resolution changed x: {width} y: {height}");
                    DisposeTexture();
                }

                if (texture == null)
                {
                    texture = Texture2D.CreateExternalTexture(
                        width,
                        height,
                        TextureFormat.BGRA32,
                        false,
                        false,
                        videoTextures.texture);

                    texture.filterMode = FilterMode.Point;
                    texture.wrapMode = TextureWrapMode.Clamp;
                }
                else
                {
                    texture.UpdateExternalTexture(videoTextures.texture);
                }
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        void Complete()
        {
            m_JobHandle.Complete();
            m_JobHandle = default;
        }

        static unsafe int Configure(IntPtr decoder, NativeArray<byte> spsPpsData, int spsSize, int nalUnitHeaderSize)
        {
            Profiler.BeginSample($"{nameof(H264Decoder)}.{nameof(Configure)}");

            var spsData = (byte*)spsPpsData.GetUnsafeReadOnlyPtr() + nalUnitHeaderSize;
            var ppsData = (byte*)spsPpsData.GetUnsafeReadOnlyPtr() + spsSize + nalUnitHeaderSize;

            var ret = H264DecoderPlugin.Configure(
                decoder,
                spsData,
                spsSize - nalUnitHeaderSize,
                ppsData,
                spsPpsData.Length - spsSize - nalUnitHeaderSize);

            Profiler.EndSample();

            return ret;
        }

        static unsafe int Decode(IntPtr decoder, NativeArray<byte> data, long timestampTicks, int tickRate, bool doNotOutputFrame)
        {
            Profiler.BeginSample($"{nameof(H264Decoder)}.{nameof(Decode)}");

            var ret = H264DecoderPlugin.Decode(
                decoder,
                (byte*)data.GetUnsafeReadOnlyPtr(),
                data.Length,
                timestampTicks,
                tickRate,
                doNotOutputFrame);

            Profiler.EndSample();

            return ret;
        }
    }
}
