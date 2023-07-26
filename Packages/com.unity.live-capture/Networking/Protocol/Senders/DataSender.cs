using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// An enum defining options that configure how data is sent over the network.
    /// </summary>
    [Flags]
    enum DataOptions : int
    {
        /// <summary>
        /// No options specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only transport data over the network if the data to send is different from the
        /// data most recently sent to the remote. This is expensive for large messages,
        /// so consider disabling for data sent infrequently.
        /// </summary>
        OnlySendChangedValues = (1 << 0),

        /// <summary>
        /// The default options.
        /// </summary>
        Default = OnlySendChangedValues,
    }

    /// <summary>
    /// The base class for message used to send data to a remote.
    /// </summary>
    /// <typeparam name="T">The type of data to send.</typeparam>
    abstract class DataSender<T> : MessageBase, IDataSender
    {
        /// <summary>
        /// The latest version of the message serialized format.
        /// </summary>
        internal const int k_Version = 0;

        /// <summary>
        /// The flags used to configure how data is sent.
        /// </summary>
        protected readonly DataOptions m_Options;

        readonly int m_Version;
        readonly MemoryStream m_Buffer0;
        readonly MemoryStream m_Buffer1;
        int m_LastBuffer;

        /// <summary>
        /// Creates a new <see cref="DataSender{T}"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        protected DataSender(string id, ChannelType channel, DataOptions options) : base(id, channel)
        {
            m_Version = k_Version;
            m_Options = options;

            if (m_Options.Contains(DataOptions.OnlySendChangedValues))
            {
                m_Buffer0 = new MemoryStream();
                m_Buffer1 = new MemoryStream();
            }

            Reset();
        }

        /// <inheritdoc/>
        protected DataSender(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    m_Options = stream.ReadStruct<DataOptions>();
                    break;
                default:
                    throw new Exception($"{nameof(DataSender<T>)} version is not supported by this application version.");
            }

            if (m_Options.Contains(DataOptions.OnlySendChangedValues))
            {
                m_Buffer0 = new MemoryStream();
                m_Buffer1 = new MemoryStream();
            }

            Reset();
        }

        /// <inheritdoc/>
        internal override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteStruct(m_Version);

            switch (m_Version)
            {
                case 0:
                    stream.WriteStruct(m_Options);
                    break;
                default:
                    throw new Exception($"{nameof(DataSender<T>)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc/>
        void IDataSender.Reset()
        {
            m_LastBuffer = -1;
        }

        /// <summary>
        /// Sends data to the remote.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public unsafe void Send(T data)
        {
            try
            {
                // If we have previously sent a message, we optionally check if the new message is identical
                // to the last message sent avoid sending redundant data. To do this, we double buffer messages
                // so we can do a memory comparison of the formatted message buffer with the last message sent.
                if (m_Options.Contains(DataOptions.OnlySendChangedValues))
                {
                    var lastBuffer = m_LastBuffer == 0 ? m_Buffer0 : m_Buffer1;
                    var tempBuffer = m_LastBuffer == 0 ? m_Buffer1 : m_Buffer0;

                    tempBuffer.SetLength(0);
                    OnWrite(tempBuffer, ref data);

                    // if the buffers have different lengths, then trivially they can't have identical contents
                    if (m_LastBuffer >= 0 && lastBuffer.Length == tempBuffer.Length)
                    {
                        fixed (void* lastPtr = &lastBuffer.GetBuffer()[0])
                        fixed (void* tempPtr = &tempBuffer.GetBuffer()[0])
                        {
                            // if the message is the same, we don't send anything
                            if (UnsafeUtility.MemCmp(lastPtr, tempPtr, tempBuffer.Length) == 0)
                                return;
                        }
                    }

                    // remember which buffer the latest data was written to
                    m_LastBuffer = (m_LastBuffer + 1) % 2;
                }

                Protocol.SendMessage(this, ref data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Reset();
            }
        }

        internal void Write(MemoryStream stream, ref T data)
        {
            if (m_Options.Contains(DataOptions.OnlySendChangedValues))
            {
                var buffer = m_LastBuffer == 0 ? m_Buffer0 : m_Buffer1;
                stream.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
            }
            else
            {
                OnWrite(stream, ref data);
            }
        }

        void Reset()
        {
            ((IDataSender)this).Reset();
        }

        /// <summary>
        /// Formats data to be sent on the network.
        /// </summary>
        /// <param name="stream">The stream to write into.</param>
        /// <param name="data">The data to write.</param>
        protected abstract void OnWrite(MemoryStream stream, ref T data);
    }
}
