using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A bidirectional one-to-one mapping between two sets of values.
    /// </summary>
    /// <typeparam name="T0">The type of value in the first set.</typeparam>
    /// <typeparam name="T1">The type of value in the second set.</typeparam>
    class Bijection<T0, T1> : ICollection<KeyValuePair<T0, T1>>
    {
        /// <summary>
        /// Maps one direction of the bijection.
        /// </summary>
        /// <typeparam name="T2">The type of value to map from.</typeparam>
        /// <typeparam name="T3">The type of value to map to.</typeparam>
        public class Indexer<T2, T3>
        {
            readonly Dictionary<T2, T3> m_Dictionary;

            /// <summary>
            /// Creates a new <see cref="Indexer{T2, T3}"/> instance.
            /// </summary>
            /// <param name="dictionary">The dictionary instance defining the mapping.</param>
            public Indexer(Dictionary<T2, T3> dictionary)
            {
                if (dictionary == null)
                    throw new ArgumentNullException(nameof(dictionary));

                m_Dictionary = dictionary;
            }

            /// <summary>
            /// Gets a mapped value.
            /// </summary>
            /// <param name="key">The value to map from.</param>
            /// <returns>The mapped value.</returns>
            public T3 this[T2 key]
            {
                get => m_Dictionary[key];
                set => m_Dictionary[key] = value;
            }

            /// <summary>
            /// Gets a mapped value.
            /// </summary>
            /// <param name="key">The value to map from.</param>
            /// <param name="value">Returns the mapped value.</param>
            /// <returns>True if the key has a mapping defined; false otherwise.</returns>
            public bool TryGetValue(T2 key, out T3 value) => m_Dictionary.TryGetValue(key, out value);
        }

        readonly Dictionary<T0, T1> m_Forward = new Dictionary<T0, T1>();
        readonly Dictionary<T1, T0> m_Reverse = new Dictionary<T1, T0>();

        /// <summary>
        /// The mapping from <typeparamref name="T0"/> values to <typeparamref name="T1"/> values.
        /// </summary>
        public Indexer<T0, T1> Forward { get; }

        /// <summary>
        /// The mapping from <typeparamref name="T1"/> values to <typeparamref name="T0"/> values.
        /// </summary>
        public Indexer<T1, T0> Reverse { get; }

        /// <summary>
        /// The number of mappings in the bijection.
        /// </summary>
        public int Count => m_Forward.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Creates a new <see cref="Bijection{T1, T2}"/> instance.
        /// </summary>
        public Bijection()
        {
            Forward = new Indexer<T0, T1>(m_Forward);
            Reverse = new Indexer<T1, T0>(m_Reverse);
        }

        /// <summary>
        /// Checks if a value has a mapping defined.
        /// </summary>
        /// <param name="key">The value to check.</param>
        /// <returns>True if the value is mapped; false otherwise.</returns>
        public bool ContainsKey(T0 key)
        {
            return key != null && m_Forward.ContainsKey(key);
        }

        /// <summary>
        /// Checks if a value has a mapping defined.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is mapped; false otherwise.</returns>
        public bool ContainsValue(T1 value)
        {
            return value != null && m_Reverse.ContainsKey(value);
        }

        /// <summary>
        /// Checks if the bijection defines a given mapping.
        /// </summary>
        /// <param name="key">The first value in the mapping.</param>
        /// <param name="value">The second value in the mapping.</param>
        /// <returns>True if the mapping exists; false otherwise.</returns>
        public bool Contains(T0 key, T1 value)
        {
            return key != null && value != null && m_Forward.TryGetValue(key, out var mapped) && value.Equals(mapped);
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<T0, T1> item) => Contains(item.Key, item.Value);

        /// <summary>
        /// Adds a new mapping to the bijection.
        /// </summary>
        /// <param name="key">The first value.</param>
        /// <param name="value">The second value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or
        /// <paramref name="value"/> are null.</exception>
        /// <exception cref="ArgumentException">Thrown if the bijection already contains a mapping for
        /// <paramref name="key"/> or <paramref name="value"/>.</exception>
        public void Add(T0 key, T1 value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (m_Forward.ContainsKey(key))
                throw new ArgumentException($"Map already contains entry for {key}!", nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (m_Reverse.ContainsKey(value))
                throw new ArgumentException($"Map already contains entry for {value}!", nameof(value));

            m_Forward.Add(key, value);
            m_Reverse.Add(value, key);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<T0, T1> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Removes the mapping for a value from the bijection.
        /// </summary>
        /// <param name="key">The value to remove the mapping for.</param>
        /// <returns>True if a mapping was removed; false otherwise.</returns>
        public bool RemoveKey(T0 key)
        {
            if (key != null && m_Forward.TryGetValue(key, out var v1))
            {
                m_Forward.Remove(key);
                m_Reverse.Remove(v1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the mapping for a value from the bijection.
        /// </summary>
        /// <param name="value">The value to remove the mapping for.</param>
        /// <returns>True if a mapping was removed; false otherwise.</returns>
        public bool RemoveValue(T1 value)
        {
            if (value != null && m_Reverse.TryGetValue(value, out var v0))
            {
                m_Forward.Remove(v0);
                m_Reverse.Remove(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a mapping from the bijection.
        /// </summary>
        /// <param name="key">The first value in the mapping.</param>
        /// <param name="value">The second value in the mapping.</param>
        /// <returns>True if the mapping was removed; false otherwise.</returns>
        public bool Remove(T0 key, T1 value)
        {
            if (Contains(key, value))
            {
                m_Forward.Remove(key);
                m_Reverse.Remove(value);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<T0, T1> item) => Remove(item.Key, item.Value);

        /// <summary>
        /// Removes all entries from the bijection.
        /// </summary>
        public void Clear()
        {
            m_Forward.Clear();
            m_Reverse.Clear();
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<T0, T1>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Must be non-negative!");
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("The number of elements in the collection is greater than the available space from the index to the end of the destination array.");

            foreach (var mapping in m_Forward)
            {
                array[arrayIndex++] = mapping;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<T0, T1>> GetEnumerator() => m_Forward.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => m_Forward.GetEnumerator();
    }
}
