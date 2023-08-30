using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A message used to receive events from a remote.
    /// </summary>
    sealed class EventReceiver : MessageBase, IDataReceiver
    {
        readonly List<Action> m_Handlers = new List<Action>();

        /// <summary>
        /// Creates a new <see cref="EventSender"/> instance.
        /// </summary>
        /// <param name="id">A unique identifier for this message.</param>
        public EventReceiver(string id) : base(id, ChannelType.ReliableOrdered)
        {
        }

        [Preserve]
        internal EventReceiver(Stream stream) : base(stream)
        {
        }

        /// <summary>
        /// Adds a callback to invoke when this event is raised.
        /// </summary>
        /// <param name="callback">The callback to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
        public void AddHandler(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!m_Handlers.Contains(callback))
            {
                m_Handlers.Add(callback);
            }
        }

        /// <summary>
        /// Removes a callback from this event.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        /// <returns><see langword="true"/> if the callback was removed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
        public bool RemoveHandler(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return m_Handlers.Remove(callback);
        }

        /// <inheritdoc />
        void IDataReceiver.Receive(MemoryStream stream)
        {
            foreach (var handler in m_Handlers)
            {
                try
                {
                    handler.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <inheritdoc />
        internal override MessageBase GetInverse() => new EventSender(ID);

        /// <summary>
        /// Gets a <see cref="EventReceiver"/> from a protocol by ID.
        /// </summary>
        /// <param name="protocol">The protocol to get the message from.</param>
        /// <param name="id">The ID of the message.</param>
        /// <param name="message">The returned message instance, or <see langword="default"/> if the message was not found.</param>
        /// <returns><see langword="true"/> if the message was found, otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Protocol protocol, string id, out EventReceiver message)
        {
            try
            {
                message = protocol.GetEventReceiver(id);
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
