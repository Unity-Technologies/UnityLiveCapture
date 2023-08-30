using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// A class that is used to define a networked communication protocol.
    /// </summary>
    /// <remarks>
    /// The protocol consists of messages which can either be sent or received, each identified
    /// by a unique string. After messages have been added to the protocol, the protocol can be
    /// serialized and sent to the remote, allowing the remote to know exactly what messages are
    /// valid and ensure message data is formatted identically on both ends.
    /// </remarks>
    partial class Protocol : IEnumerable<MessageBase>
    {
        readonly List<MessageBase> m_Messages = new List<MessageBase>();
        readonly Dictionary<string, MessageBase> m_IdToMessage = new Dictionary<string, MessageBase>();
        readonly Dictionary<ushort, IDataReceiver> m_CodeToHandler = new Dictionary<ushort, IDataReceiver>();
        NetworkBase m_Network;
        Remote m_Remote;

        /// <summary>
        /// Gets the name of this protocol.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the version of this protocol.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets a value indicating whether the protocol is read-only.
        /// </summary>
        /// <remarks>
        /// A protocol that is read-only does not allow adding new messages after the protocol
        /// has been created.
        /// </remarks>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Protocol"/> instance.
        /// </summary>
        /// <param name="name">The name of the protocol.</param>
        /// <param name="version">The protocol version.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or only white-space.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="version"/> is null.</exception>
        public Protocol(string name, Version version)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A non-empty name must be provided.", nameof(name));
            if (version == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Version = version;
            IsReadOnly = false;
        }

        /// <summary>
        /// Sets the remote this protocol is used to communicate with.
        /// </summary>
        /// <param name="network">The network on which to communicate.</param>
        /// <param name="remote">The remote to communicate with.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="network"/> or <paramref name="remote"/> is null.</exception>
        public void SetNetwork(NetworkBase network, Remote remote)
        {
            if (network == null)
                throw new ArgumentNullException(nameof(network));
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            m_Network = network;
            m_Remote = remote;

            m_Network.DeregisterMessageHandler(m_Remote);
            m_Network.RegisterMessageHandler(m_Remote, ReceiveMessage, false);
        }

        /// <summary>
        /// Adds a message to the protocol.
        /// </summary>
        /// <param name="message">The message to add. It must not have been added to another protocol instance.</param>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <returns>The message instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this protocol is read-only or already
        /// contains <see cref="ushort.MaxValue"/> messages.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException"> Thrown if <paramref name="message"/> is already assigned to
        /// another protocol, or a message already added to this protocol has the same ID.
        /// </exception>
        public T Add<T>(T message) where T : MessageBase
        {
            if (IsReadOnly)
                throw new InvalidOperationException($"Protocol \"{this}\" is read only.");
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.Protocol != null)
                throw new ArgumentException($"Message \"{message}\" is already assigned to protocol \"{message.Protocol}\".", nameof(message));
            if (m_IdToMessage.ContainsKey(message.ID))
                throw new ArgumentException($"A message with ID \"{message.ID}\" is already defined in protocol \"{this}\".", nameof(message));
            if (m_Messages.Count >= ushort.MaxValue)
                throw new InvalidOperationException($"Protocol \"{this}\" already has the maximum number of messages registered ({ushort.MaxValue}).");

            AddInternal(message, (ushort)m_Messages.Count);
            return message;
        }

        void AddInternal(MessageBase message, ushort code)
        {
            message.SetProtocol(this, code);

            m_Messages.Add(message);
            m_IdToMessage.Add(message.ID, message);

            if (message is IDataReceiver receiver)
                m_CodeToHandler.Add(code, receiver);
        }

        /// <summary>
        /// Gets a <see cref="EventSender"/> message from this protocol by ID.
        /// </summary>
        /// <param name="id">The ID of the message.</param>
        /// <returns>The message instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if there is no message with the given ID, or the message
        /// is not a <see cref="EventSender"/>.</exception>
        public EventSender GetEventSender(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (!m_IdToMessage.TryGetValue(id, out var message))
                throw new ArgumentException($"No message with ID \"{id}\" exists in protocol \"{this}\".", nameof(id));
            if (!(message is EventSender sender))
                throw new ArgumentException($"Message with ID \"{id}\" is not of type {nameof(EventSender)}.", nameof(id));

            return sender;
        }

        /// <summary>
        /// Gets a <see cref="EventReceiver"/> message from this protocol by ID.
        /// </summary>
        /// <param name="id">The ID of the message.</param>
        /// <returns>The message instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if there is no message with the given ID, or the message
        /// is not a <see cref="EventReceiver"/>.</exception>
        public EventReceiver GetEventReceiver(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (!m_IdToMessage.TryGetValue(id, out var message))
                throw new ArgumentException($"No message with ID \"{id}\" exists in protocol \"{this}\".", nameof(id));
            if (!(message is EventReceiver receiver))
                throw new ArgumentException($"Message with ID \"{id}\" is not of type {nameof(EventReceiver)}.", nameof(id));

            return receiver;
        }

        /// <summary>
        /// Gets a <see cref="DataSender{T}"/> message from this protocol by ID.
        /// </summary>
        /// <param name="id">The ID of the message.</param>
        /// <typeparam name="T">The type of data sent by the message.</typeparam>
        /// <typeparam name="TSender">The type of the message sender.</typeparam>
        /// <returns>The message instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if there is no message with the given ID, or the message
        /// is not a <typeparamref name="TReceiver"/>.</exception>
        public TSender GetDataSender<T, TSender>(string id) where TSender : DataSender<T>
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (!m_IdToMessage.TryGetValue(id, out var message))
                throw new ArgumentException($"No message with ID \"{id}\" exists in protocol \"{this}\".", nameof(id));
            if (!(message is TSender sender))
                throw new ArgumentException($"Message with ID \"{id}\" is not of type {typeof(TSender).Name}.", nameof(id));

            return sender;
        }

        /// <summary>
        /// Gets a <see cref="DataReceiver{T}"/> message from this protocol by ID.
        /// </summary>
        /// <param name="id">The ID of the message.</param>
        /// <typeparam name="T">The type of data received by the message.</typeparam>
        /// <typeparam name="TReceiver">The type of the message receiver.</typeparam>
        /// <returns>The message instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if there is no message with the given ID, or the message
        /// is not a <typeparamref name="TReceiver"/>.</exception>
        public TReceiver GetDataReceiver<T, TReceiver>(string id) where TReceiver : DataReceiver<T>
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (!m_IdToMessage.TryGetValue(id, out var message))
                throw new ArgumentException($"No message with ID \"{id}\" exists in protocol \"{this}\".", nameof(id));
            if (!(message is TReceiver receiver))
                throw new ArgumentException($"Message with ID \"{id}\" is not of type {typeof(TReceiver).Name}.", nameof(id));

            return receiver;
        }

        /// <inheritdoc/>
        public IEnumerator<MessageBase> GetEnumerator()
        {
            return m_Messages.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates the protocol instance that a remote can use to communicate.
        /// </summary>
        /// <remarks>
        /// This clones the protocol, with all message receivers transformed into senders and all
        /// senders transformed into receivers.
        /// </remarks>
        /// <returns>A new inverted protocol instance.</returns>
        public Protocol CreateInverse()
        {
            var inverted = new Protocol(Name, Version);

            foreach (var message in this)
                inverted.Add(message.GetInverse());

            inverted.IsReadOnly = true;

            return inverted;
        }

        /// <summary>
        /// Resets the protocol state.
        /// </summary>
        /// <remarks>
        /// This re-initializes the internal state for all messages in the protocol.
        /// </remarks>
        public void Reset()
        {
            foreach (var message in this)
            {
                if (message is IDataSender sender)
                    sender.Reset();
            }
        }

        internal void SendMessage(EventSender sender)
        {
            if (m_Network == null || m_Remote == null)
                new InvalidOperationException($"Cannot send message \"{sender}\" on protocol \"{this}\" since a network has not been assigned.");

            if (m_Network.IsConnected(m_Remote))
            {
                var message = Message.Get(m_Remote, sender.Channel);
                message.Data.WriteStruct(sender.Code);

                m_Network.SendMessage(message);
            }
        }

        internal void SendMessage<T>(DataSender<T> sender, ref T data)
        {
            if (m_Network == null || m_Remote == null)
                new InvalidOperationException($"Cannot send message \"{sender}\" on protocol \"{this}\" since a network has not been assigned.");

            if (m_Network.IsConnected(m_Remote))
            {
                var message = Message.Get(m_Remote, sender.Channel);
                message.Data.WriteStruct(sender.Code);
                sender.Write(message.Data, ref data);

                m_Network.SendMessage(message);
            }
        }

        void ReceiveMessage(Message message)
        {
            try
            {
                var code = message.Data.ReadStruct<ushort>();

                if (m_CodeToHandler.TryGetValue(code, out var receiver))
                    receiver.Receive(message.Data);
            }
            finally
            {
                message.Dispose();
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Name;
    }
}
