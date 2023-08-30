namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// The interface for captured data that has a sampling time.
    /// </summary>
    interface ISample
    {
        /// <summary>
        /// The time of the sample expressed in seconds.
        /// </summary>
        /// <remarks>
        /// In the absence of an external timecode source, this value presents the time in seconds since the client connected.
        /// This can be used to determine the order and timing of this sample relative to other samples received.
        /// </remarks>
        double Time { get; }
    }
}
