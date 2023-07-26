using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that holds a serializable reference to a <see cref="IRegistrable"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IRegistrable"/> this reference can hold.</typeparam>
    [Serializable]
    struct RegisteredRef<T> : IEquatable<RegisteredRef<T>> where T : class, IRegistrable
    {
        [SerializeField]
        string m_Id;
        [SerializeField]
        string m_Registry;

        /// <summary>
        /// Gets the registry this reference points to.
        /// </summary>
        public Registry<T> Registry
        {
            get
            {
                Registry<T>.TryGetRegistry(m_Registry, out var registry);
                return registry;
            }
        }

        /// <summary>
        /// Create a serializable reference to a <typeparamref name="T"/> instance.
        /// </summary>
        /// <param name="value">The <typeparamref name="T"/> instance to reference.</param>
        /// <param name="registry">The registry to look for <paramref name="value"/> in when resolving the reference.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="registry"/> is null.</exception>
        public RegisteredRef(T value, Registry<T> registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            m_Id = value != null ? value.Id : null;
            m_Registry = registry.Name;
        }

        /// <summary>
        /// Get the referenced instance.
        /// </summary>
        /// <returns>The referenced instance, or <see langword="null"/> if the instance is not registered with the <see cref="Registry{T}"/>.</returns>
        public T Resolve()
        {
            return Registry<T>.TryGetRegistry(m_Registry, out var registry) ? registry[m_Id] : null;
        }

        /// <summary>
        /// Sets the registry this reference points to.
        /// </summary>
        /// <param name="registry">The registry to look for the instance in when resolving the reference.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="registry"/> is null.</exception>
        public void SetRegistry(Registry<T> registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            m_Registry = registry.Name;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="RegisteredRef{T}"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RegisteredRef<T> other)
        {
            return (string.IsNullOrEmpty(m_Id) && string.IsNullOrEmpty(other.m_Id)) || m_Id == other.m_Id;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="RegisteredRef{T}"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is RegisteredRef<T> other && Equals(other);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => m_Id.GetHashCode();

        /// <summary>
        /// Determines whether two specified instances of <see cref="RegisteredRef{T}"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RegisteredRef<T> a, RegisteredRef<T> b) => a.Equals(b);

        /// <summary>
        /// Determines whether two specified instances of <see cref="RegisteredRef{T}"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RegisteredRef<T> a, RegisteredRef<T> b) => !(a == b);
    }
}
