using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A class which may be used to manage the timecode updates for an <see cref="ITimecodeSource"/>.
    /// </summary>
    /// <remarks>
    /// This handles updating the timecode at the best point in the update loop, and ensures
    /// that the timecode source is genlocked when an <see cref="ISyncProvider"/> is active.
    /// </remarks>
    public class TimecodeSourceState : IDisposable
    {
        static readonly List<TimecodeSourceState> s_Sources = new List<TimecodeSourceState>();

        struct TimecodeSourceUpdate
        {
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!playerLoop.TryFindSubSystem<TimeUpdate>(out var system))
            {
                return;
            }

            var index = system.IndexOf<TimeUpdate.WaitForLastPresentationAndUpdateTime>();

            if (index < 0)
            {
                return;
            }

            system.AddSubSystem<TimecodeSourceUpdate>(index + 1, UpdateTimecodeSources);

            if (!playerLoop.TryUpdate(system))
            {
                return;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void UpdateTimecodeSources()
        {
            foreach (var source in s_Sources)
            {
                source.Update();
            }
        }

        readonly Func<FrameTimeWithRate?> m_PollFrameTime;
        FrameTimeWithRate? m_LastPolledFrameTime;
        FrameTimeWithRate? m_LastFrameTime;

        /// <summary>
        /// The frame time and frame rate to use for the current frame.
        /// </summary>
        public FrameTimeWithRate? CurrentTime => m_LastFrameTime;

        /// <summary>
        /// Creates a new <see cref="TimecodeSourceState"/> instance.
        /// </summary>
        /// <param name="pollFrameTime">A function that polls the timecode source at the start of the frame for the latest timecode.
        /// Return <see langword="null"/> if there is no timecode available.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pollFrameTime"/> is <see langword="null"/>.</exception>
        public TimecodeSourceState(Func<FrameTimeWithRate?> pollFrameTime)
        {
            m_PollFrameTime = pollFrameTime ?? throw new ArgumentNullException(nameof(pollFrameTime));

            s_Sources.Add(this);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            s_Sources.Remove(this);
        }

        void Update()
        {
            var frameTime = m_PollFrameTime();

            // If a new timecode is received from the source, use it.
            if (frameTime != null && frameTime != m_LastPolledFrameTime)
            {
                m_LastPolledFrameTime = frameTime;
                m_LastFrameTime = frameTime;
                return;
            }

            // If a new timecode is not received over the last frame, progress time based on the current
            // sync provider instead.
            if (m_LastFrameTime == null)
            {
                return;
            }

            var syncProvider = SyncManager.Instance.ActiveSyncProvider;

            if (syncProvider == null ||
                syncProvider.Status != SyncStatus.Synchronized ||
                syncProvider.LastPulseCountDelta == null ||
                !syncProvider.SyncRate.IsValid)
            {
                return;
            }

            m_LastFrameTime += new FrameTimeWithRate(syncProvider.SyncRate, new FrameTime(syncProvider.LastPulseCountDelta.Value));
        }
    }
}
