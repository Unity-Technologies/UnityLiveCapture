using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.PlayerLoop;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An empty struct that defines the custom player loop update used for synchronization.
    /// </summary>
    /// <remarks>
    /// The synchronizer update takes place immediately following Update, before
    /// the animation system update.
    /// </remarks>
    internal struct SynchronizerUpdate
    {
    }

    /// <summary>
    /// A component used to synchronize playback of content from <see cref="ITimedDataSource"/> instances.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Live Capture/Timecode Synchronizer")]
    [HelpURL(Documentation.baseURL + "ref-component-timecode-synchronizer" + Documentation.endURL)]
    public class SynchronizerComponent : MonoBehaviour
    {
        static readonly List<SynchronizerComponent> s_SynchronizerComponents = new List<SynchronizerComponent>();

        /// <summary>
        /// The currently enabled synchronizer instances.
        /// </summary>
        internal static IReadOnlyList<SynchronizerComponent> Synchronizers => s_SynchronizerComponents;

        /// <summary>
        /// An event invoked when the <see cref="Synchronizers"/> list has changed.
        /// </summary>
        internal static event Action SynchronizersChanged;

        [SerializeField]
        Synchronizer m_Impl = new Synchronizer();

        Coroutine m_CalibrationCoro;

        /// <summary>
        /// The synchronizer instance.
        /// </summary>
        internal Synchronizer Impl => m_Impl;

        /// <summary>
        /// The synchronizer instance.
        /// </summary>
        public ISynchronizer Synchronizer => m_Impl;

#if UNITY_EDITOR
        PlayableGraph m_DummyPlayableGraph;
#endif

        void OnEnable()
        {
            // Do the update prior to the animation update in PreLateUpdate. This is necessary to avoid adding a frame of
            // latency with live capture devices that drive the actors via the playables graph.
            PlayerLoopExtensions.RegisterUpdate<PreLateUpdate, SynchronizerUpdate>(OnSynchronizerUpdate, 0);

#if UNITY_EDITOR
            // Playing a PlayableGraph tricks the Editor into keeping the update loop
            // alive.
            m_DummyPlayableGraph = PlayableGraph.Create("Synchronizer Editor Update");
            ScriptPlayableOutput.Create(m_DummyPlayableGraph, "output");
            m_DummyPlayableGraph.Play();
#endif

            s_SynchronizerComponents.Add(this);
            SynchronizersChanged?.Invoke();
        }

        void OnDisable()
        {
            s_SynchronizerComponents.Remove(this);
            SynchronizersChanged?.Invoke();

            StopCalibration();
            Impl.Pause();
            PlayerLoopExtensions.DeregisterUpdate<SynchronizerUpdate>(OnSynchronizerUpdate);

#if UNITY_EDITOR
            if (m_DummyPlayableGraph.IsValid())
            {
                m_DummyPlayableGraph.Destroy();
            }
#endif
        }

        void OnSynchronizerUpdate()
        {
            Impl.Update();
        }

        /// <summary>
        /// Starts calibration of the synchronized group.
        /// </summary>
        /// <remarks>
        /// Calibration is used to automatically determine optimal delays and buffer sizes for each data
        /// source in the synchronized group so they will have valid data to present for each synchronized frame.
        /// </remarks>
        /// <param name="calibrator">The calibrator to use. If <see langword="null"/>, the default calibrator is used.</param>
        public void StartCalibration(ISynchronizationCalibrator calibrator = null)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            calibrator ??= new DefaultSyncCalibrator();

            m_CalibrationCoro = StartCoroutine(Impl.StartCalibration(calibrator));
        }

        /// <summary>
        /// Cancels the synchronizer calibration if it is currently in progress.
        /// </summary>
        public void StopCalibration()
        {
            Impl.StopCalibration();

            if (m_CalibrationCoro != null)
            {
                StopCoroutine(m_CalibrationCoro);
                m_CalibrationCoro = null;
            }
        }
    }
}
