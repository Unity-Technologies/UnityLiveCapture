using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A thread-safe pool of byte buffers. Optimized to handle large buffers with greatly varying sizes.
    /// </summary>
    class BufferPool
    {
        readonly ConcurrentBag<byte[]>[] m_Pools;
        readonly int m_MinBufferSize;
        readonly int m_IndexOffset;

        /// <summary>
        /// Creates a new <see cref="BufferPool"/> instance.
        /// </summary>
        /// <param name="minBufferSize">The minimum capacity of buffer to pool. Will be rounded up to the next of 2.</param>
        public BufferPool(int minBufferSize = 1024)
        {
            m_MinBufferSize = minBufferSize < 1 ? 1 : Mathf.NextPowerOfTwo(minBufferSize);
            m_IndexOffset = Log2(m_MinBufferSize);

            // we need one pool of buffers for each power of two up to the max buffer size
            m_Pools = new ConcurrentBag<byte[]>[32 - m_IndexOffset];

            for (var i = 0; i < m_Pools.Length; i++)
                m_Pools[i] = new ConcurrentBag<byte[]>();
        }

        /// <summary>
        /// Clears all buffers currently in the pool.
        /// </summary>
        public void Clear()
        {
            foreach (var pool in m_Pools)
                while (!pool.IsEmpty)
                    pool.TryTake(out _);
        }

        /// <summary>
        /// Gets a buffer from the pool. When done, return the buffer to the
        /// pool using <see cref="Release"/>.
        /// </summary>
        /// <param name="capacity">The required buffer size in bytes.</param>
        /// <returns>A buffer at least <paramref name="capacity"/> bytes in length
        /// but no more than twice the required capacity in length.</returns>
        public byte[] Get(int capacity)
        {
            capacity = Math.Max(capacity, m_MinBufferSize);

            var size = Mathf.NextPowerOfTwo(capacity);
            var index = Log2(size) - m_IndexOffset;

            if (!m_Pools[index].TryTake(out var buffer))
                buffer = new byte[size];

            return buffer;
        }

        /// <summary>
        /// Returns a buffer to the pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        public void Release(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < m_MinBufferSize || !Mathf.IsPowerOfTwo(buffer.Length))
                throw new ArgumentException($"Length ({buffer.Length}) is not valid for this pool.", nameof(buffer));

            m_Pools[Log2(Mathf.NextPowerOfTwo(buffer.Length)) - m_IndexOffset].Add(buffer);
        }

        static int Log2(int value)
        {
            var result = 0;
            while ((value >>= 1) > 0)
                result++;
            return result;
        }
    }
}
