using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a provider of the state of a timeline hierarchy.
    /// </summary>
    interface ITimelineHierarchyImpl
    {
        /// <summary>
        /// Gets the PlayableDirector and the TimelineClip that play a specified PlayableDirector.
        /// </summary>
        /// <param name="director">The PlayableDirector to find the clip from.</param>
        /// <param name="parentDirector">When this method returns, contains the PlayableDirector currently playing
        /// the specified PlayableDirector, if any; otherwise, the default value for PlayableDirector.</param>
        /// <param name="parentClip">When this method returns, contains the TimelineClip currently playing
        /// the specified PlayableDirector, if any; otherwise, the default value for TimelineClip.</param>
        /// <returns><see langword="true"/> if the parent director and clip was found; otherwise, <see langword="false"/>.</returns>
        bool TryGetParentContext(PlayableDirector director, out PlayableDirector parentDirector, out TimelineClip parentClip);
    }

    /// <summary>
    /// Gets the PlayableDirector playing the provided PlayableDirector one level up in the hierarchy of timelines.
    /// </summary>
    class TimelineHierarchy
    {
        internal static TimelineHierarchy Instance { get; } = new TimelineHierarchy();

        ITimelineHierarchyImpl m_Impl;

        internal void SetImpl(ITimelineHierarchyImpl impl)
        {
            m_Impl = impl;
        }

        /// <summary>
        /// Gets the PlayableDirector playing the provided PlayableDirector at the top level of the hierarchy of timelines.
        /// </summary>
        /// <param name="director">The PlayableDirector to look the root for.</param>
        /// <returns>The root PlayableDirector.</returns>
        public static PlayableDirector GetRootDirector(PlayableDirector director)
        {
            if (director == null)
                throw new ArgumentNullException(nameof(director));

            if (TryGetParentContext(director, out var parentDirector, out var parentClip))
            {
                return GetRootDirector(parentDirector);
            }
            else
            {
                return director;
            }
        }

        /// <summary>
        /// Gets the PlayableDirector and the TimelineClip that play a specified PlayableDirector.
        /// </summary>
        /// <param name="director">The PlayableDirector to find the clip from.</param>
        /// <param name="parentDirector">When this method returns, contains the PlayableDirector currently playing
        /// the specified PlayableDirector, if any; otherwise, the default value for PlayableDirector.</param>
        /// <param name="parentClip">When this method returns, contains the TimelineClip currently playing
        /// the specified PlayableDirector, if any; otherwise, the default value for TimelineClip.</param>
        /// <returns><see langword="true"/> if the parent director and clip was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetParentContext(PlayableDirector director, out PlayableDirector parentDirector, out TimelineClip parentClip)
        {
            parentDirector = null;
            parentClip = null;

            if (director == null)
                throw new ArgumentNullException(nameof(director));

            return Instance.m_Impl != null
                && Instance.m_Impl.TryGetParentContext(director, out parentDirector, out parentClip);
        }
    }
}
