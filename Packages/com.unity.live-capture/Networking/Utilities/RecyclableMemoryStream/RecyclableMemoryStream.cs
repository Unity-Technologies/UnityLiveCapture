// The MIT License (MIT)
//
// Copyright (c) 2015-2016 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// ---------------------------------------------------------------------
// Modified by Unity Technologies 2020
// ---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.IO
{
    /// <summary>
    /// MemoryStream implementation that deals with pooling and managing memory streams which use potentially large
    /// buffers.
    /// </summary>
    /// <remarks>
    /// This class works in tandem with the RecyclableMemoryStreamManager to supply MemoryStream
    /// objects to callers, while avoiding these specific problems:
    /// 1. LOH allocations - since all large buffers are pooled, they will never incur a Gen2 GC
    /// 2. Memory waste - A standard memory stream doubles its size when it runs out of room. This
    /// leads to continual memory growth as each stream approaches the maximum allowed size.
    /// 3. Memory copying - Each time a MemoryStream grows, all the bytes are copied into new buffers.
    /// This implementation only copies the bytes when GetBuffer is called.
    /// 4. Memory fragmentation - By using homogeneous buffer sizes, it ensures that blocks of memory
    /// can be easily reused.
    ///
    /// The stream is implemented on top of a series of uniformly-sized blocks. As the stream's length grows,
    /// additional blocks are retrieved from the memory manager. It is these blocks that are pooled, not the stream
    /// object itself.
    ///
    /// The biggest wrinkle in this implementation is when GetBuffer() is called. This requires a single
    /// contiguous buffer. If only a single block is in use, then that block is returned. If multiple blocks
    /// are in use, we retrieve a larger buffer from the memory manager. These large buffers are also pooled,
    /// split by size--they are multiples/exponentials of a chunk size (1 MB by default).
    ///
    /// Once a large buffer is assigned to the stream the blocks are NEVER again used for this stream. All operations take place on the
    /// large buffer. The large buffer can be replaced by a larger buffer from the pool as needed. All blocks and large buffers
    /// are maintained in the stream until the stream is disposed (unless AggressiveBufferReturn is enabled in the stream manager).
    ///
    /// </remarks>
    sealed class RecyclableMemoryStream : MemoryStream
    {
        const long k_MaxStreamLength = int.MaxValue;

        static readonly byte[] s_EmptyArray = new byte[0];

        readonly RecyclableMemoryStreamManager m_MemoryManager;
        long m_DisposedState;
        int m_Length;
        int m_Position;

        /// <summary>
        /// All of these blocks must be the same size.
        /// </summary>
        readonly List<byte[]> m_Blocks = new List<byte[]>(1);

        /// <summary>
        /// This buffer exists so that WriteByte can forward all of its calls to Write
        /// without creating a new byte[] buffer on every call.
        /// </summary>
        readonly byte[] m_ByteBuffer = new byte[1];

        /// <summary>
        /// This list is used to store buffers once they're replaced by something larger.
        /// This is for the cases where you have users of this class that may hold onto the buffers longer
        /// than they should and you want to prevent race conditions which could corrupt the data.
        /// </summary>
        List<byte[]> m_DirtyBuffers;

        /// <summary>
        /// This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
        /// construction if the caller immediately requests a single large buffer.
        /// </summary>
        /// <remarks>If this field is non-null, it contains the concatenation of the bytes found in the individual
        /// blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
        /// </remarks>
        byte[] m_LargeBuffer;

        /// <summary>
        /// Gets the memory manager being used by this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal RecyclableMemoryStreamManager MemoryManager => m_MemoryManager;

        /// <summary>
        /// Allocate a new RecyclableMemoryStream object.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
            : base(s_EmptyArray)
        {
            m_MemoryManager = memoryManager;
        }

        internal void Init(int requestedSize = 0, byte[] initialLargeBuffer = null)
        {
            m_DisposedState = 0L;
            m_Length = 0;
            m_Position = 0;
            m_Blocks.Clear();

            if (m_DirtyBuffers != null)
                m_DirtyBuffers.Clear();

            m_LargeBuffer = null;

            if (requestedSize < m_MemoryManager.BlockSize)
            {
                requestedSize = m_MemoryManager.BlockSize;
            }

            if (initialLargeBuffer == null)
            {
                EnsureCapacity(requestedSize);
            }
            else
            {
                m_LargeBuffer = initialLargeBuffer;
            }
        }

        ~RecyclableMemoryStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns the memory used by this stream back to the pool.
        /// </summary>
        /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false)</param>
        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref m_DisposedState, 1, 0) != 0)
            {
                return;
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            else
            {
                // We're being finalized.
#if !NETSTANDARD1_4
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    // If we're being finalized because of a shutdown, don't go any further.
                    // We have no idea what's already been cleaned up. Triggering events may cause
                    // a crash.
                    base.Dispose(disposing);
                    return;
                }
#endif
            }

            if (m_LargeBuffer != null)
            {
                m_MemoryManager.ReturnLargeBuffer(m_LargeBuffer);
            }

            if (m_DirtyBuffers != null)
            {
                foreach (var buffer in m_DirtyBuffers)
                {
                    m_MemoryManager.ReturnLargeBuffer(buffer);
                }
            }

            m_MemoryManager.ReturnBlocks(m_Blocks);
            m_Blocks.Clear();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Equivalent to Dispose.
        /// </summary>
