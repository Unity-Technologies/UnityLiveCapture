#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture.VirtualCamera
{
#if UNITY_EDITOR
    [DisplayName("Anchor")]
#endif
    class AnchorPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        TransformTakeBinding m_Target;

        [SerializeField]
        AnchorSettings m_Settings;

        public TransformTakeBinding Target
        {
            get => m_Target;
            set => m_Target = value;
        }

        public AnchorSettings Settings
        {
            get => m_Settings;
            set => m_Settings = value;
        }

        internal Animator Actor { get; set; }

        /// <summary>
        /// Describes the timeline features supported by a clip.
        /// </summary>
        ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.Extrapolation | ClipCaps.ClipIn;

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
            if (Actor == null)
            {
                return Playable.Create(graph);
            }

            var director = owner.GetComponent<PlayableDirector>();
            var target = m_Target.GetValue(director);

            var playable = ScriptPlayable<AnchorPlayable>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.Anchorable = Actor.GetComponent<IAnchorable>();
            behaviour.Target = target;
            behaviour.Settings = m_Settings;

            return playable;
        }
    }

    class AnchorPlayable : PlayableBehaviour
    {
        public IAnchorable Anchorable { get; set; }
        public Transform Target { get; set; }
        public AnchorSettings Settings { get; set; }

        int m_ID;

        public override void OnPlayableCreate(Playable playable)
        {
            m_ID = playable.GetHandle().GetHashCode();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (Anchorable != null)
            {
                Anchorable.AnchorTo(null, null, m_ID);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (Anchorable != null)
            {
                Anchorable.AnchorTo(Target, Settings, m_ID);
            }
        }
    }
}
