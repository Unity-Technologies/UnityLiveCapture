#if UNITY_EDITOR
using UnityEditor;
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Playable Asset class for <see cref="TakeRecorderTrack"/>
    /// </summary>
#if UNITY_EDITOR
    [DisplayName("Shot")]
#endif
    class ShotPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        const int k_Version = 1;
        const string k_DefaultDirectory = "Assets/Takes";
        const string k_DefaultShotName = "New Shot";

        [SerializeField]
        int m_Version = k_Version;
        [SerializeField]
        bool m_AutoClipName = true;
        [SerializeField]
        int m_SceneNumber = 1;
        [SerializeField]
        string m_ShotName = k_DefaultShotName;
        [SerializeField]
        int m_TakeNumber = 1;
        [SerializeField]
        string m_Description;
        [SerializeField]
        string m_Directory = k_DefaultDirectory;
        [SerializeField]
        Take m_Take;
        [SerializeField]
        Take m_IterationBase;
        double? m_DurationOverride;

        public bool AutoClipName
        {
            get => m_AutoClipName;
            set => m_AutoClipName = value;
        }

        public string Directory
        {
            get => m_Directory;
            set => m_Directory = value;
        }

        public Take Take
        {
            get => m_Take;
            set => m_Take = value;
        }

        public Take IterationBase
        {
            get => m_IterationBase;
            set => m_IterationBase = value;
        }

        public int SceneNumber
        {
            get => m_SceneNumber;
            set => m_SceneNumber = value;
        }

        public string ShotName
        {
            get => m_ShotName;
            set => m_ShotName = value;
        }

        public int TakeNumber
        {
            get => m_TakeNumber;
            set => m_TakeNumber = value;
        }

        public string Description
        {
            get => m_Description;
            set => m_Description = value;
        }

        internal void Migrate(string clipName)
        {
            if (m_Version == 0)
            {
                m_ShotName = clipName;

                ++m_Version;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
#endif
            }

            m_Version = k_Version;
        }

        /// <summary>
        /// Describes the timeline features supported by a clip.
        /// </summary>
        ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.Extrapolation | ClipCaps.ClipIn;

        /// <summary>
        /// The playback duration of the instantiated Playable, in seconds.
        /// </summary>
        public override double duration => m_DurationOverride ?? GetTakeDuration(m_Take, base.duration);

        /// <summary>
        /// Injects SlatePlayables into the given graph.
        /// </summary>
        /// <param name="graph">The graph to inject playables into.</param>
        /// <param name="owner">The game object which initiated the build.</param>
        /// <returns>
        ///   <para>The playable injected into the graph, or the root playable if multiple playables are injected.</para>
        /// </returns>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<NestedTimelinePlayable>.Create(graph);
        }

        /// <summary>
        /// (Internal) Set a duration override in order to control the output of PlayableAsset.duration
        /// at specific moments in order to prevent Timeline from trimming created TimelineClips.
        /// </summary>
        internal void SetDurationOverride(double? duration)
        {
            m_DurationOverride = duration;
        }

        double GetTakeDuration(Take take, double defaultDuration)
        {
            if (take.TryGetContentRange(out var start, out var end))
            {
                return end;
            }

            return defaultDuration;
        }
    }
}
