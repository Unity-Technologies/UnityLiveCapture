using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A singleton class that keeps track of <see cref="ITimedDataSource"/> instances.
    /// </summary>
    public sealed class TimedDataSourceManager : IEnumerable<ITimedDataSource>
    {
        /// <summary>
        /// The <see cref="TimedDataSourceManager"/> instance.
        /// </summary>
        public static TimedDataSourceManager Instance { get; } = new TimedDataSourceManager(nameof(TimedDataSourceManager));

        /// <summary>
        /// The registry used to hold registered sources.
        /// </summary>
        internal Registry<ITimedDataSource> Registry { get; }

        /// <summary>
        /// Gets the list of registered sources.
        /// </summary>
        public IReadOnlyList<ITimedDataSource> Entries => Registry.Entries;

        /// <summary>
        /// An event triggered when a new source is registered.
        /// </summary>
        public event Action<ITimedDataSource> Added
        {
            add => Registry.Added += value;
            remove => Registry.Added -= value;
        }

        /// <summary>
        /// An event triggered when a source is unregistered.
        /// </summary>
        public event Action<ITimedDataSource> Removed
        {
            add => Registry.Removed += value;
            remove => Registry.Removed -= value;
        }

        /// <summary>
        /// Creates a new <see cref="TimedDataSourceManager"/> instance.
        /// </summary>
        /// <param name="name">The name used to identify this registry for this manager instance.</param>
        internal TimedDataSourceManager(string name)
        {
            Registry = new Registry<ITimedDataSource>(name);
        }

        /// <summary>
        /// Gets the <see cref="ITimedDataSource"/> with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the source to get.</param>
        /// <returns>
        /// The source, or <see langword="null"/> if none with the given ID are registered.
        /// </returns>
        public ITimedDataSource this[string id] => Registry[id];

        /// <summary>
        /// Adds a <see cref="ITimedDataSource"/> to the registry.
        /// </summary>
        /// <param name="source">The source to register.</param>
        /// <returns>
        /// <c>true</c> if the source was registered successfully, <c>false</c> if <paramref name="source"/> is <c>null</c>,
        /// the source's ID is null or empty, or another source was already registered with the same ID.
        /// </returns>
        public bool Register(ITimedDataSource source) => Registry.Register(source);

        /// <summary>
        /// Removes a <see cref="ITimedDataSource"/> from the registry.
        /// </summary>
        /// <param name="source">The source to unregister.</param>
        /// <returns>
        /// <c>true</c> if the source was unregistered successfully, <c>false</c> if <paramref name="source"/> didn't exist in the registry.
        /// </returns>
        public bool Unregister(ITimedDataSource source) => Registry.Unregister(source);

        /// <summary>
        /// Unregister all sources.
        /// </summary>
        public void Clear() => Registry.Clear();

        /// <summary>
        /// Creates a new ID if the given ID is uninitialized or is already used.
        /// </summary>
        /// <param name="id">The ID to initialize.</param>
        /// <returns><see langword="true"/> if a new ID was generated; <see langword="false"/> otherwise.</returns>
        public bool EnsureIdIsValid(ref string id) => Registry.EnsureIdIsValid(ref id);

        /// <summary>
        /// Gets an enumerator that iterates over all registered sources.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<ITimedDataSource> GetEnumerator() => Registry.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
