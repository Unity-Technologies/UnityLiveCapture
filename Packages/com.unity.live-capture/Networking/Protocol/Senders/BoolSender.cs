using System;
using System.IO;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to send boolean values to a remote.
    /// </summary>
    sealed class BoolSender : DataSender<bool>
    {
        /// <summary>
        /// The latest version of the message serialized format.
        /// </summary>
        internal new const int k_Version = 0;

        readonly int m_Version;

        /// <summary>
        /// Creates a new <see cref="BoolSender"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        /// <param name="channel">The network channel used to deliver this message.</param>
        /// <param name="options">The flags used to configure how data is sent.</param>
        public BoolSender(string id, ChannelType channel = ChannelType.ReliableOrdered, DataOptions options = DataOptions.Default)
            : base(id, channel, options)
        {
            m_Version = k_Version;
        }

        /// <inheritdoc/>
        [Preserve]
        internal BoolSender(Stream stream) : base(stream)
        {
            m_Version = stream.ReadStruct<int>();

            switch (m_Version)
            {
                case 0:
                    break;
                default:
                    throw new Exception($"{nameof(BoolSender)} version is not supported by this application version.");
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
                    throw new Exception($"{nameof(BoolSender)} version is not supported by this application version.");
            }
        }

        /// <inheritdoc/>
        internal override MessageBase GetInverse() => new BoolReceiver(ID, Channel, m_Options);

        /// <inheritdoc />
        protected override void OnWrite(MemoryStream stream, ref bool data)
        {
            switch (m_Version)
            {
                case 0:
                    stream.WriteStruct(data ? (byte)1 : (byte)0);
                    break;
                default:
                    throw new Exception($"{nameof(BoolSender)} version is not supported by this application version.");
            }
        }

        /// <summary>
        /// Gets a <see cref="BoolSender"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out BoolSender message)
        {
            try
            {
                message = protocol.GetDataSender<bool, BoolSender>(id);
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
