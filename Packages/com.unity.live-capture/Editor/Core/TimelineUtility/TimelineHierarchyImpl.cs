using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Playables;

namespace Unity.LiveCapture.Editor
{
    [InitializeOnLoad]
    class TimelineHierarchyImpl : ITimelineHierarchyImpl
    {
        static TimelineHierarchyImpl Instance { get; } = new TimelineHierarchyImpl();

        Dictionary<PlayableDirector, List<(PlayableDirector, TimelineClip)>> m_Parents =
            new Dictionary<PlayableDirector, List<(PlayableDirector, TimelineClip)>>();

        bool m_IsDirty = true;

        static TimelineHierarchyImpl()
        {
            Utility.graphCreated += OnGraphCreated;
            Utility.destroyingGraph += OnGraphDestroying;

            TimelineHierarchy.Instance.SetImpl(Instance);
        }

        static void OnGraphCreated(PlayableGraph graph)
        {
            Instance.SetDirty();
        }

        static void OnGraphDestroying(PlayableGraph graph)
        {
            Instance.SetDirty();
        }

        static IEnumerable<PlayableDirector> GetPlayableDirectors()
        {
            return Utility.GetAllGraphs()
                .Where(g => g.IsValid())
                .Select(g => g.GetResolver())
                .OfType<PlayableDirector>()
                .Where(p => p.playableAsset is TimelineAsset);
        }

        void SetDirty()
        {
            m_IsDirty = true;
        }

        /// <inheritdoc />
        public bool TryGetParentContext(PlayableDirector director, out PlayableDirector parentDirector, out TimelineClip parentClip)
        {
            if (director == null)
            {
                throw new ArgumentNullException(nameof(director));
            }

            RebuildContextIfNeeded();

            parentDirector = default(PlayableDirector);
            parentClip = default(TimelineClip);

            var parents = GetParents(director);

            foreach (var entry in parents)
            {
                var parent = entry.Item1;
                var clip = entry.Item2;

                Debug.Assert(parent != null);
                Debug.Assert(clip != null);

                if (!parent.enabled)
                    continue;

                var graph = parent.playableGraph;

                if (!graph.IsValid())
                    continue;

                var time = parent.time;

                if (time < clip.start || time >= clip.end)
                    continue;

                parentDirector = parent;
                parentClip = clip;

                break;
            }

            return parentDirector != null;
        }

        void RebuildContextIfNeeded()
        {
            if (m_IsDirty)
            {
                RebuildContext();
            }
        }

        void RebuildContext()
        {
            var directors = new HashSet<PlayableDirector>();

            m_Parents.Clear();

            foreach (var director in GetPlayableDirectors())
            {
                if (directors.Contains(director))
                {
                    continue;
                }

                directors.Add(director);

                RebuildContext(director, directors);
            }

            m_IsDirty = false;
        }

        void RebuildContext(PlayableDirector director, ISet<PlayableDirector> directors)
        {
            var timelineAsset = director.playableAsset as TimelineAsset;

            if (timelineAsset == null)
                return;

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track.muted)
                {
                    continue;
                }

                foreach (var clip in track.GetClips())
                {
                    foreach (var subDirector in TimelineHierarchyContextUtility.GetSubTimelines(clip, director))
                    {
                        var parents = GetParents(subDirector);

                        parents.Add((director, clip));
                        directors.Add(subDirector);

                        RebuildContext(subDirector, directors);
                    }
                }
            }
        }

        List<(PlayableDirector, TimelineClip)> GetParents(PlayableDirector director)
        {
            if (!m_Parents.TryGetValue(director, out var parents))
            {
                parents = new List<(PlayableDirector, TimelineClip)>();

                m_Parents[director] = parents;
            }

            return parents;
        }
    }
}
