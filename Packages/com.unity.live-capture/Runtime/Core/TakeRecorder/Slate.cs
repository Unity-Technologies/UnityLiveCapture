using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a slate. A slate stores information about the shot.
    /// </summary>
    [Serializable]
    public struct Slate : IEquatable<Slate>
    {
        internal static readonly Slate Empty = new Slate()
        {
            ShotName = string.Empty,
            Description = string.Empty
        };

        [SerializeField]
        int m_SceneNumber;
        [SerializeField]
        string m_ShotName;
        [SerializeField]
        int m_TakeNumber;
        [SerializeField]
        string m_Description;

        /// <summary>
        /// The number associated with the scene to record.
        /// </summary>
        public int SceneNumber
        {
            get => m_SceneNumber;
            set => m_SceneNumber = value;
        }

        /// <summary>
        /// The name of the shot stored in the slate.
        /// </summary>
        /// <remarks>
        /// The recorded takes automatically inherit from this name.
        /// </remarks>
        public string ShotName
        {
            get => m_ShotName;
            set => m_ShotName = value;
        }

        /// <summary>
        /// The number associated with the take to record.
        /// </summary>
        /// <remarks>
        /// The number increments after recording a take.
        /// </remarks>
        public int TakeNumber
        {
            get => m_TakeNumber;
            set => m_TakeNumber = value;
        }

        /// <summary>
        /// The description of the shot stored in the slate.
        /// </summary>
        public string Description
        {
            get => m_Description;
            set => m_Description = value;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"[{m_SceneNumber}] {m_ShotName} [{m_TakeNumber}]";
        }

        /// <inheritdoc/>
        public bool Equals(Slate other)
        {
            return m_SceneNumber == other.m_SceneNumber
                   && m_Description == other.m_Description
                   && m_ShotName == other.m_ShotName
                   && m_TakeNumber == other.m_TakeNumber;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current Slate.
        /// </summary>
        /// <param name="obj">The object to compare with the current Slate.</param>
        /// <returns>
        /// true if the specified object is equal to the current Slate; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Slate other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the Slate.
        /// </summary>
        /// <returns>
        /// The hash value generated for this Slate.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_SceneNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ m_ShotName.GetHashCode();
                hashCode = (hashCode * 397) ^ m_TakeNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Description.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified Slates are equal.
        /// </summary>
        /// <param name="a">The first Slate.</param>
        /// <param name="b">The second Slate.</param>
        /// <returns>
        /// true if the specified Slates are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(Slate a, Slate b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified Slates are different.
        /// </summary>
        /// <param name="a">The first Slate.</param>
        /// <param name="b">The second Slate.</param>
        /// <returns>
        /// true if the specified Slates are different; otherwise, false.
        /// </returns>
        public static bool operator !=(Slate a, Slate b)
        {
            return !(a == b);
        }
    }
}
