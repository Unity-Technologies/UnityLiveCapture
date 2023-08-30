using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    /// <summary>
    /// The configuration options for an <see cref="IEncoder"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct EncoderSettings : IEquatable<EncoderSettings>
    {
        /// <summary>
        /// The width of the output video, in pixels.
        /// </summary>
        public int width;

        /// <summary>
        /// The height in pixels of the output video.
        /// </summary>
        public int height;

        /// <summary>
        /// The frame rate in Hz of the output video.
        /// </summary>
        public int frameRate;

        /// <summary>
        /// The target bit rate of the output video in kilobits per second. This controls the video quality.
        /// </summary>
        public int bitRate;

        /// <summary>
        /// The number of frames in each group of pictures, which consists of one keyframe (I-frame) followed
        /// by delta frames (P-frames and B-frames). With larger values, you get a higher quality for a given bit rate,
        /// but the stream takes longer to recover after a dropped frame.
        /// </summary>
        public int gopSize;

        public bool Equals(EncoderSettings other)
        {
            return
                width == other.width &&
                height == other.height &&
                frameRate == other.frameRate &&
                bitRate == other.bitRate &&
                gopSize == other.gopSize;
        }

        public override bool Equals(object obj)
        {
            return obj is EncoderSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = width;
                hashCode = (hashCode * 397) ^ height;
                hashCode = (hashCode * 397) ^ frameRate;
                hashCode = (hashCode * 397) ^ bitRate;
                hashCode = (hashCode * 397) ^ gopSize;
                return hashCode;
            }
        }

        public static bool operator ==(EncoderSettings a, EncoderSettings b) => a.Equals(b);
        public static bool operator !=(EncoderSettings a, EncoderSettings b) => !a.Equals(b);
    }
}
