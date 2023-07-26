using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A singleton class that keeps track of <see cref="ITimecodeSource"/> instances.
    /// </summary>
    public sealed class TimecodeSourceManager : IEnumerable<ITimecodeSource>
    {
        /// <summary>
        /// The <see cref="TimecodeSourceManager"/> instance.
        /// </summary>
        public static TimecodeSourceManager Instance { get; } = new TimecodeSourceManager(nameof(TimecodeSourceManager));

        /// <summary>
        /// The registry used to hold registered sources.
        /// </summary>
        internal Registry<ITimecodeSource> Registry { get; }

        /// <summary>
        /// Gets the list of registered sources.
        /// </summary>
        public IReadOnlyList<ITimecodeSource> Entries => Registry.Entries;

        /// <summary>
        /// An event triggered when a new source is registered.
        /// </summary>
        public event Action<ITimecodeSource> Added
        {
            add => Registry.Added += value;
            remove => Registry.Added -= value;
        }

        /// <summary>
        /// An event triggered when a source is unregistered.
        /// </summary>
        public event Action<ITimecodeSource> Removed
        {
            add => Registry.Removed += value;
            remove => Registry.Removed -= value;
        }

        /// <summary>
        /// Creates a new <see cref="TimecodeSourceManager"/> instance.
        /// </summary>
        /// <param name="name">The name used to identify this registry for this manager instance.</param>
        internal TimecodeSourceManager(string name)
        {
            Registry = new Registry<ITimecodeSource>(name);
        }

        /// <summary>
        /// Gets the <see cref="ITimecodeSource"/> with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the source to get.</param>
        /// <returns>
        /// The source, or <see langword="null"/> if none with the given ID are registered.
        /// </returns>
        public ITimecodeSource this[string id] => Registry[id];

        /// <summary>
        /// Adds a <see cref="ITimecodeSource"/> to the registry.
        /// </summary>
        /// <param name="source">The source to register.</param>
        /// <returns>
        /// <c>true</c> if the source was registered successfully, <c>false</c> if <paramref name="source"/> is <c>null</c>,
        /// the source's ID is null or empty, or another source was already registered with the same ID.
        /// </returns>
        public bool Register(ITimecodeSource source) => Registry.Register(source);

        /// <summary>
        /// Removes a <see cref="ITimecodeSource"/> from the registry.
        /// </summary>
        /// <param name="source">The source to unregister.</param>
        /// <returns>
        /// <c>true</c> if the source was unregistered successfully, <c>false</c> if <paramref name="source"/> didn't exist in the registry.
        /// </returns>
        public bool Unregister(ITimecodeSource source) => Registry.Unregister(source);

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
        public IEnumerator<ITimecodeSource> GetEnumerator() => Registry.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
