using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A struct used to send an endpoint over the network.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct EndPointData : IEquatable<EndPointData>
    {
        uint m_IP;
        ushort m_Port;

        /// <summary>
        /// Gets the end point.
        /// </summary>
        public IPEndPoint GetEndPoint() => new IPEndPoint(IPAddress.NetworkToHostOrder((long)m_IP << 32), m_Port);

        /// <summary>
        /// Creates a new <see cref="EndPointData"/> instance.
        /// </summary>
        /// <param name="endPoint">The end point to store.</param>
        public EndPointData(IPEndPoint endPoint)
        {
            m_IP = NetworkUtilities.GetAddressBits(endPoint.Address);
            m_Port = (ushort)endPoint.Port;
        }

        /// <inheritdoc />
        public bool Equals(EndPointData other)
        {
            return m_IP == other.m_IP && m_Port == other.m_Port;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EndPointData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)m_IP * 397) ^ m_Port.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetEndPoint().ToString();
        }
    }
}
