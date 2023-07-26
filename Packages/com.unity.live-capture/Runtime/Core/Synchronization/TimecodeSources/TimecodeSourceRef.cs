using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that holds a serializable reference to a <see cref="ITimecodeSource"/>.
    /// </summary>
    [Serializable]
    public struct TimecodeSourceRef : IEquatable<TimecodeSourceRef>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The <see cref="TimecodeSourceManager"/> new references are registered to.
        /// </summary>
        internal static TimecodeSourceManager Manager { get; set; } = TimecodeSourceManager.Instance;

        [SerializeField]
        RegisteredRef<ITimecodeSource> m_Reference;

        /// <summary>
        /// Create a serializable reference to a <see cref="ITimecodeSource"/>.
        /// </summary>
        /// <param name="source">The source to reference.</param>
        public TimecodeSourceRef(ITimecodeSource source)
        {
            m_Reference = new RegisteredRef<ITimecodeSource>(source, Manager.Registry);
        }

        /// <summary>
        /// Receives a callback before Unity serializes your object.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Receives a callback after Unity deserializes your object.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // if the serialized registry doesn't exist, reset the reference
            if (m_Reference.Registry == null)
            {
                m_Reference.SetRegistry(Manager.Registry);
            }
        }

        /// <summary>
        /// Get the referenced source.
        /// </summary>
        /// <returns>The referenced source, or <see langword="null"/> if the source is not registered with the <see cref="TimecodeSourceManager"/>.</returns>
        public ITimecodeSource Resolve() => m_Reference.Resolve();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="TimecodeSourceRef"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public bool Equals(TimecodeSourceRef other) => m_Reference == other.m_Reference;

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="TimecodeSourceRef"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is TimecodeSourceRef other && Equals(other);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => m_Reference.GetHashCode();

        /// <summary>
        /// Determines whether two specified instances of <see cref="TimecodeSourceRef"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(TimecodeSourceRef a, TimecodeSourceRef b) => a.Equals(b);

        /// <summary>
        /// Determines whether two specified instances of <see cref="TimecodeSourceRef"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(TimecodeSourceRef a, TimecodeSourceRef b) => !(a == b);
    }
}
