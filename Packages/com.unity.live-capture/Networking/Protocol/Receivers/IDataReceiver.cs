using System.IO;

namespace Unity.LiveCapture.Networking.Protocols
{
    /// <summary>
    /// An interface for messages which receive data from a remote.
    /// </summary>
    interface IDataReceiver
    {
        /// <summary>
        /// Reads data received from the network.
        /// </summary>
        /// <param name="stream">The stream containing the received data.</param>
        void Receive(MemoryStream stream);
    }
}
