namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// An enum defining the modes a device operates in.
    /// </summary>
    enum DeviceMode : byte
    {
        /// <summary>
        /// The device is disabled.
        /// </summary>
        None = 0,

        /// <summary>
        /// The device is ready for playing recorded takes.
        /// </summary>
        Playback = 10,

        /// <summary>
        /// The device is ready for receiving live data.
        /// </summary>
        LiveStream = 20,
    }
}
