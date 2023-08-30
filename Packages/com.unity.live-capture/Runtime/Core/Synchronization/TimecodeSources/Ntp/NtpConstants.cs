namespace Unity.LiveCapture.Ntp
{
    /// <summary>
    /// A class that defines constant values used by the network time protocol.
    /// </summary>
    static class NtpConstants
    {
        /// <summary>
        /// The most recent version of the NTP protocol.
        /// </summary>
        public const int CurrentVersion = 4;

        /// <summary>
        /// The standard port used for NTP.
        /// </summary>
        public const int Port = 123;

        /// <summary>
        /// The size of a basic NTP packet in bytes.
        /// </summary>
        public const int PacketLength = 48;
    }
}
