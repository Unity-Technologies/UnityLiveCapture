using System;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A handle that indentifies a property created in a <see cref="LiveStream"/>.
    /// </summary>
    public struct LivePropertyHandle : IEquatable<LivePropertyHandle>
    {
        static int s_NextId;

        int m_Id;

        internal static LivePropertyHandle Create()
        {
            unchecked
            {
                s_NextId++;
            }

            return new LivePropertyHandle()
            {
                m_Id = s_NextId
            };
        }

        /// <summary>
        /// Determines whether the <see cref="LivePropertyHandle"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="LivePropertyHandle"/> to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>

        public bool Equals(LivePropertyHandle other)
        {
            return m_Id == other.m_Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="LivePropertyHandle"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="LivePropertyHandle"/>.</param>
        /// <returns>
        /// true if the specified object is equal to the current <see cref="LivePropertyHandle"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is LivePropertyHandle other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the <see cref="LivePropertyHandle"/>.
        /// </summary>
        /// <returns>
        /// The hash value generated for this <see cref="LivePropertyHandle"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return m_Id.GetHashCode();
        }

        /// <summary>
        /// Determines whether the two specified <see cref="LivePropertyHandle"/> are equal.
        /// </summary>
        /// <param name="a">The first <see cref="LivePropertyHandle"/>.</param>
        /// <param name="b">The second <see cref="LivePropertyHandle"/>.</param>
        /// <returns>
        /// true if the specified <see cref="LivePropertyHandle"/> are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(LivePropertyHandle a, LivePropertyHandle b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified <see cref="LivePropertyHandle"/> are different.
        /// </summary>
        /// <param name="a">The first <see cref="LivePropertyHandle"/>.</param>
        /// <param name="b">The second <see cref="LivePropertyHandle"/>.</param>
        /// <returns>
        /// true if the specified <see cref="LivePropertyHandle"/> are different; otherwise, false.
        /// </returns>
        public static bool operator !=(LivePropertyHandle a, LivePropertyHandle b)
        {
            return !(a == b);
        }
    }
}
