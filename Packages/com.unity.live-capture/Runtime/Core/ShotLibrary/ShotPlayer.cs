using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A component that plays shots stored in a <see cref="ShotLibrary" /> asset.
    /// </summary>
    /// <remarks>
    /// This component allows you to select a shot and prepares its PlayableDirector component with the
    /// according TimelineAsset. You can then review the take by playing the PlayableDirector or by using
    /// the Timeline window.
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(PlayableDirector))]
    [HelpURL(Documentation.baseURL + "ref-component-shot-player" + Documentation.endURL)]
    [DisallowMultipleComponent]
    public class ShotPlayer : MonoBehaviour
    {
        internal static List<ShotPlayer> Instances { get; } = new List<ShotPlayer>();
        internal static int Version { get; private set; }

        static void IncrementVersion()
        {
            unchecked
            {
                ++Version;
            }
        }

        [SerializeField]
        ShotLibrary m_ShotLibrary;
        [SerializeField]
        int m_Selection;
        [SerializeField, HideInInspector]
        List<TrackBindingEntry> m_Bindings = new List<TrackBindingEntry>();
        PlayableDirector m_Director;

        /// <summary>
        /// The <see cref="ShotLibrary" /> asset that provides the shot information.
        /// </summary>
        public ShotLibrary ShotLibrary
        {
            get => m_ShotLibrary;
            set
            {
                m_ShotLibrary = value;
                ValidateSelection();
            }
        }

        /// <summary>
        /// The index of the selected shot to preview.
        /// </summary>
        public int Selection
        {
            get => m_Selection;
            set
            {
                m_Selection = value;
                ValidateSelection();
            }
        }

        internal PlayableDirector Director => m_Director;

        void OnEnable()
        {
            m_Director = GetComponent<PlayableDirector>();

            Instances.Add(this);

            IncrementVersion();
        }

        void OnDisable()
        {
            Instances.Remove(this);

            IncrementVersion();
        }

        void OnValidate()
        {
            ValidateSelection();
        }

        void ValidateSelection()
        {
            var count = 0;

            if (m_ShotLibrary != null)
            {
                count = m_ShotLibrary.Count;
            }

            m_Selection = Mathf.Clamp(m_Selection, -1, count - 1);
        }

        bool IsValid()
        {
            return m_ShotLibrary != null && m_Director != null;
        }

        bool IsSelectionValid()
        {
            return m_Selection >= 0 && m_Selection < m_ShotLibrary.Count;
        }

        void Update()
        {
            SetupDirectorIfNeeded();
        }

        internal bool SetupDirectorIfNeeded()
        {
            var timeline = default(PlayableAsset);

            if (IsValid() && IsSelectionValid())
            {
                var shot = m_ShotLibrary.GetShot(m_Selection);
                var take = shot.Take;

                if (TakeRecorder.IsRecording()
                    && TakeRecorder.Context is ShotPlayerContext context
                    && context.ShotPlayer == this)
                {
                    take = shot.IterationBase;
                }

                timeline = take != null ? take.Timeline : null;
            }

            if (m_Director.playableAsset != timeline)
            {
                ClearSceneBindings();
                SetSceneBindings();

                m_Director.playableAsset = timeline;

                return true;
            }

            return false;
        }

        internal void ClearSceneBindings()
        {
            m_Director.ClearSceneBindings(m_Bindings);
            m_Bindings.Clear();
        }

        internal void SetSceneBindings()
        {
            if (IsValid() && IsSelectionValid())
            {
                var shot = m_ShotLibrary.GetShot(m_Selection);

                if (shot.Take != null)
                {
                    m_Bindings.AddRange(shot.Take.BindingEntries);
                }

                if (shot.IterationBase != null)
                {
                    m_Bindings.AddRange(shot.IterationBase.BindingEntries);
                }

                m_Director.SetSceneBindings(m_Bindings);
            }
        }
    }
}
