namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// An interface for messages which send data to a remote.
    /// </summary>
    interface IDataSender
    {
        /// <summary>
        /// Resets the sender state.
        /// </summary>
        void Reset();
    }
}
