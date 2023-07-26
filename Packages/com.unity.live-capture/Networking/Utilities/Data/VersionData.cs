using System;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A struct used to send version information over the network.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VersionData : IEquatable<VersionData>
    {
        ushort m_Major;
        ushort m_Minor;
        ushort m_Build;
        ushort m_Revision;

        /// <summary>
        /// Gets the version information.
        /// </summary>
        /// <returns>A new version instance.</returns>
        public Version GetVersion() => new Version(m_Major, m_Minor, m_Build, m_Revision);

        /// <summary>
        /// Creates a new <see cref="VersionData"/> instance.
        /// </summary>
        /// <param name="version">The version to store.</param>
        /// <exception cref="ArgumentException"> Thrown if any number in <paramref name="version"/>
        /// exceeds <see cref="ushort.MaxValue"/>.</exception>
        public VersionData(Version version)
        {
            m_Major = (version.Major <= ushort.MaxValue) ? (ushort)version.Major : throw new ArgumentException($"Major version {version.Major} exceeds {ushort.MaxValue}!");
            m_Minor = (version.Minor <= ushort.MaxValue) ? (ushort)version.Minor : throw new ArgumentException($"Minor version {version.Minor} exceeds {ushort.MaxValue}!");
            m_Build = (version.Build <= ushort.MaxValue) ? (ushort)version.Build : throw new ArgumentException($"Build version {version.Build} exceeds {ushort.MaxValue}!");
            m_Revision = (version.Revision <= ushort.MaxValue) ? (ushort)version.Revision : throw new ArgumentException($"Revision version {version.Revision} exceeds {ushort.MaxValue}!");
        }

        /// <inheritdoc />
        public bool Equals(VersionData other)
        {
            return
                m_Major == other.m_Major &&
                m_Minor == other.m_Minor &&
                m_Build == other.m_Build &&
                m_Revision == other.m_Revision;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is VersionData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Major.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Minor.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Build.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Revision.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetVersion().ToString();
        }
    }
}
