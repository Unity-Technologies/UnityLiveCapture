using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a shot. A shot stores information about the take to play, the slate used, and the
    /// output directory. The <see cref="TakeRecorder"/> uses that information to name and store the
    /// recorded takes in the correct directory.
    /// </summary>
    [Serializable]
    public struct Shot : IEquatable<Shot>
    {
        [SerializeField]
        double m_TimeOffset;
        [SerializeField]
        double m_Duration;
        [SerializeField]
        string m_Directory;
        [SerializeField]
        int m_SceneNumber;
        [SerializeField]
        string m_Name;
        [SerializeField]
        int m_TakeNumber;
        [SerializeField]
        string m_Description;
        [SerializeField]
        Take m_Take;
        [SerializeField]
        Take m_IterationBase;

        /// <summary>
        /// The time, in seconds, to add to the contents of a recording.
        /// </summary>
        public double TimeOffset
        {
            get => m_TimeOffset;
            set => m_TimeOffset = value;
        }

        /// <summary>
        /// The duration of the shot in seconds.
        /// </summary>
        public double Duration
        {
            get => m_Duration;
            set => m_Duration = value;
        }

        /// <summary>
        /// The file path containing the recorded takes.
        /// </summary>
        public string Directory
        {
            get => m_Directory;
            set => m_Directory = value;
        }

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
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
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
        /// The slate associated with the shot to record.
        /// </summary>
        public Slate Slate
        {
            get => new Slate()
            {
                ShotName = m_Name,
                SceneNumber = m_SceneNumber,
                TakeNumber = m_TakeNumber,
                Description = m_Description
            };
            set
            {
                m_Name = value.ShotName;
                m_SceneNumber = value.SceneNumber;
                m_TakeNumber = value.TakeNumber;
                m_Description = value.Description;
            }
        }

        /// <summary>
        /// The selected take of the slate.
        /// </summary>
        public Take Take
        {
            get => m_Take;
            set => m_Take = value;
        }

        /// <summary>
        /// The take to iterate from in the next recording.
        /// </summary>
        public Take IterationBase
        {
            get => m_IterationBase;
            set => m_IterationBase = value;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"[{m_SceneNumber}] {m_Name} [{m_TakeNumber}]";
        }

        /// <summary>
        /// Determines whether the specified shot is equal to the current one.
        /// </summary>
        /// <param name="other">The shot specified to compare with the current one.</param>
        /// <returns>
        /// true if the specified shot is equal to the current one; otherwise, false.
        /// </returns>
        public bool Equals(Shot other)
        {
            return m_SceneNumber == other.m_SceneNumber
                   && m_Description == other.m_Description
                   && m_Name == other.m_Name
                   && m_TakeNumber == other.m_TakeNumber
                   && m_Directory == other.m_Directory
                   && m_Duration == other.m_Duration
                   && m_IterationBase == other.m_IterationBase
                   && m_Take == other.m_Take
                   && m_TimeOffset == other.m_TimeOffset;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current shot.
        /// </summary>
        /// <param name="obj">The object specified to compare with the current shot.</param>
        /// <returns>
        /// true if the specified object is equal to the current shot; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Shot other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the shot.
        /// </summary>
        /// <returns>
        /// The hash value generated for this shot.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_SceneNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Name.GetHashCode();
                hashCode = (hashCode * 397) ^ m_TakeNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Description.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Directory.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Duration.GetHashCode();
                hashCode = (hashCode * 397) ^ m_IterationBase.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Take.GetHashCode();
                hashCode = (hashCode * 397) ^ m_TimeOffset.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified shots are equal.
        /// </summary>
        /// <param name="a">The first shot.</param>
        /// <param name="b">The second Shot.</param>
        /// <returns>
        /// true if the specified shots are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(Shot a, Shot b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified shots are different.
        /// </summary>
        /// <param name="a">The first shot.</param>
        /// <param name="b">The second shot.</param>
        /// <returns>
        /// true if the specified shots are different; otherwise, false.
        /// </returns>
        public static bool operator !=(Shot a, Shot b)
        {
            return !(a == b);
        }
    }
}
