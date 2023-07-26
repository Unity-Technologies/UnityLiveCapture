using System;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// A struct that contains information about a server instance.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    struct ServerData : IEquatable<ServerData>
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.StringMaxLength + 1)]
        string m_ProductName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.StringMaxLength + 1)]
        string m_InstanceName;

        Guid m_Id;
        VersionData m_Version;

        /// <summary>
        /// The name of the server application.
        /// </summary>
        public string ProductName => m_ProductName;

        /// <summary>
        /// The display name of the server instance.
        /// </summary>
        public string InstanceName => m_InstanceName;

        /// <summary>
        /// The unique identifier of the server instance.
        /// </summary>
        public Guid ID => m_Id;

        /// <summary>
        /// Gets the version of the server instance.
        /// </summary>
        /// <returns>A new version instance.</returns>
        public Version GetVersion() => m_Version.GetVersion();

        /// <summary>
        /// Creates a new <see cref="ServerData"/> instance.
        /// </summary>
        /// <param name="productName">The name of the server application.</param>
        /// <param name="instanceName">The display name of the server instance.</param>
        /// <param name="id">The unique identifier of the server instance.</param>
        /// <param name="version">The version of the server instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="productName"/>, <paramref name="instanceName"/>
        /// or <paramref name="version"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="productName"/> or <paramref name="instanceName"/>
        /// exceeds <see cref="Constants.StringMaxLength"/> characters in length.</exception>
        public ServerData(string productName, string instanceName, Guid id, Version version)
        {
            if (productName == null)
                throw new ArgumentNullException(nameof(productName));
            if (instanceName == null)
                throw new ArgumentNullException(nameof(instanceName));
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            if (productName.Length > Constants.StringMaxLength)
                throw new ArgumentException($"String length of {productName.Length} exceeds maximum ({Constants.StringMaxLength} characters).", nameof(productName));
            if (instanceName.Length > Constants.StringMaxLength)
                throw new ArgumentException($"String length of {instanceName.Length} exceeds maximum ({Constants.StringMaxLength} characters).", nameof(instanceName));

            m_ProductName = productName;
            m_InstanceName = instanceName;
            m_Id = id;
            m_Version = new VersionData(version);
        }

        /// <summary>
        /// Determines whether the <see cref="ServerData"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="ServerData"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ServerData other)
        {
            return
                m_ProductName == other.m_ProductName &&
                m_InstanceName == other.m_InstanceName &&
                m_Id.Equals(other.m_Id) &&
                m_Version.Equals(other.m_Version);
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is ServerData other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for this ServerData.
        /// </summary>
        /// <returns>
        /// The hash value generated for this ServerData.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_ProductName.GetHashCode();
                hashCode = (hashCode * 397) ^ m_InstanceName.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Id.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Version.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Product Name: {m_ProductName},\nInstance Name: {m_InstanceName},\nInstance ID: {m_Id},\nVersion: {m_Version}";
        }
    }
}
