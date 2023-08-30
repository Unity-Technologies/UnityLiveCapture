using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to send blittable structs to a remote.
    /// </summary>
    /// <remarks>
    /// This message type is optimized for messages that are relatively infrequent.
    /// It uses <see cref="JsonUtility"/> for the serialization.
    /// </remarks>
    /// <typeparam name="T">The type of data to send. It must be compatible with <see cref="JsonUtility"/>.</typeparam>
    sealed class JsonSender<T> : DataSender<T>
    {
        /// <summary>
        /// The latest version of the message serialized format.
        /// </summary>
        internal new const int k_Version = 0;

        readonly int m_Version;

        /// <summary>
        /// Creates a new <see cref="JsonSender{T}"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        public JsonSender(string id, ChannelType channel = ChannelType.ReliableOrdered, DataOptions options = DataOptions.Default)
            : base(id, channel, options)
        {
            m_Version = k_Version;
        }

        /// <inheritdoc />
        [Preserve]
        internal JsonSender(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    break;
                default:
                    throw new Exception($"{nameof(JsonSender<T>)} version is not supported by this application version.");
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
                    throw new Exception($"{nameof(JsonSender<T>)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new JsonReceiver<T>(ID, Channel, m_Options);

        /// <inheritdoc />
        protected override void OnWrite(MemoryStream stream, ref T data)
        {
            switch (m_Version)
            {
                case 0:
                    var json = JsonUtility.ToJson(data);
                    stream.WriteString(json);
                    break;
                default:
                    throw new Exception($"{nameof(JsonSender<T>)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Gets a <see cref="JsonSender{T}"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out JsonSender<T> message)
        {
            try
            {
                message = protocol.GetDataSender<T, JsonSender<T>>(id);
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
