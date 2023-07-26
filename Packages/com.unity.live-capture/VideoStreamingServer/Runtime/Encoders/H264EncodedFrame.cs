using System;
using UnityEngine;

namespace Unity.LiveCapture.VideoStreaming.Server
{
    /// <summary>
    /// Stores a single frame of H264 video.
    /// </summary>
    class H264EncodedFrame
    {
        public ArraySegment<byte> spsNalu;
        public ArraySegment<byte> ppsNalu;
        public ArraySegment<byte> imageNalu;

        /// <summary>
        /// Allocates the buffer so it can contain a number of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to reallocate if required.</param>
        /// <param name="size">The minimum number of bytes the reallocated buffer must contain.</param>
        public void SetSize(ref ArraySegment<byte> buffer, int size)
        {
            var array = buffer.Array;

            if (array == null || array.Length < size)
                array = new byte[Mathf.NextPowerOfTwo(size)];

            buffer = new ArraySegment<byte>(array, 0, size);
        }
    };
}
