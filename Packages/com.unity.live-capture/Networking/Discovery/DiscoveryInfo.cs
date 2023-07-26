using System;
using System.Linq;
using System.Net;

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// A struct containing information about a discovered server.
    /// </summary>
    struct DiscoveryInfo : IEquatable<DiscoveryInfo>
    {
        /// <summary>
        /// The properties of the discovered server.
        /// </summary>
        public ServerData ServerInfo { get; }

        /// <summary>
        /// The end points on which the server is accepting new connections.
        /// </summary>
        public IPEndPoint[] EndPoints { get; }

        /// <summary>
        /// Creates a new <see cref="DiscoveryInfo"/> instance.
        /// </summary>
        /// <param name="serverInfo">The properties of the discovered server.</param>
        /// <param name="endPoints">The end points on which the server is accepting new connections.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endPoints"/> is null.</exception>
        public DiscoveryInfo(ServerData serverInfo, IPEndPoint[] endPoints)
        {
            ServerInfo = serverInfo;
            EndPoints = endPoints ?? throw new ArgumentNullException(nameof(endPoints));
        }

        /// <summary>
        /// Determines whether the <see cref="DiscoveryInfo"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="DiscoveryInfo"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(DiscoveryInfo other)
        {
            if (!ServerInfo.Equals(other.ServerInfo))
                return false;
            if (EndPoints.Length != other.EndPoints.Length)
                return false;

            for (var i = 0; i < EndPoints.Length; i++)
            {
                if (!EndPoints[i].Equals(other.EndPoints[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is DiscoveryInfo other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (ServerInfo.GetHashCode() * 397) ^ EndPoints.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{{\n{ServerInfo},\nEnd Points: {string.Join(", ", EndPoints.Select(e => e.ToString()))}\n}}";
        }
    }
}
