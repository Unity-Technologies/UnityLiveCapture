using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to receive json from a remote.
    /// </summary>
    /// <remarks>
    /// This message type is optimized for messages that are relatively infrequent.
    /// It uses <see cref="JsonUtility"/> for the deserialization.
    /// </remarks>
    /// <typeparam name="T">The type of data to send. It must be compatible with <see cref="JsonUtility"/>.</typeparam>
    sealed class JsonReceiver<T> : DataReceiver<T>
    {
        readonly int m_Version;

        /// <summary>
        /// Creates a new <see cref="JsonReceiver{T}"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        public JsonReceiver(string id, ChannelType channel = ChannelType.ReliableOrdered, DataOptions options = DataOptions.Default)
            : base(id, channel, options)
        {
            m_Version = JsonSender<T>.k_Version;
        }

        /// <inheritdoc />
        [Preserve]
        internal JsonReceiver(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    break;
                default:
                    throw new Exception($"{nameof(JsonReceiver<T>)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc/>
        internal override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteStruct(m_Version);

            switch (m_Version)
            {
                case 0:
                    break;
                default:
                    throw new Exception($"{nameof(JsonReceiver<T>)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new JsonSender<T>(ID, Channel, m_Options);

        /// <inheritdoc />
        protected override T OnRead(MemoryStream stream)
        {
            switch (m_Version)
            {
                case 0:
                    var json = stream.ReadString();
                    return JsonUtility.FromJson<T>(json);
                default:
                    throw new Exception($"{nameof(JsonReceiver<T>)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Gets a <see cref="JsonReceiver{T}"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out JsonReceiver<T> message)
        {
            try
            {
                message = protocol.GetDataReceiver<T, JsonReceiver<T>>(id);
                return true;
            }
            catch
            {
                message = default;
                return false;
            }
        }
    }
}
