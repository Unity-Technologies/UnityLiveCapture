using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A base class that includes functionality suitable for most <see cref="ISyncProvider"/> implementations.
    /// </summary>
    [Serializable]
    public abstract class SyncProvider : ISyncProvider
    {
        [NonSerialized]
        float? m_LastCaptureDeltaTime;
        [NonSerialized]
        bool m_HasShownSyncRateWarning;
        [NonSerialized]
        bool m_HasShownVSyncWarning;
        [NonSerialized]
        bool m_HasShownCaptureTimeWarning;

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public virtual FrameRate SyncRate => TakeRecorder.FrameRate;

        /// <inheritdoc />
        [field: NonSerialized]
        public SyncStatus Status { get; protected set; } = SyncStatus.Stopped;

        /// <inheritdoc />
        [field: NonSerialized]
        public int? LastPulseCountDelta { get; private set; }

        /// <inheritdoc />
        [field: NonSerialized]
        public int DroppedFrameCount { get; private set; }

        /// <summary>
        /// Determines if this synchronization provider can synchronize while the Editor is in edit mode.
        /// </summary>
        protected virtual bool RunInEditMode => false;

        /// <inheritdoc />
        void ISyncProvider.StartSynchronizing()
        {
            if (Status == SyncStatus.Stopped && (Application.isPlaying || RunInEditMode))
            {
                Status = SyncStatus.Synchronized;
                LastPulseCountDelta = null;
                DroppedFrameCount = 0;
                m_LastCaptureDeltaTime = null;

                ResetWarnings();
                OnStart();
            }
        }

        /// <inheritdoc />
        void ISyncProvider.StopSynchronizing()
        {
            if (Status != SyncStatus.Stopped)
            {
                Status = SyncStatus.Stopped;
                LastPulseCountDelta = null;
                DroppedFrameCount = 0;

                if (m_LastCaptureDeltaTime != null)
                {
                    Time.captureDeltaTime = 0;
                    m_LastCaptureDeltaTime = null;
                }

                OnStop();
            }
        }

        /// <inheritdoc />
        bool ISyncProvider.WaitForNextPulse()
        {
            if (Status == SyncStatus.Stopped)
            {
                return false;
            }

            if (!CanSync() || !OnWaitForNextPulse(out var pulseCount) || pulseCount < 0)
            {
                Status = SyncStatus.NotSynchronized;
                LastPulseCountDelta = null;

                if (m_LastCaptureDeltaTime != null)
                {
                    Time.captureDeltaTime = 0;
                    m_LastCaptureDeltaTime = null;
                }

                return false;
            }

            // If multiple pulses have occurred since the last update, then at least one pulse was skipped over, meaning a
            // frame was dropped. Only check for dropped frames if the provider was synchronized last frame.
            if (Status == SyncStatus.Synchronized && pulseCount > 1)
            {
                var droppedFrames = pulseCount - 1;
                DroppedFrameCount += droppedFrames;
                Debug.LogWarning($"Dropped {droppedFrames} frame{(droppedFrames != 1 ? "s" : string.Empty)}.");
            }

            Status = SyncStatus.Synchronized;
            LastPulseCountDelta = pulseCount;
            ResetWarnings();

            // The engine time should progress based on the synchronization signal rate and pulse count, not the platform time.
            // This helps make synchronized data feeds have better temporal coherence with other engine systems. To achieve
            // this, you can use Time.captureDeltaTime to control how much Time.time increments the next frame. However,
            // other scripts may also use Time.captureDeltaTime, so you must be careful to ensure it doesn't unexpectedly change.
            var deltaTime = pulseCount * (float)SyncRate.FrameInterval;

            Time.captureDeltaTime = deltaTime;
            m_LastCaptureDeltaTime = deltaTime;
            return true;
        }

        /// <summary>
        /// Called when the sync provider becomes active.
        /// </summary>
        protected virtual void OnStart()
        {
            // ensures the player loop runs as fast as possible.
            Application.targetFrameRate = -1;
        }

        /// <summary>
        /// Called when the sync provider becomes inactive.
        /// </summary>
        protected virtual void OnStop() { }

        /// <summary>
        /// Blocks execution on the current thread until the next synchronization signal pulse is received.
        /// </summary>
        /// <param name="pulseCount">The number of synchronization signal pulses since this method was last called.</param>
        /// <returns><see langword="true"/> when a pulse was successfully received; otherwise, <see langword="false"/>.</returns>
        protected abstract bool OnWaitForNextPulse(out int pulseCount);

        bool CanSync()
        {
            var canSync = true;

            if (!SyncRate.IsValid || SyncRate.Numerator <= 0)
            {
                canSync = false;
                LogWarning(
                    $"Genlock source \"{Name}\" has an invalid sync rate ({SyncRate}). Specify a valid sync rate to enable genlock.",
                    ref m_HasShownSyncRateWarning
                );
            }
            if (QualitySettings.vSyncCount != 0)
            {
                canSync = false;
                LogWarning(
                    $"A genlock source is active, but VSync is enabled. Disable VSync in the Quality Settings to enable genlock.",
                    ref m_HasShownVSyncWarning
                );
            }
            if (m_LastCaptureDeltaTime != null && m_LastCaptureDeltaTime.Value != Time.captureDeltaTime)
            {
                canSync = false;
                LogWarning(
                    $"Genlock sources require exclusive control of Time.captureDeltaTime, but it is being set by another script.",
                    ref m_HasShownCaptureTimeWarning
                );
            }

            return canSync;
        }

        void ResetWarnings()
        {
            m_HasShownSyncRateWarning = false;
            m_HasShownVSyncWarning = false;
            m_HasShownCaptureTimeWarning = false;
        }

        static void LogWarning(string warning, ref bool hasShownWarning)
        {
            if (!hasShownWarning)
            {
                Debug.LogWarning(warning);
                hasShownWarning = true;
            }
        }
    }
}
