using System;
using UnityEngine.Playables;
using Unity.LiveCapture.Internal;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A collection of methods to control a PlayableDirector abstracting away the timeline hierarchy.
    /// </summary>
    class PlayableDirectorControls
    {
        /// <summary>
        /// Plays a PlayableDirector.
        /// </summary>
        /// <remarks>
        /// This method abstracts away any timeline hierarchy the PlayableDirector my be sitting in.
        /// </remarks>
        /// <param name="director">The PlayableDirector play.</param>
        public static void Play(PlayableDirector director)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            var root = TimelineHierarchy.GetRootDirector(director);

            if (Timeline.MasterDirector == root)
            {
                Timeline.Play();
            }
            else
            {
                PlayableDirectorInternal.ResetFrameTiming();

                root.Play();
            }
        }

        public static bool IsPlaying(PlayableDirector director)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            var root = TimelineHierarchy.GetRootDirector(director);

            return root.state == PlayState.Playing;
        }

        /// <summary>
        /// Pauses a PlayableDirector.
        /// </summary>
        /// <remarks>
        /// This method abstracts away any timeline hierarchy the PlayableDirector my be sitting in.
        /// </remarks>
        /// <param name="director">The PlayableDirector pause.</param>
        public static void Pause(PlayableDirector director)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            var root = TimelineHierarchy.GetRootDirector(director);

            if (Timeline.MasterDirector == root)
            {
                Timeline.Pause();
            }
            else
            {
                root.Pause();
            }
        }

        /// <summary>
        /// Sets the time of a PlayableDirector by converting and setting it to the root PlayableDirector.
        /// </summary>
        /// <remarks>
        /// The provided time is local to the provided PlayableDirector,
        /// and it gets converted to time relative to the root PlayableDirector.
        /// </remarks>
        /// <param name="director">The PlayableDirector to set the time to.</param>
        /// <param name="time">The time to set local to the PlayableDirector.</param>
        public static void SetTime(PlayableDirector director, double time)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            while (TimelineHierarchy.TryGetParentContext(director, out var parentDirector, out var parentClip))
            {
                time = MathUtility.Clamp(time, 0f, parentClip.duration) + parentClip.start;
                director = parentDirector;
            }

            if (Timeline.MasterDirector == director)
            {
                // Director.state returns PlayState.Paused when the root playable's IsDone() returns true.
                // IsDone() returns true when time >= duration.
                // Calling Pause() before SetGlobalTime will have no effect when time >= duration,
                // as it relies on Director.state. Setting the time will work but the director will keep playing.
                // The order of this two calls is then important.
                Timeline.SetGlobalTime(time);
                Timeline.Pause();
            }
            else
            {
                director.Pause();
                director.time = time;
                director.DeferredEvaluate();
            }
        }
    }
}
