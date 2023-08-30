namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// Defines constants used for server discovery.
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// The default UDP port used for server discovery.
        /// </summary>
        public static readonly ushort DefaultPort = 12043;

        /// <summary>
        /// The number of characters that may be used in the name strings.
        /// </summary>
        public const int StringMaxLength = 32;
    }
}
