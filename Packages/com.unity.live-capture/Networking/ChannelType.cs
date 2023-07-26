namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// The communication channels which may be supported by the networking implementation.
    /// </summary>
    enum ChannelType : byte
    {
        /// <summary>
        /// A channel with reliable and ordered delivery of packets. Messages sent on this
        /// channel are guaranteed to arrive, and arrive in the same order they were sent in.
        /// </summary>
        ReliableOrdered = 0,
        /// <summary>
        /// A channel with unreliable and unordered delivery of packets. Messages sent on this
        /// channel may not arrive, and may not arrive in the same order they were sent in.
        /// </summary>
        UnreliableUnordered = 32,
    }
}
