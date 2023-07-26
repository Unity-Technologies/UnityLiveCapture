namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// Holds a message and any metadata used by the network layer.
    /// </summary>
    readonly struct Packet
    {
        /// <summary>
        /// The types of messages used in the network protocol.
        /// </summary>
        public enum Type : byte
        {
            /// <summary>
            /// Indicates an invalid message type.
            /// </summary>
            Invalid = 0,
            /// <summary>
            /// A non-protocol message which contains generic data.
            /// </summary>
            Generic = 1,
            /// <summary>
            /// The message sent to establish a new connection.
            /// </summary>
            Initialization = 10,
            /// <summary>
            /// A message sent periodically to ensure the connection is still open.
            /// </summary>
            Heartbeat = 50,
            /// <summary>
            /// A message sent to gracefully close a connection.
            /// </summary>
            Disconnect = 100,
        }

        /// <summary>
        /// The message associated with this packet.
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// The meaning of the message in the network protocol.
        /// </summary>
        public Type PacketType { get; }

        /// <summary>
        /// Creates a new <see cref="Packet"/> instance.
        /// </summary>
        /// <param name="message">The message associated with this packet.</param>
        /// <param name="type">The meaning of the message in the network protocol.</param>
        public Packet(Message message, Type type)
        {
            Message = message;
            PacketType = type;
        }
    }
}