#if NETSTANDARD1_4
        public void Close()
#else
        public override void Close()
#endif
        {
            Dispose(true);
        }

        /// <summary>
        /// Gets or sets the capacity
        /// </summary>
        /// <remarks>Capacity is always in multiples of the memory manager's block size, unless
        /// the large buffer is in use.  Capacity never decreases during a stream's lifetime.
        /// Explicitly setting the capacity to a lower value than the current value will have no effect.
        /// This is because the buffers are all pooled by chunks and there's little reason to
        /// allow stream truncation.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Capacity
        {
            get
            {
                CheckDisposed();
                if (m_LargeBuffer != null)
                {
                    return m_LargeBuffer.Length;
                }

                var size = (long)m_Blocks.Count * m_MemoryManager.BlockSize;
                return (int)Math.Min(int.MaxValue, size);
            }
            set
            {
                CheckDisposed();
                EnsureCapacity(value);
            }
        }

        /// <summary>
        /// Gets the number of bytes written to this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override long Length
        {
            get
            {
                CheckDisposed();
                return m_Length;
            }
        }

        /// <summary>
        /// Gets the current position in the stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override long Position
        {
            get
            {
                CheckDisposed();
                return m_Position;
            }
            set
            {
                CheckDisposed();

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be non-negative!");
                }
                if (value > k_MaxStreamLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Cannot be more than {k_MaxStreamLength}!");
                }

                m_Position = (int)value;
            }
        }

        /// <summary>
        /// Whether the stream can currently read
        /// </summary>
        public override bool CanRead => !Disposed;

        /// <summary>
        /// Whether the stream can currently seek
        /// </summary>
        public override bool CanSeek => !Disposed;

        /// <summary>
        /// Always false
        /// </summary>
        public override bool CanTimeout => false;

        /// <summary>
        /// Whether the stream can currently write
        /// </summary>
        public override bool CanWrite => !Disposed;

        /// <summary>
        /// Returns a single buffer containing the contents of the stream.
        /// The buffer may be longer than the stream length.
        /// </summary>
        /// <returns>A byte[] buffer</returns>
        /// <remarks>IMPORTANT: Doing a Write() after calling GetBuffer() invalidates the buffer. The old buffer is held onto
        /// until Dispose is called, but the next time GetBuffer() is called, a new buffer from the pool will be required.</remarks>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#if NETSTANDARD1_4
        public byte[] GetBuffer()
#else
        public override byte[] GetBuffer()
#endif
        {
            CheckDisposed();

            if (m_LargeBuffer != null)
            {
                return m_LargeBuffer;
            }

            if (m_Blocks.Count == 1)
            {
                return m_Blocks[0];
            }

            // Buffer needs to reflect the capacity, not the length, because
            // it's possible that people will manipulate the buffer directly
            // and set the length afterward. Capacity sets the expectation
            // for the size of the buffer.
            var newBuffer = m_MemoryManager.GetLargeBuffer(Capacity);

            // InternalRead will check for existence of largeBuffer, so make sure we
            // don't set it until after we've copied the data.
            InternalRead(newBuffer, 0, m_Length, 0);
            m_LargeBuffer = newBuffer;

            if (m_Blocks.Count > 0 && m_MemoryManager.AggressiveBufferReturn)
            {
                m_MemoryManager.ReturnBlocks(m_Blocks);
                m_Blocks.Clear();
            }

            return m_LargeBuffer;
        }

        /// <summary>
        /// Returns an ArraySegment that wraps a single buffer containing the contents of the stream.
        /// </summary>
        /// <param name="buffer">An ArraySegment containing a reference to the underlying bytes.</param>
        /// <returns>Always returns true.</returns>
        /// <remarks>GetBuffer has no failure modes (it always returns something, even if it's an empty buffer), therefore this method
        /// always returns a valid ArraySegment to the same buffer returned by GetBuffer.</remarks>
