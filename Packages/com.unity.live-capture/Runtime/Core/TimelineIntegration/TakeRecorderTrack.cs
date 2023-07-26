using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Timeline track that you can use to play and record a <see cref="Take"/>.
    /// </summary>
    [TrackClipType(typeof(ShotPlayableAsset))]
    [HelpURL(Documentation.baseURL + "take-system-setting-up-timeline" + Documentation.endURL)]
    class TakeRecorderTrack : TrackAsset
    {
#if UNITY_EDITOR
        bool m_GatheringProperties;
#endif

        /// <inheritdoc/>
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            var mixerPlayable = ScriptPlayable<TakeRecorderTrackMixer>.Create(graph, inputCount);
            var mixer = mixerPlayable.GetBehaviour();

            mixer.Construct(director, this);

            return mixerPlayable;
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (m_GatheringProperties)
            {
                return;
            }

            m_GatheringProperties = true;

            var sceneReferences = new HashSet<UnityObject>();

            foreach (var clip in GetClips())
            {
                var shotAsset = clip.asset as ShotPlayableAsset;
                var take = shotAsset.Take;
                var iterationBase = shotAsset.IterationBase;

                if (take != null)
                {
                    take.Timeline.GatherProperties(director, driver);

                    GatherSceneReferences(director, take, sceneReferences);
                }

                if (iterationBase != null)
                {
                    iterationBase.Timeline.GatherProperties(director, driver);

                    GatherSceneReferences(director, iterationBase, sceneReferences);
                }
            }

            var previewer = new TimelinePropertyPreviewer(driver);
            var previewables = new List<IPreviewable>();

            foreach (var obj in sceneReferences)
            {
                if (obj is GameObject go)
                {
                    previewables.AddRange(go.GetComponents<IPreviewable>());
                }
                else if (obj is Component component)
                {
                    previewables.AddRange(component.GetComponents<IPreviewable>());
                }
            }

            foreach (var previewable in previewables)
            {
                previewable.Register(previewer);
            }

            m_GatheringProperties = false;
        }

        static void GatherSceneReferences(PlayableDirector director, Take take, HashSet<UnityObject> sceneReferences)
        {
            Debug.Assert(director != null);
            Debug.Assert(take != null);
            Debug.Assert(sceneReferences != null);

            foreach (var entry in take.BindingEntries)
            {
                var value = director.GetGenericBinding(entry.Track);

                if (value != null)
                {
                    sceneReferences.Add(value);
                }
            }
        }
#endif
    }
}
