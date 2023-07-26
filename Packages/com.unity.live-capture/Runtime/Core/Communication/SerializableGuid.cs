using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a serializable globally unique identifier (GUID).
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    struct SerializableGuid : IEquatable<SerializableGuid>
    {
        [FieldOffset(0)]
        Guid m_Guid;

        [SerializeField]
        [FieldOffset(0)]
        long m_Part0;

        [SerializeField]
        [FieldOffset(8)]
        long m_Part1;

        /// <summary>
        /// Initializes a new instance of the SerializableGuid class by using the value represented by the specified string.
        /// </summary>
        /// <param name="guid">A string representing a GUID.</param>
        /// <returns>A new instance of the SerializableGuid class.</returns>
        public static SerializableGuid FromString(string guid)
        {
            return new SerializableGuid()
            {
                m_Guid = Guid.Parse(guid)
            };
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>
        /// The hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return m_Guid.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return m_Guid.ToString("N");
        }

        /// <summary>
        /// Determines whether the <see cref="SerializableGuid"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="SerializableGuid"/> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(SerializableGuid other)
        {
            return m_Guid.Equals(other.m_Guid);
        }

        /// <summary>
        /// Converts a guid from <see cref="SerializableGuid"/> to System.Guid.
        /// </summary>
        /// <param name="guid">The value to convert.</param>
        /// <returns>The guid represented as a System.Guid.</returns>
        public static implicit operator Guid(SerializableGuid guid) => guid.m_Guid;

        /// <summary>
        /// Converts a guid from System.Guid to <see cref="SerializableGuid"/>.
        /// </summary>
        /// <param name="guid">The value to convert.</param>
        /// <returns>The guid represented as a <see cref="SerializableGuid"/>.</returns>
        public static implicit operator SerializableGuid(Guid guid) => new SerializableGuid() { m_Guid = guid };
    }
}
