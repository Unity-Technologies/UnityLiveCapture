using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// The data sent to create a remote.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RemoteData
    {
        Guid m_Id;
        EndPointData m_Tcp;
        EndPointData m_Udp;

        /// <summary>
        /// The ID of the <see cref="NetworkBase"/> instance for a remote.
        /// </summary>
        public Guid ID => m_Id;

        /// <summary>
        /// The TCP end point the remote uses to communicate.
        /// </summary>
        public IPEndPoint GetTcpEndPoint() => m_Tcp.GetEndPoint();

        /// <summary>
        /// The UDP end point the remote uses to communicate.
        /// </summary>
        public IPEndPoint GetUdpEndPoint() => m_Udp.GetEndPoint();

        /// <summary>
        /// Creates a new <see cref="RemoteData"/> instance.
        /// </summary>
        /// <param name="id">The ID of the <see cref="NetworkClient"/> instance for a remote.</param>
        /// <param name="tcpEndPoint">The TCP end point the remote uses to communicate.</param>
        /// <param name="udpEndPoint">The UDP end point the remote uses to communicate.</param>
        public RemoteData(Guid id, IPEndPoint tcpEndPoint, IPEndPoint udpEndPoint)
        {
            m_Id = id;
            m_Tcp = new EndPointData(tcpEndPoint);
            m_Udp = new EndPointData(udpEndPoint);
        }
    }
}
