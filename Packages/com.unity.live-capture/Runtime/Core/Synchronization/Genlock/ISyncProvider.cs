using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An interface for synchronization sources which may be used to control the engine update timing.
    /// </summary>
    /// <remarks>
    /// Classes implementing this interface must have the <c>System.SerializableAttribute</c> and
    /// <see cref="SyncProviderAttribute"/> to be assignable from the Live Capture project settings.
    /// </remarks>
    public interface ISyncProvider
    {
        /// <summary>
        /// The display name of the synchronization provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The pulse rate of the synchronization signal.
        /// </summary>
        FrameRate SyncRate { get; }

        /// <summary>
        /// The status of the synchronization provider.
        /// </summary>
        SyncStatus Status { get; }

        /// <summary>
        /// The number of synchronization signal pulses between the last two <see cref="WaitForNextPulse"/> calls.
        /// </summary>
        int? LastPulseCountDelta { get; }

        /// <summary>
        /// The number of synchronization signal pulses that have been skipped since starting the synchronization provider.
        /// </summary>
        int DroppedFrameCount { get; }

        /// <summary>
        /// Begin listening for synchronization pulses.
        /// </summary>
        void StartSynchronizing();

        /// <summary>
        /// Stop listening for synchronization pulses.
        /// </summary>
        void StopSynchronizing();

        /// <summary>
        /// Blocks execution on the current thread until the next synchronization signal pulse is received.
        /// </summary>
        /// <remarks>
        /// This is expected to be called once per frame.
        /// </remarks>
        /// <returns><see langword="true"/> when a pulse was successfully received; otherwise, <see langword="false"/>.</returns>
        bool WaitForNextPulse();
    }
}
