// ---------------------------------------------------------------------
// Copyright (c) 2015-2016 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Modified by Unity Technologies 2020
// ---------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.IO
{
    /// <summary>
    /// Manages pools of RecyclableMemoryStream objects.
    /// </summary>
    /// <remarks>
    /// There are two pools managed in here. The small pool contains same-sized buffers that are handed to streams
    /// as they write more data.
    ///
    /// For scenarios that need to call GetBuffer(), the large pool contains buffers of various sizes, all
    /// multiples/exponentials of LargeBufferMultiple (1 MB by default). They are split by size to avoid overly-wasteful buffer
    /// usage. There should be far fewer 8 MB buffers than 1 MB buffers, for example.
    /// </remarks>
    sealed class RecyclableMemoryStreamManager
    {
        public const int DefaultBlockSize = 128 * 1024;
        public const int DefaultLargeBufferMultiple = 1024 * 1024;
        public const int DefaultMaximumBufferSize = 128 * 1024 * 1024;

        readonly ConcurrentBag<RecyclableMemoryStream> m_Pool = new ConcurrentBag<RecyclableMemoryStream>();

        /// <summary>
        /// pools[0] = 1x largeBufferMultiple buffers
        /// pools[1] = 2x largeBufferMultiple buffers
        /// pools[2] = 3x(multiple)/4x(exponential) largeBufferMultiple buffers
        /// etc., up to maximumBufferSize
        /// </summary>
        readonly ConcurrentStack<byte[]>[] m_LargePools;
        readonly long[] m_LargeBufferFreeSize;
        readonly long[] m_LargeBufferInUseSize;

        readonly ConcurrentStack<byte[]> m_SmallPool;
        long m_SmallPoolFreeSize;
        long m_SmallPoolInUseSize;

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications.
        /// </summary>
        public RecyclableMemoryStreamManager()
            : this(DefaultBlockSize, DefaultLargeBufferMultiple, DefaultMaximumBufferSize, false) { }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="ArgumentOutOfRangeException">blockSize is not a positive number, or largeBufferMultiple is not a positive number, or maximumBufferSize is less than blockSize.</exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple of largeBufferMultiple</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, false) { }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy</param>
        /// <exception cref="ArgumentOutOfRangeException">blockSize is not a positive number, or largeBufferMultiple is not a positive number, or maximumBufferSize is less than blockSize.</exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple/exponential of largeBufferMultiple</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Must be a positive number.");
            }
            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(largeBufferMultiple), "Must be a positive number.");
            }
            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumBufferSize), "Must be at least blockSize.");
            }

            BlockSize = blockSize;
            LargeBufferMultiple = largeBufferMultiple;
            MaximumBufferSize = maximumBufferSize;
            UseExponentialLargeBuffer = useExponentialLargeBuffer;

            if (!IsLargeBufferSize(maximumBufferSize))
            {
                throw new ArgumentException($"{nameof(maximumBufferSize)} is not {(UseExponentialLargeBuffer ? "an exponential" : "a multiple")} of largeBufferMultiple.", nameof(maximumBufferSize));
            }

            m_SmallPool = new ConcurrentStack<byte[]>();
            var numLargePools = useExponentialLargeBuffer
                ? ((int)Math.Log(maximumBufferSize / largeBufferMultiple, 2) + 1)
                : (maximumBufferSize / largeBufferMultiple);

            // +1 to store size of bytes in use that are too large to be pooled
            m_LargeBufferInUseSize = new long[numLargePools + 1];
            m_LargeBufferFreeSize = new long[numLargePools];

            m_LargePools = new ConcurrentStack<byte[]>[numLargePools];

            for (var i = 0; i < m_LargePools.Length; ++i)
            {
                m_LargePools[i] = new ConcurrentStack<byte[]>();
            }
        }

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int LargeBufferMultiple { get; }

        /// <summary>
        /// Use multiple large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseMultipleLargeBuffer => !UseExponentialLargeBuffer;

        /// <summary>
        /// Use exponential large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseExponentialLargeBuffer { get; }

        /// <summary>
        /// Gets the maximum buffer size.
        /// </summary>
        /// <remarks>Any buffer that is returned to the pool that is larger than this will be
        /// discarded and garbage collected.</remarks>
        public int MaximumBufferSize { get; }

        /// <summary>
        /// Number of bytes in small pool not currently in use
        /// </summary>
        public long SmallPoolFreeSize => m_SmallPoolFreeSize;

        /// <summary>
        /// Number of bytes currently in use by stream from the small pool
        /// </summary>
        public long SmallPoolInUseSize => m_SmallPoolInUseSize;

        /// <summary>
        /// Number of bytes in large pool not currently in use
        /// </summary>
        public long LargePoolFreeSize
        {
            get
            {
                var sum = 0L;
                foreach (var freeSize in m_LargeBufferFreeSize)
                {
                    sum += freeSize;
                }
                return sum;
            }
        }

        /// <summary>
        /// Number of bytes currently in use by streams from the large pool
        /// </summary>
        public long LargePoolInUseSize
        {
            get
            {
                var sum = 0L;
                foreach (var inUseSize in m_LargeBufferInUseSize)
                {
                    sum += inUseSize;
                }
                return sum;
            }
        }

        /// <summary>
        /// How many blocks are in the small pool
        /// </summary>
        public long SmallBlocksFree => m_SmallPool.Count;

        /// <summary>
        /// How many buffers are in the large pool
        /// </summary>
        public long LargeBuffersFree
        {
            get
            {
                var free = 0L;
                foreach (var pool in m_LargePools)
                {
                    free += pool.Count;
                }
                return free;
            }
        }

        /// <summary>
        /// How many bytes of small free blocks to allow before we start dropping
        /// those returned to us.
        /// </summary>
        public long MaximumFreeSmallPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of large free buffers to allow before we start dropping
        /// those returned to us.
        /// </summary>
        public long MaximumFreeLargePoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will
        /// result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool. E.g. when GetBuffer() is called on
        /// a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </summary>
        public bool AggressiveBufferReturn { get; set; }

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A byte[] array</returns>
        internal byte[] GetBlock()
        {
            if (!m_SmallPool.TryPop(out var block))
            {
                // We'll add this back to the pool when the stream is disposed
                // (unless our free pool is too large)
                block = new byte[BlockSize];
            }
            else
            {
                Interlocked.Add(ref m_SmallPoolFreeSize, -BlockSize);
            }

            Interlocked.Add(ref m_SmallPoolInUseSize, BlockSize);
            return block;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the large buffer pool. This buffer
        /// will be at least the requiredSize and always be a multiple/exponential of largeBufferMultiple.
        /// </summary>
        /// <param name="requiredSize">The minimum length of the buffer</param>
        /// <returns>A buffer of at least the required size.</returns>
        internal byte[] GetLargeBuffer(int requiredSize)
        {
            requiredSize = RoundToLargeBufferSize(requiredSize);

            var poolIndex = GetPoolIndex(requiredSize);

            byte[] buffer;
            if (poolIndex < m_LargePools.Length)
            {
                if (!m_LargePools[poolIndex].TryPop(out buffer))
                {
                    buffer = new byte[requiredSize];
                }
                else
                {
                    Interlocked.Add(ref m_LargeBufferFreeSize[poolIndex], -buffer.Length);
                }
            }
            else
            {
                // Buffer is too large to pool. They get a new buffer.

                // We still want to track the size, though, and we've reserved a slot
                // in the end of the inuse array for nonpooled bytes in use.
                poolIndex = m_LargeBufferInUseSize.Length - 1;

                // We still want to round up to reduce heap fragmentation.
                buffer = new byte[requiredSize];
            }

            Interlocked.Add(ref m_LargeBufferInUseSize[poolIndex], buffer.Length);

            return buffer;
        }

        int RoundToLargeBufferSize(int requiredSize)
        {
            if (UseExponentialLargeBuffer)
            {
                var pow = 1;
                while (LargeBufferMultiple * pow < requiredSize)
                {
                    pow <<= 1;
                }
                return LargeBufferMultiple * pow;
            }
            else
            {
                return ((requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple) * LargeBufferMultiple;
            }
        }

        bool IsLargeBufferSize(int value)
        {
            return (value != 0) && (UseExponentialLargeBuffer
                ? (value == RoundToLargeBufferSize(value))
                : (value % LargeBufferMultiple) == 0);
        }

        int GetPoolIndex(int length)
        {
            if (UseExponentialLargeBuffer)
            {
                var index = 0;
                while ((LargeBufferMultiple << index) < length)
                {
                    ++index;
                }
                return index;
            }
            else
            {
                return length / LargeBufferMultiple - 1;
            }
        }

        /// <summary>
        /// Returns the buffer to the large pool
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentException">buffer.Length is not a multiple/exponential of LargeBufferMultiple (it did not originate from this pool)</exception>
        internal void ReturnLargeBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (!IsLargeBufferSize(buffer.Length))
            {
                throw new ArgumentException($"Buffer did not originate from this memory manager. The size is not {(UseExponentialLargeBuffer ? "an exponential" : "a multiple")} of {LargeBufferMultiple}.");
            }

            var poolIndex = GetPoolIndex(buffer.Length);

            if (poolIndex < m_LargePools.Length)
            {
                if ((m_LargePools[poolIndex].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes || MaximumFreeLargePoolBytes == 0)
                {
                    m_LargePools[poolIndex].Push(buffer);
                    Interlocked.Add(ref m_LargeBufferFreeSize[poolIndex], buffer.Length);
                }
            }
            else
            {
                // This is a non-poolable buffer, but we still want to track its size for inuse
                // analysis. We have space in the inuse array for this.
                poolIndex = m_LargeBufferInUseSize.Length - 1;
            }

            Interlocked.Add(ref m_LargeBufferInUseSize[poolIndex], -buffer.Length);
        }

        /// <summary>
        /// Returns the blocks to the pool
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool</param>
        /// <exception cref="ArgumentNullException">blocks is null</exception>
        /// <exception cref="ArgumentException">blocks contains buffers that are the wrong size (or null) for this memory manager</exception>
        internal void ReturnBlocks(ICollection<byte[]> blocks)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException(nameof(blocks));
            }

            var bytesToReturn = blocks.Count * BlockSize;
            Interlocked.Add(ref m_SmallPoolInUseSize, -bytesToReturn);

            foreach (var block in blocks)
            {
                if (block == null || block.Length != BlockSize)
                {
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
                }
            }

            foreach (var block in blocks)
            {
                if (MaximumFreeSmallPoolBytes == 0 || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref m_SmallPoolFreeSize, BlockSize);
                    m_SmallPool.Push(block);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
        /// on the underlying stream.</remarks>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(int requiredSize = 0, bool asContiguousBuffer = false)
        {
            if (!m_Pool.TryTake(out var stream))
            {
                stream = new RecyclableMemoryStream(this);
            }

            if (!asContiguousBuffer || requiredSize <= BlockSize)
            {
                stream.Init(requiredSize);
            }
            else
            {
                stream.Init(requiredSize, GetLargeBuffer(requiredSize));
            }

            return stream;
        }

        public void FreeStream(MemoryStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Dispose();

            if (stream is RecyclableMemoryStream recyclableMemoryStream && recyclableMemoryStream.MemoryManager == this)
            {
                m_Pool.Add(recyclableMemoryStream);
            }
        }
    }
}
