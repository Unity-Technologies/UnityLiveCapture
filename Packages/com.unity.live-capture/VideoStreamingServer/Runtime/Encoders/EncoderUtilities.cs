using System;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    static class EncoderUtilities
    {
        static readonly VideoEncoder[] s_EncoderValues = Enum.GetValues(typeof(VideoEncoder)).Cast<VideoEncoder>().ToArray();

        /// <summary>
        /// Determines if the specified encoder is compatible on the machine.
        /// </summary>
        /// <param name="encoder">The specified encoder.</param>
        /// <returns>True if supported; false otherwise.</returns>
        public static EncoderSupport IsSupported(VideoEncoder encoder)
        {
            switch (encoder)
            {
                case VideoEncoder.NoEncoder:
                    return EncoderSupport.Supported;
                case VideoEncoder.MediaFoundationH264:
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    return EncoderSupport.Supported;
#else
                    return EncoderSupport.NotSupportedOnPlatform;
#endif
                case VideoEncoder.NvencH264:
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    return (EncoderSupport)NvencH264EncoderPlugin.EncoderIsCompatible();
#else
                    return EncoderSupport.NotSupportedOnPlatform;
#endif
                case VideoEncoder.VideoToolboxH264:
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    return EncoderSupport.Supported;
#else
                    return EncoderSupport.NotSupportedOnPlatform;
#endif
                default:
                    return EncoderSupport.NotSupportedOnPlatform;
            }
        }

        /// <summary>
        /// Finds the first supported encoder on the machine.
        /// </summary>
        /// <returns>The first supported encoder, or <see cref="VideoEncoder.NoEncoder"/> if none are available.</returns>
        public static VideoEncoder FindFirstSupportedEncoder()
        {
            foreach (var encoder in s_EncoderValues)
            {
                if (encoder != VideoEncoder.NoEncoder && IsSupported(encoder) == EncoderSupport.Supported)
                {
                    return encoder;
                }
            }
            return VideoEncoder.NoEncoder;
        }

        /// <summary>
        /// Finds the default supported encoder on the machine.
        /// </summary>
        /// <returns>The default supported encoder.</returns>
        public static VideoEncoder DefaultSupportedEncoder()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return VideoEncoder.MediaFoundationH264;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return VideoEncoder.VideoToolboxH264;
#else
            return FindFirstSupportedEncoder();
#endif
        }

        /// <summary>
        /// Creates an instance of the specified encoder.
        /// </summary>
        /// <param name="encoder">The encoder to create.</param>
        /// <returns>
        /// The created instance of the specified encoder, or null if the specified encoder is invalid.
        /// </returns>
        public static IEncoder InitializeEncoder(VideoEncoder encoder)
        {
            if (IsSupported(encoder) != EncoderSupport.Supported)
            {
                return null;
            }

            switch (encoder)
            {
                case VideoEncoder.MediaFoundationH264:
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    return new MediaFoundationH264Encoder();
#else
                    return null;
#endif
                case VideoEncoder.NvencH264:
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    return new NvencH264Encoder();
#else
                    return null;
#endif
                case VideoEncoder.VideoToolboxH264:
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    return new MacOSH264Encoder();
#else
                    return null;
#endif
                default:
                    return null;
            }
        }
    }
}
