namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// Describes how a disconnection occured.
    /// </summary>
    enum DisconnectStatus
    {
        /// <summary>
        /// The connection was shut down gracefully.
        /// </summary>
        Graceful,
        /// <summary>
        /// The remote could not be reached long enough for the connection to time out.
        /// </summary>
        Timeout,
        /// <summary>
        /// A fatal network error was encountered so the connection was terminated.
        /// </summary>
        Error,
        /// <summary>
        /// The remote has reconnected without gracefully disconnecting first.
        /// </summary>
        Reconnected,
    }
}
