using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture
{
    interface IDirectorProvider
    {
        PlayableDirector Director { get; }
    }

    class DirectorContext : ITakeRecorderContext
    {
        IDirectorProvider m_Provider;
        PlayableGraph m_Graph;
        Shot[] m_ShotCache = Array.Empty<Shot>();
        List<PlayableAssetContext> m_Shots = new List<PlayableAssetContext>();

        public int Version { get; private set; }
        public int Selection { get; set; }
        public Shot[] Shots => m_ShotCache;
        public PlayableDirector Director { get; private set; }

        public DirectorContext(IDirectorProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            m_Provider = provider;

            Update();
        }

        public int IndexOf(TimelineHierarchyContext hierarchy, PlayableAsset asset)
        {
            if (asset == null)
            {
                return -1;
            }

            for (var i = 0; i < m_Shots.Count; ++i)
            {
                var shot = m_Shots[i];

                if (asset == shot.Asset && (hierarchy == null || hierarchy.Equals(shot.Hierarchy)))
                {
                    return i;
                }
            }

            return -1;
        }

        public void SetShot(int index, Shot shot)
        {
            if (index >= 0 && index < m_Shots.Count)
            {
                var prevShot = m_Shots[index];

                shot.TimeOffset = prevShot.GetTimeOffset();
                shot.Duration = prevShot.GetDuration();

                m_ShotCache[index] = shot;

                var asset = m_Shots[index].Asset;

                if (asset != null)
                {
                    asset.SceneNumber = shot.SceneNumber;
                    asset.ShotName = shot.Name;
                    asset.TakeNumber = shot.TakeNumber;
                    asset.Description = shot.Description;
                    asset.Directory = shot.Directory;
                    asset.Take = shot.Take;
                    asset.IterationBase = shot.IterationBase;
                }

                unchecked
                {
                    ++Version;
                }
            }
        }

        public Object GetStorage(int index)
        {
            return m_Shots[index].Asset;
        }

        public void Update()
        {
            Debug.Assert(m_Provider != null);

            var director = m_Provider.Director;
            var reload = Director != director
                || director != null && !director.playableGraph.Equals(m_Graph);

            Director = director;

            if (director != null)
            {
                m_Graph = director.playableGraph;
            }
            else
            {
                m_Graph = default(PlayableGraph);
            }

            if (reload)
            {
                Reload();
            }
        }

        public void Reload()
        {
            m_Shots.Clear();

            BuildShots(Director, m_Shots);

            var shots = new List<Shot>(m_Shots.Count);

            foreach (var shot in m_Shots)
            {
                var asset = shot.Asset;

                shots.Add(new Shot()
                {
                    TimeOffset = shot.GetTimeOffset(),
                    Duration = shot.GetDuration(),
                    SceneNumber = asset.SceneNumber,
                    Name = asset.ShotName,
                    TakeNumber = asset.TakeNumber,
                    Description = asset.Description,
                    Directory = asset.Directory,
                    Take = asset.Take,
                    IterationBase = asset.IterationBase
                });
            }

            m_ShotCache = shots.ToArray();

            unchecked
            {
                ++Version;
            }
        }

        public bool IsValid()
        {
            return Director != null;
        }

        public void Play()
        {
            if (this.HasSelection())
            {
                m_Shots[Selection].Play();
            }
        }

        public bool IsPlaying()
        {
            if (this.HasSelection())
            {
                return m_Shots[Selection].IsPlaying();
            }

            return false;
        }

        public void Pause()
        {
            if (this.HasSelection())
            {
                m_Shots[Selection].Pause();
            }
        }

        public double GetTime()
        {
            if (this.HasSelection())
            {
                return m_Shots[Selection].GetTime();
            }

            return 0d;
        }

        public void SetTime(double value)
        {
            if (this.HasSelection())
            {
                m_Shots[Selection].SetTime(value);
            }
        }

        public double GetDuration()
        {
            if (this.HasSelection())
            {
                return m_Shots[Selection].GetDuration();
            }

            return 0d;
        }

        public IExposedPropertyTable GetResolver(int index)
        {
            return m_Shots[index].GetDirector();
        }

        public void ClearSceneBindings(int index)
        {
            ClearSceneBindings(m_Shots[index]);
        }

        public void SetSceneBindings(int index)
        {
            SetSceneBindings(m_Shots[index]);
        }

        public void Rebuild(int index)
        {
            var ctx = m_Shots[index];

            if (ctx.Hierarchy.IsValid())
            {
                var director = ctx.GetDirector();

                director.RebuildGraph();
                director.Evaluate();

                // Rebuild might be called after DirectorUpdateAnimationEnd. Calling DeferredEvaluate
                // forces the Editor to do one extra update loop evaluation before the end of the frame.
                director.DeferredEvaluate();
            }
        }

        void ClearSceneBindings(PlayableAssetContext ctx)
        {
            var director = ctx.GetDirector();

            if (ctx.Asset.Take != null)
            {
                director.ClearSceneBindings(ctx.Asset.Take.BindingEntries);
            }

            if (ctx.Asset.IterationBase != null)
            {
                director.ClearSceneBindings(ctx.Asset.IterationBase.BindingEntries);
            }
        }

        void SetSceneBindings(PlayableAssetContext ctx)
        {
            var director = ctx.GetDirector();

            SetBindingsFromTimeline(director);
        }

        static void SetBindingsFromTimeline(PlayableDirector director)
        {
            Debug.Assert(director != null);

            var timeline = director.playableAsset as TimelineAsset;

            if (timeline == null)
                return;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is TakeRecorderTrack)
                {
                    foreach (var clip in track.GetClips())
                    {
                        var asset = clip.asset as ShotPlayableAsset;

                        SetSceneBindings(asset.Take, asset.IterationBase, director);
                    }
                }
            }
        }

        static void SetSceneBindings(Take take, Take iterationBase, PlayableDirector director)
        {
            Debug.Assert(director != null);

            if (take != null)
            {
                director.SetSceneBindings(take.BindingEntries);
            }

            if (iterationBase != null)
            {
                director.SetSceneBindings(iterationBase.BindingEntries);
            }
        }

        static void BuildShots(PlayableDirector director, List<PlayableAssetContext> shots)
        {
            if (director == null)
                return;

            var hierarchy = new Stack<TimelineContext>();

            BuildShots(director, null, hierarchy, shots);
        }

        static void BuildShots(
            PlayableDirector director,
            TimelineClip parentClip,
            Stack<TimelineContext> hierarchy,
            List<PlayableAssetContext> shots)
        {
            if (director == null)
                return;

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
                    foreach (var subDirector in Timeline.GetSubTimelines(clip, director))
                    {
                        hierarchy.Push(new TimelineContext(director, clip));

                        BuildShots(subDirector, clip, hierarchy, shots);

                        hierarchy.Pop();
                    }
                }
            }

            hierarchy.Push(new TimelineContext(director));

            var hierarchyContext = default(TimelineHierarchyContext);

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track.muted)
                {
                    continue;
                }

                if (track is TakeRecorderTrack takeRecorderTrack)
                {
                    if (hierarchyContext == null)
                    {
                        hierarchyContext = new TimelineHierarchyContext(hierarchy);
                    }

                    foreach (var clip in takeRecorderTrack.GetClips())
                    {
                        var asset = clip.asset as ShotPlayableAsset;
                        var ctx = new PlayableAssetContext(clip, hierarchyContext);

                        shots.Add(ctx);
                    }
                }
            }

            hierarchy.Pop();
        }
    }
}
