using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A class that manages a set of <see cref="IRegistrable"/> instances which may be looked up by their ID string.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IRegistrable"/> indexed using this registry.</typeparam>
    class Registry<T> : IEnumerable<T> where T : class, IRegistrable
    {
        static readonly Dictionary<string, Registry<T>> s_NameToRegistry = new Dictionary<string, Registry<T>>();

        readonly Dictionary<string, T> m_Registry = new Dictionary<string, T>();

        /// <summary>
        /// The name of this registry.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Gets the list of registered <typeparamref name="T"/> instances.
        /// </summary>
        public IReadOnlyList<T> Entries => m_Registry.Values.ToList();

        /// <summary>
        /// An event triggered when a new <typeparamref name="T"/> instance is registered.
        /// </summary>
        public event Action<T> Added;

        /// <summary>
        /// An event triggered when a <typeparamref name="T"/> instance is unregistered.
        /// </summary>
        public event Action<T> Removed;

        /// <summary>
        /// Creates a new <see cref="Registry{T}"/> instance.
        /// </summary>
        /// <param name="name">
        /// The name used to identify this registry. The name must not be null or empty, and cannot match the name of any other
        /// registry of this type. It should also be stable across domain reloads, or else any serialized <see cref="RegisteredRef{T}"/>
        /// values will not correctly resolve the reference as they will not be able to find the registry.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="name"/> is the empty string.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is already used by another registry.</exception>
        public Registry(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentOutOfRangeException(nameof(name), name, "Cannot be empty.");
            if (s_NameToRegistry.ContainsKey(name))
                throw new ArgumentException($"A {nameof(Registry<T>)} with name \"{name}\" already exists.", nameof(name));

            Name = name;

            s_NameToRegistry.Add(name, this);
        }

        /// <summary>
        /// Gets the <typeparamref name="T"/> instance with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the <typeparamref name="T"/> to get.</param>
        /// <returns>The instance, or <see langword="null"/> if none with the given ID are registered.</returns>
        public T this[string id]
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                    return null;

                return m_Registry.TryGetValue(id, out var instance) ? instance : null;
            }
        }

        /// <summary>
        /// Adds a <typeparamref name="T"/> instance to the registry.
        /// </summary>
        /// <param name="instance">The instance to register.</param>
        /// <returns>
        /// <c>true</c> if the instance was registered successfully, <c>false</c> if <paramref name="instance"/> is <c>null</c>,
        /// the instance's ID is null or empty, or another instance was already registered with the same ID.
        /// </returns>
        public bool Register(T instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.Id) || m_Registry.ContainsKey(instance.Id))
                return false;

            m_Registry[instance.Id] = instance;
            OnAdded(instance);
            return true;
        }

        /// <summary>
        /// Removes a <typeparamref name="T"/> instance from the registry.
        /// </summary>
        /// <param name="instance">The instance to unregister.</param>
        /// <returns>
        /// <c>true</c> if the instance was unregistered successfully, <c>false</c> if <paramref name="instance"/> didn't exist in the registry.
        /// </returns>
        public bool Unregister(T instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.Id) || !m_Registry.Remove(instance.Id))
                return false;

            OnRemoved(instance);
            return true;
        }

        /// <summary>
        /// Unregister all instances in this registry.
        /// </summary>
        public void Clear()
        {
            var instances = m_Registry.Values.ToList();
            m_Registry.Clear();
            foreach (var instance in instances)
            {
                OnRemoved(instance);
            }
        }

        /// <summary>
        /// Creates a new ID if the given ID is uninitialized or is already used in the registry.
        /// </summary>
        /// <param name="id">The ID to initialize.</param>
        /// <returns><see langword="true"/> if a new ID was generated; <see langword="false"/> otherwise.</returns>
        public bool EnsureIdIsValid(ref string id)
        {
            if (string.IsNullOrEmpty(id) || m_Registry.ContainsKey(id))
            {
                id = Guid.NewGuid().ToString();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an enumerator that iterates over all entries in the registry.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<T> GetEnumerator() => m_Registry.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void OnAdded(T instance)
        {
            Added?.Invoke(instance);
        }

        void OnRemoved(T instance)
        {
            Removed?.Invoke(instance);
        }

        /// <summary>
        /// Gets a registry by name.
        /// </summary>
        /// <param name="name">The name of the registry to get.</param>
        /// <param name="registry">The returned registry, or <see langword="null"/> if not found.</param>
        /// <returns><see langword="true"/> if the registry was found, <see langword="false"/> otherwise.</returns>
        internal static bool TryGetRegistry(string name, out Registry<T> registry)
        {
            if (string.IsNullOrEmpty(name))
            {
                registry = null;
                return false;
            }

            return s_NameToRegistry.TryGetValue(name, out registry);
        }
    }
}
