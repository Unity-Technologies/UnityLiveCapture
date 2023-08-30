using System;
using System.IO;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to receive boolean values from a remote.
    /// </summary>
    sealed class BoolReceiver : DataReceiver<bool>
    {
        readonly int m_Version;

        /// <summary>
        /// Creates a new <see cref="BoolReceiver"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        public BoolReceiver(string id, ChannelType channel = ChannelType.ReliableOrdered, DataOptions options = DataOptions.Default)
            : base(id, channel, options)
        {
            m_Version = BoolSender.k_Version;
        }

        /// <inheritdoc />
        [Preserve]
        internal BoolReceiver(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    break;
                default:
                    throw new Exception($"{nameof(BoolReceiver)} version is not supported by this application version.");
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
                    throw new Exception($"{nameof(BoolReceiver)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new BoolSender(ID, Channel, m_Options);

        /// <inheritdoc />
        protected override bool OnRead(MemoryStream stream)
        {
            switch (m_Version)
            {
                case 0:
                    return stream.ReadStruct<byte>() == 1;
                default:
                    throw new Exception($"{nameof(BoolReceiver)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Gets a <see cref="BoolReceiver"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out BoolReceiver message)
        {
            try
            {
                message = protocol.GetDataReceiver<bool, BoolReceiver>(id);
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
