using System;
using System.Net;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A handle to a remote server or client.
    /// </summary>
    class Remote : IEquatable<Remote>
    {
        /// <summary>
        /// The handle that represents all remotes.
        /// </summary>
        public static Remote All { get; } = new Remote(Guid.Parse("ffffffffffffffffffffffffffffffff"), null, null);

        /// <summary>
        /// The ID of the <see cref="NetworkBase"/> represented by this handle.
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// The ip address and port the remote uses for reliable communication.
        /// </summary>
        public IPEndPoint TcpEndPoint { get; }

        /// <summary>
        /// The ip address and port the remote uses for unreliable communication.
        /// </summary>
        public IPEndPoint UdpEndPoint { get; }

        /// <summary>
        /// Creates a new <see cref="Remote"/> handle instance.
        /// </summary>
        /// <param name="id">The ID of the <see cref="NetworkBase"/> instance to represent.</param>
        /// <param name="tcpEndPoint">The ip address and port the remote uses for reliable communication.</param>
        /// <param name="udpEndPoint">The ip address and port the remote uses for unreliable communication.</param>
        internal Remote(Guid id, IPEndPoint tcpEndPoint, IPEndPoint udpEndPoint)
        {
            ID = id;
            TcpEndPoint = tcpEndPoint;
            UdpEndPoint = udpEndPoint;
        }

        /// <summary>
        /// Determines whether the <see cref="Remote"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Remote"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Remote other) => !(other is null) && ID == other.ID;

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj) => obj is Remote other && Equals(other);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => ID.GetHashCode();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            // truncate the GUID to keep it a readable length
            return this == All ? "All" : $"{ID.ToString("N").Substring(0, 8)}";
        }

        /// <summary>
        /// Determines whether two remote handles are equivalent.
        /// </summary>
        /// <param name="a">The first remote.</param>
        /// <param name="b">The second remote.</param>
        /// <returns>True if the remotes are equivalent; false otherwise.</returns>
        public static bool operator ==(Remote a, Remote b) => ReferenceEquals(a, b) || (!(a is null) && a.Equals(b));

        /// <summary>
        /// Determines whether two remotes handles are not equivalent.
        /// </summary>
        /// <param name="a">The first remote.</param>
        /// <param name="b">The second remote.</param>
        /// <returns>True if the remotes are not equivalent; false otherwise.</returns>
        public static bool operator !=(Remote a, Remote b) => !(a == b);
    }
}
