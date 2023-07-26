using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to send events to a remote.
    /// </summary>
    sealed class EventSender : MessageBase
    {
        /// <summary>
        /// Creates a new <see cref="EventSender"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        public EventSender(string id) : base(id, ChannelType.ReliableOrdered)
        {
        }

        [Preserve]
        internal EventSender(Stream stream) : base(stream)
        {
        }

        /// <summary>
        /// Invokes this event.
        /// </summary>
        public void Send()
        {
            try
            {
                Protocol.SendMessage(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new EventReceiver(ID);

        /// <summary>
        /// Gets a <see cref="EventSender"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out EventSender message)
        {
            try
            {
                message = protocol.GetEventSender(id);
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
