using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace Unity.LiveCapture
{
    class NestedTimelinePlayable : PlayableBehaviour
    {
        Playable m_Playable;
        Playable m_TimelinePlayable;
        TimelineAsset m_Current;

        public override void OnPlayableCreate(Playable playable)
        {
            m_Playable = playable;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Dispose();
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (m_TimelinePlayable.IsValid())
            {
                m_TimelinePlayable.Play();
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (m_TimelinePlayable.IsValid())
            {
                m_TimelinePlayable.Pause();
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (m_TimelinePlayable.IsValid())
            {
                m_TimelinePlayable.SetTime(playable.GetTime());
            }
        }

        public void SetTimeline(TimelineAsset timeline)
        {
            if (timeline == m_Current)
            {
                return;
            }

            Dispose();

            if (timeline != null && !CheckCircularDependencies(m_Playable, timeline))
            {
                var graph = m_Playable.GetGraph();
                var owner = (graph.GetResolver() as PlayableDirector).gameObject;

                m_TimelinePlayable = CreateTimelinePlayable(timeline, graph, owner);
                m_TimelinePlayable.SetDuration(timeline.duration);
                m_TimelinePlayable.SetTraversalMode(PlayableTraversalMode.Mix);

                m_Playable.AddInput(m_TimelinePlayable, 0, 1f);
                m_Current = timeline;
            }
        }

        void Dispose()
        {
            m_Current = null;

            if (!m_TimelinePlayable.IsValid())
            {
                return;
            }

            var graph = m_Playable.GetGraph();
            var index = 0;

            while (index < graph.GetOutputCount())
            {
                var output = graph.GetOutput(index);
                var sourcePlayable = output.GetSourcePlayable();

                if (sourcePlayable.Equals(m_TimelinePlayable))
                {
                    graph.DestroyOutput(output);
                }
                else
                {
                    ++index;
                }
            }

            m_TimelinePlayable.Destroy();
        }

        static Playable CreateTimelinePlayable(TimelineAsset timeline, PlayableGraph graph, GameObject owner)
        {
            Debug.Assert(timeline != null);
            Debug.Assert(graph.IsValid());
            Debug.Assert(owner != null);

            var autoRebalanceTree = false;
#if UNITY_EDITOR
            autoRebalanceTree = true;
#endif
            return TimelinePlayable.Create(
                graph, timeline.GetOutputTracks(), owner, autoRebalanceTree, true);
        }

        static bool CheckCircularDependencies(Playable target, TimelineAsset timeline)
        {
            Debug.Assert(target.IsValid());

            if (timeline == null)
            {
                return false;
            }

            var graph = target.GetGraph();
            var director = graph.GetResolver() as PlayableDirector;
            var rootTimeline = director.playableAsset as TimelineAsset;
            var rootPlayable = graph.GetRootPlayable(0);

            if (rootTimeline == timeline)
            {
                return true;
            }

            var count = 0;

            return FindCircularDependencies(rootPlayable, target, timeline, ref count);
        }

        static bool FindCircularDependencies(Playable playable, Playable target, TimelineAsset timeline, ref int count)
        {
            Debug.Assert(playable.IsValid());
            Debug.Assert(target.IsValid());
            Debug.Assert(timeline != null);

            var timelineFound = false;

            if (playable.GetPlayableType() == typeof(NestedTimelinePlayable))
            {
                if (playable.Equals(target))
                {
                    return count > 0;
                }

                var nestedPlayable = (ScriptPlayable<NestedTimelinePlayable>)playable;
                var behaviour = nestedPlayable.GetBehaviour();

                timelineFound = timeline == behaviour.m_Current;
            }

            if (timelineFound)
            {
                ++count;
            }

            for (var i = 0; i < playable.GetInputCount(); ++i)
            {
                var input = playable.GetInput(i);

                if (!input.IsValid())
                {
                    continue;
                }

                if (FindCircularDependencies(input, target, timeline, ref count))
                {
                    return true;
                }
            }

            if (timelineFound)
            {
                --count;
            }

            return false;
        }
    }
}
