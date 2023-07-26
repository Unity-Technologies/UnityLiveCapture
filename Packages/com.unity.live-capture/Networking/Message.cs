using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.IO;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A message which can be sent and received over the network. Instances are pooled to avoid
    /// allocations, so it is extremely important not to hold onto message references after they
    /// have been disposed.
    /// </summary>
    class Message : IDisposable
    {
        /// <summary>
        /// When the message contains a data stream larger than this value in bytes, the stream will
        /// be disposed when the message is returned to the pool.
        /// </summary>
        const int k_LargeMessageSize = 8 * 1024;

        static readonly ConcurrentBag<Message> s_Pool = new ConcurrentBag<Message>();
        static readonly RecyclableMemoryStreamManager s_Memory = new RecyclableMemoryStreamManager();

        Remote m_Remote;
        ChannelType m_ChannelType;
        MemoryStream m_Data;
        bool m_Disposed;

        /// <summary>
        /// The remote this message is sent to, or where the message was received from.
        /// </summary>
        public Remote Remote
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(Message));

                return m_Remote;
            }
        }

        /// <summary>
        /// The networking channel the message is delivered using.
        /// </summary>
        public ChannelType ChannelType
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(Message));

                return m_ChannelType;
            }
        }

        /// <summary>
        /// The contents of the message.
        /// </summary>
        public MemoryStream Data
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(Message));

                return m_Data;
            }
        }

        Message()
        {
        }

        /// <summary>
        /// Returns the message to the pool. Be careful not to hold onto message references after disposing,
        /// as the message instance is mutable.
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;

            m_Remote = null;
            m_ChannelType = default;

            // To minimize reallocating memory we want to avoid disposing recyclable memory stream instances
            // for messages waiting in the pool. However, this is very inefficient when individual messages
            // are large, as lots of memory will be held by unused messages in the pool. A good balance can be
            // reached by only disposing large memory streams, since they should be much less frequent, while
            // small data streams won't use much memory.
            if (m_Data.Capacity > k_LargeMessageSize)
            {
                s_Memory.FreeStream(m_Data);
                m_Data = null;
            }
            else
            {
                m_Data.SetLength(0);
                m_Data.Seek(0, SeekOrigin.Begin);
            }

            s_Pool.Add(this);
        }

        /// <summary>
        /// Gets a <see cref="Message"/> instance from the message pool. This is thread safe.
        /// </summary>
        /// <param name="remote">The remote to send the message to. Use <see cref="Networking.Remote.All"/> to send this
        /// message to all connected remotes.</param>
        /// <param name="channelType">The networking channel the message is delivered using.</param>
        /// <param name="dataCapacity">The expected size of the message data. If larger than 0, this value is used to
        /// optimize memory allocation for the data stream.</param>
        /// <returns>A message instance from the pool.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="remote"/> is null.</exception>
        public static Message Get(Remote remote, ChannelType channelType, int dataCapacity = 0)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            return GetInternal(remote, channelType, dataCapacity);
        }

        /// <summary>
        /// Gets a <see cref="Message"/> instance from the message pool. This is thread safe.
        /// </summary>
        /// <param name="remote">Where to send the message, or where the message was received from. May
        /// be left as null if not yet known.</param>
        /// <param name="channelType">The networking channel the message is delivered using.</param>
        /// <param name="dataCapacity">The expected size of the message data. If larger than 0, this value is used to
        /// optimize memory allocation for the data stream.</param>
        static Message GetInternal(Remote remote, ChannelType channelType, int dataCapacity)
        {
            if (!s_Pool.TryTake(out var message))
                message = new Message();

            message.m_Remote = remote;
            message.m_ChannelType = channelType;
            message.m_Disposed = false;

            if (message.m_Data == null || !(message.Data.CanRead && message.Data.CanWrite))
                message.m_Data = s_Memory.GetStream(dataCapacity);
            else
                message.m_Data.Capacity = dataCapacity;

            return message;
        }
    }
}