#if NET40 || NET45
        public bool TryGetBuffer(out ArraySegment<byte> buffer)
#else
        public override bool TryGetBuffer(out ArraySegment<byte> buffer)
#endif
        {
            CheckDisposed();
            buffer = new ArraySegment<byte>(GetBuffer(), 0, (int)Length);
            // GetBuffer has no failure modes, so this should always succeed
            return true;
        }

        /// <summary>
        /// Returns a new array with a copy of the buffer's contents. You should almost certainly be using GetBuffer combined with the Length to
        /// access the bytes in this stream. Calling ToArray will destroy the benefits of pooled buffers, but it is included
        /// for the sake of completeness.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#pragma warning disable CS0809
        [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
        public override byte[] ToArray()
        {
            CheckDisposed();
            var newBuffer = new byte[Length];
            InternalRead(newBuffer, 0, m_Length, 0);
            return newBuffer;
        }

#pragma warning restore CS0809

        /// <summary>
        /// Reads from the current position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
        /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return SafeRead(buffer, offset, count, ref m_Position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="streamPosition">Position in the stream to start reading from</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
        /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public int SafeRead(byte[] buffer, int offset, int count, ref int streamPosition)
        {
            CheckDisposed();

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count");
            }

            var amountRead = InternalRead(buffer, offset, count, streamPosition);
            streamPosition += amountRead;
            return amountRead;
        }

#if NETCOREAPP2_1 || NETSTANDARD2_1
        /// <summary>
        /// Reads from the current position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Read(Span<byte> buffer)
        {
            return SafeRead(buffer, ref m_Position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="streamPosition">Position in the stream to start reading from</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public int SafeRead(Span<byte> buffer, ref int streamPosition)
        {
            CheckDisposed();

            var amountRead = InternalRead(buffer, streamPosition);
            streamPosition += amountRead;
            return amountRead;
        }
#endif

        /// <summary>
        /// Writes the buffer to the stream
        /// </summary>
        /// <param name="buffer">Source buffer</param>
        /// <param name="offset">Start position</param>
        /// <param name="count">Number of bytes to write</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="ArgumentException">buffer.Length - offset is not less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be in the range [0, buffer.Length - 1].");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be non-negative.");
            }
            if (count + offset > buffer.Length)
            {
                throw new ArgumentException("Count must be greater than buffer.Length - offset.");
            }

            var blockSize = m_MemoryManager.BlockSize;
            var end = (long)m_Position + count;
            // Check for overflow
            if (end > k_MaxStreamLength)
            {
                throw new IOException("Maximum capacity exceeded.");
            }

            EnsureCapacity((int)end);

            if (m_LargeBuffer == null)
            {
                var bytesRemaining = count;
                var bytesWritten = 0;
                var blockAndOffset = GetBlockAndRelativeOffset(m_Position);

                while (bytesRemaining > 0)
                {
                    var currentBlock = m_Blocks[blockAndOffset.Block];
                    var remainingInBlock = blockSize - blockAndOffset.Offset;
                    var amountToWriteInBlock = Math.Min(remainingInBlock, bytesRemaining);

                    Buffer.BlockCopy(buffer, offset + bytesWritten, currentBlock, blockAndOffset.Offset, amountToWriteInBlock);

                    bytesRemaining -= amountToWriteInBlock;
                    bytesWritten += amountToWriteInBlock;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, m_LargeBuffer, m_Position, count);
            }

            m_Position = (int)end;
            m_Length = Math.Max(m_Position, m_Length);
        }

#if NETCOREAPP2_1 || NETSTANDARD2_1
        /// <summary>
        /// Writes the buffer to the stream
        /// </summary>
        /// <param name="source">Source buffer</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void Write(ReadOnlySpan<byte> source)
        {
            CheckDisposed();

            var blockSize = m_MemoryManager.BlockSize;
            var end = (long)m_Position + source.Length;
            // Check for overflow
            if (end > k_MaxStreamLength)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            EnsureCapacity((int)end);

            if (m_LargeBuffer == null)
            {
                var blockAndOffset = GetBlockAndRelativeOffset(m_Position);

                while (source.Length > 0)
                {
                    var currentBlock = m_Blocks[blockAndOffset.Block];
                    var remainingInBlock = blockSize - blockAndOffset.Offset;
                    var amountToWriteInBlock = Math.Min(remainingInBlock, source.Length);

                    source.Slice(0, amountToWriteInBlock).CopyTo(currentBlock.AsSpan(blockAndOffset.Offset));
                    source = source.Slice(amountToWriteInBlock);

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
            }
            else
            {
                source.CopyTo(m_LargeBuffer.AsSpan(m_Position));
            }

            m_Position = (int)end;
            m_Length = Math.Max(m_Position, m_Length);
        }
#endif

        /// <summary>
        /// Returns a useful string for debugging. This should not normally be called in actual production code.
        /// </summary>
        public override string ToString()
        {
            return $"Length = {Length:N0} bytes";
        }

        /// <summary>
        /// Writes a single byte to the current position in the stream.
        /// </summary>
        /// <param name="value">byte value to write</param>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void WriteByte(byte value)
        {
            CheckDisposed();
            m_ByteBuffer[0] = value;
            Write(m_ByteBuffer, 0, 1);
        }

        /// <summary>
        /// Reads a single byte from the current position in the stream.
        /// </summary>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int ReadByte()
        {
            return SafeReadByte(ref m_Position);
        }

        /// <summary>
        /// Reads a single byte from the specified position in the stream.
        /// </summary>
        /// <param name="streamPosition">The position in the stream to read from</param>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public int SafeReadByte(ref int streamPosition)
        {
            CheckDisposed();

            if (streamPosition == m_Length)
            {
                return -1;
            }

            byte value;
            if (m_LargeBuffer == null)
            {
                var blockAndOffset = GetBlockAndRelativeOffset(streamPosition);
                value = m_Blocks[blockAndOffset.Block][blockAndOffset.Offset];
            }
            else
            {
                value = m_LargeBuffer[streamPosition];
            }

            streamPosition++;
            return value;
        }

        /// <summary>
        /// Sets the length of the stream
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value is negative or larger than MaxStreamLength</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void SetLength(long value)
        {
            CheckDisposed();

            if (value < 0 || value > k_MaxStreamLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Value must be non-negative and at most {k_MaxStreamLength}!");
            }

            EnsureCapacity((int)value);

            m_Length = (int)value;
            if (m_Position > value)
            {
                m_Position = (int)value;
            }
        }

        /// <summary>
        /// Sets the position to the offset from the seek location
        /// </summary>
        /// <param name="offset">How many bytes to move</param>
        /// <param name="loc">From where</param>
        /// <returns>The new position</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset is larger than MaxStreamLength</exception>
        /// <exception cref="ArgumentException">Invalid seek origin</exception>
        /// <exception cref="IOException">Attempt to set negative position</exception>
        public override long Seek(long offset, SeekOrigin loc)
        {
            CheckDisposed();

            if (offset > k_MaxStreamLength)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), $"Offset cannot be larger than {k_MaxStreamLength}!");
            }

            var newPosition = loc switch
            {
                SeekOrigin.Begin => (int)offset,
                SeekOrigin.Current => (int)offset + m_Position,
                SeekOrigin.End => (int)offset + m_Length,
                _ => throw new ArgumentException("Invalid seek origin", nameof(loc))
            };

            if (newPosition < 0)
            {
                throw new IOException("Seek before beginning");
            }

            m_Position = newPosition;
            return m_Position;
        }

        /// <summary>
        /// Synchronously writes this stream's bytes to the parameter stream.
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <remarks>Important: This does a synchronous write, which may not be desired in some situations</remarks>
        public override void WriteTo(Stream stream)
        {
            CheckDisposed();

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (m_LargeBuffer == null)
            {
                var currentBlock = 0;
                var bytesRemaining = m_Length;

                while (bytesRemaining > 0)
                {
                    var amountToCopy = Math.Min(m_Blocks[currentBlock].Length, bytesRemaining);
                    stream.Write(m_Blocks[currentBlock], 0, amountToCopy);

                    bytesRemaining -= amountToCopy;

                    ++currentBlock;
                }
            }
            else
            {
                stream.Write(m_LargeBuffer, 0, m_Length);
            }
        }

        bool Disposed => Interlocked.Read(ref m_DisposedState) != 0;

        void CheckDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException($"This memory stream is disposed.");
            }
        }

        int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
        {
            if (m_Length - fromPosition <= 0)
            {
                return 0;
            }

            int amountToCopy;

            if (m_LargeBuffer == null)
            {
                var blockAndOffset = GetBlockAndRelativeOffset(fromPosition);
                var bytesWritten = 0;
                var bytesRemaining = Math.Min(count, m_Length - fromPosition);

                while (bytesRemaining > 0)
                {
                    amountToCopy = Math.Min(m_Blocks[blockAndOffset.Block].Length - blockAndOffset.Offset, bytesRemaining);
                    Buffer.BlockCopy(m_Blocks[blockAndOffset.Block], blockAndOffset.Offset, buffer, bytesWritten + offset, amountToCopy);

                    bytesWritten += amountToCopy;
                    bytesRemaining -= amountToCopy;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
                return bytesWritten;
            }

            amountToCopy = Math.Min(count, m_Length - fromPosition);
            Buffer.BlockCopy(m_LargeBuffer, fromPosition, buffer, offset, amountToCopy);
            return amountToCopy;
        }

#if NETCOREAPP2_1 || NETSTANDARD2_1
        int InternalRead(Span<byte> buffer, int fromPosition)
        {
            if (m_Length - fromPosition <= 0)
            {
                return 0;
            }

            int amountToCopy;

            if (m_LargeBuffer == null)
            {
                var blockAndOffset = GetBlockAndRelativeOffset(fromPosition);
                var bytesWritten = 0;
                var bytesRemaining = Math.Min(buffer.Length, m_Length - fromPosition);

                while (bytesRemaining > 0)
                {
                    amountToCopy = Math.Min(m_Blocks[blockAndOffset.Block].Length - blockAndOffset.Offset, bytesRemaining);
                    m_Blocks[blockAndOffset.Block].AsSpan(blockAndOffset.Offset, amountToCopy).CopyTo(buffer.Slice(bytesWritten));

                    bytesWritten += amountToCopy;
                    bytesRemaining -= amountToCopy;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
                return bytesWritten;
            }

            amountToCopy = Math.Min(buffer.Length, m_Length - fromPosition);
            m_LargeBuffer.AsSpan(fromPosition, amountToCopy).CopyTo(buffer);
            return amountToCopy;
        }
#endif

        struct BlockAndOffset
        {
            public int Block;
            public int Offset;

            public BlockAndOffset(int block, int offset)
            {
                Block = block;
                Offset = offset;
            }
        }

        BlockAndOffset GetBlockAndRelativeOffset(int offset)
        {
            var blockSize = m_MemoryManager.BlockSize;
            return new BlockAndOffset(offset / blockSize, offset % blockSize);
        }

        void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > m_MemoryManager.MaximumStreamCapacity && m_MemoryManager.MaximumStreamCapacity > 0)
            {
                throw new InvalidOperationException($"Requested capacity is too large: {newCapacity}. Limit is {m_MemoryManager.MaximumStreamCapacity}.");
            }

            if (m_LargeBuffer != null)
            {
                if (newCapacity > m_LargeBuffer.Length)
                {
                    var newBuffer = m_MemoryManager.GetLargeBuffer(newCapacity);
                    InternalRead(newBuffer, 0, m_Length, 0);
                    ReleaseLargeBuffer();
                    m_LargeBuffer = newBuffer;
                }
            }
            else
            {
                while (Capacity < newCapacity)
                {
                    m_Blocks.Add((m_MemoryManager.GetBlock()));
                }
            }
        }

        /// <summary>
        /// Release the large buffer (either stores it for eventual release or returns it immediately).
        /// </summary>
        void ReleaseLargeBuffer()
        {
            if (m_MemoryManager.AggressiveBufferReturn)
            {
                m_MemoryManager.ReturnLargeBuffer(m_LargeBuffer);
            }
            else
            {
                if (m_DirtyBuffers == null)
                {
                    // We most likely will only ever need space for one
                    m_DirtyBuffers = new List<byte[]>(1);
                }
                m_DirtyBuffers.Add(m_LargeBuffer);
            }

            m_LargeBuffer = null;
        }
    }
}
