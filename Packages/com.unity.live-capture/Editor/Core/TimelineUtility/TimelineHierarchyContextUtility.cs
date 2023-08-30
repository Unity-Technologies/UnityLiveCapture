using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Editor
{
    static class TimelineHierarchyContextUtility
    {
        /// <summary>
        /// Creates a <see cref="TimelineHierarchyContext"/> that matches the hierarchy displayed in the Timeline window.
        /// </summary>
        public static TimelineHierarchyContext FromTimelineNavigation()
        {
            return new TimelineHierarchyContext(EnumerateFromTimelineNavigation());
        }

        internal static IEnumerable<TimelineContext> EnumerateFromTimelineNavigation()
        {
            return EnumerateFromTimelineNavigationInternal().Reverse();
        }

        static IEnumerable<TimelineContext> EnumerateFromTimelineNavigationInternal()
        {
            var window = TimelineEditor.GetWindow();

            if (window == null)
            {
                yield return default(TimelineContext);
                yield break;
            }

            var contexts = window.navigator.GetBreadcrumbs();
            var prevContext = default(SequenceContext);
            var first = true;

            foreach (var context in contexts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    yield return new TimelineContext(prevContext.director, context.clip);
                }

                prevContext = context;
            }

            yield return new TimelineContext(Timeline.InspectedDirector);
        }

        /// <summary>
        /// Gets the sub-timelines for a specific clip if it supports playing nested timelines.
        /// </summary>
        /// <param name="clip">The clip with the ControlPlayableAsset.</param>
        /// <param name="director">The playable director driving the Timeline Clip. This may not be the same as TimelineEditor.inspectedDirector.</param>
        /// <returns>The sub-timelines to control.</returns>
        public static List<PlayableDirector> GetSubTimelines(TimelineClip clip, PlayableDirector director)
        {
            var editor = CustomTimelineEditorCache.GetClipEditor(clip);
            List<PlayableDirector> directors = new List<PlayableDirector>();
            try
            {
                editor.GetSubTimelines(clip, director, directors);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return directors;
        }
    }
}
