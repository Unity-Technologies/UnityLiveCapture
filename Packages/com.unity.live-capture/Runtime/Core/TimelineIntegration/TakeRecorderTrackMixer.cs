using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    class TakeRecorderTrackMixer : PlayableBehaviour
    {
        bool m_Initialized;
        Playable m_Playable;
        TimelineHierarchyContext m_HierarchyContext;
        TrackAsset m_Track;
        PlayableDirector m_Director;
        List<TimelineClip> m_Clips;
        Dictionary<Playable, float> m_Weights = new Dictionary<Playable, float>();

        public override void OnPlayableCreate(Playable playable)
        {
            m_Playable = playable;
        }

        public override void OnGraphStart(Playable playable)
        {
            Initialize();
        }

        public override void OnGraphStop(Playable playable)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            for (var i = 0; i < m_Playable.GetInputCount(); ++i)
            {
                var slatePlayable = m_Playable.GetInput(i);
                var weight = m_Playable.GetInputWeight(i);

                if (slatePlayable.IsValid() && slatePlayable.GetInputCount() > 0)
                {
                    m_Weights[slatePlayable.GetInput(0)] = weight;
                }
            }

            var graph = playable.GetGraph();
            var rootPlayable = graph.GetRootPlayable(0);
            var outputCount = graph.GetOutputCount();

            for (var i = 0; i < outputCount; ++i)
            {
                var output = graph.GetOutput(i);
                var sourcePlayable = output.GetSourcePlayable();

                if (m_Weights.TryGetValue(sourcePlayable, out var weight))
                {
                    output.SetWeight(weight);
                    sourcePlayable.SetDone(rootPlayable.IsDone());
                }
            }
        }

        void Initialize()
        {
            if (m_Initialized)
            {
                return;
            }

            var count = m_Playable.GetInputCount();

            Debug.Assert(m_Clips.Count == count);

            var context = MasterTimelineContext.Instance;
            var activeContext = TakeRecorder.Context;
            var isRecording = context == activeContext && TakeRecorder.IsRecording();

            for (var i = 0; i < count; ++i)
            {
                var clip = m_Clips[i];
                var asset = clip.asset as ShotPlayableAsset;

                asset.Migrate(clip.displayName);

                if (asset.AutoClipName)
                {
                    clip.displayName = asset.ShotName;
                }

                var index = context.IndexOf(m_HierarchyContext, asset);
                var inputPlayable = (ScriptPlayable<NestedTimelinePlayable>)m_Playable.GetInput(i);
                var nestedTimeline = inputPlayable.GetBehaviour();
                var isContextRecording = isRecording && index == context.Selection;
                var take = isContextRecording ? asset.IterationBase : asset.Take;

                if (take != null)
                {
                    SetTrackBindingsIfNeeded(take);

                    nestedTimeline.SetTimeline(take.Timeline);
                }
            }

            m_Director.RebindPlayableGraphOutputs();

            m_Initialized = true;
        }

        public void Construct(PlayableDirector director, TrackAsset track)
        {
            Debug.Assert(director != null);
            Debug.Assert(track != null);

            m_Track = track;
            m_Director = director;
            m_HierarchyContext = TimelineHierarchyContextUtility.FromContext(new TimelineContext(director));
            m_Clips = new List<TimelineClip>(track.GetClips());
        }

        void SetTrackBindingsIfNeeded(Take take)
        {
            Debug.Assert(take != null);

            var requiresLoadingTrackBindings = false;

            foreach (var entry in take.BindingEntries)
            {
                var track = entry.Track;
                var binding = entry.Binding;

                if (track == null)
                    continue;

                var exposedProperty = binding.GetValue(m_Director);
                var value = m_Director.GetGenericBinding(track);

                if (exposedProperty != null && value == null)
                {
                    requiresLoadingTrackBindings = true;

                    break;
                }
            }

            if (requiresLoadingTrackBindings)
            {
                m_Director.SetSceneBindings(take.BindingEntries);
            }
        }
    }
}
