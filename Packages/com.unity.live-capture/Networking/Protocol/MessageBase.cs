using System;
using System.IO;
using UnityEngine;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// The base class used by all messages that can be added to a <see cref="Protocols.Protocol"/>.
    /// </summary>
    abstract class MessageBase
    {
        /// <summary>
        /// The latest version of the message serialized format.
        /// </summary>
        const int k_Version = 0;

        /// <summary>
        /// The unique identifier of this message.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The network channel used to deliver this message.
        /// </summary>
        public ChannelType Channel { get; }

        /// <summary>
        /// The protocol instance this message belongs to.
        /// </summary>
        public Protocol Protocol { get; private set; }

        /// <summary>
        /// The code used to identify packets associated with this message.
        /// </summary>
        internal ushort Code { get; private set; }

        readonly int m_Version;

        /// <summary>
        /// Creates a new <see cref="MessageBase"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
        protected MessageBase(string id, ChannelType channel)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            m_Version = k_Version;
            ID = id;
            Channel = channel;
        }

        /// <summary>
        /// Deserializes a <see cref="MessageBase"/> instance from a data stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        protected MessageBase(Stream stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    ID = stream.ReadString();
                    Channel = stream.ReadStruct<ChannelType>();
                    Code = stream.ReadStruct<ushort>();
                    break;
                default:
                    throw new Exception($"{nameof(MessageBase)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Serializes this message to a data stream.
        /// </summary>
        /// <param name="stream">The stream to write into.</param>
        internal virtual void Serialize(Stream stream)
        {
            stream.WriteStruct(m_Version);

            switch (m_Version)
            {
                case 0:
                    stream.WriteString(ID);
                    stream.WriteStruct(Channel);
                    stream.WriteStruct(Code);
                    break;
                default:
                    throw new Exception($"{nameof(MessageBase)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Assigns this message to a protocol.
        /// </summary>
        /// <param name="protocol">The protocol this message will belong to.</param>
        /// <param name="code">The code used to identify packets associated with this message.</param>
        internal void SetProtocol(Protocol protocol, ushort code)
        {
            Protocol = protocol;
            Code = code;
        }

        internal abstract MessageBase GetInverse();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => ID;
    }
}
