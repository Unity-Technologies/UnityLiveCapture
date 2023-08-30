using System;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// The data broadcast on the network to inform clients that a server is no longer available.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    struct ShutdownData
    {
        Guid m_Id;

        /// <summary>
        /// The unique identifier of the server instance.
        /// </summary>
        public Guid ID => m_Id;

        /// <summary>
        /// Creates a new <see cref="ShutdownData"/> instance.
        /// </summary>
        /// <param name="id">The unique identifier of the server instance.</param>
        public ShutdownData(Guid id)
        {
            m_Id = id;
        }
    }
}
