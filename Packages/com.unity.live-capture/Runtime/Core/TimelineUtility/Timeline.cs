using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a provider of the information currently being edited in the Timeline Editor Window.
    /// </summary>
    interface ITimelineImpl
    {
        /// <summary>
        /// The PlayableDirector associated with the timeline currently being shown in the Timeline window.
        /// </summary>
        PlayableDirector InspectedDirector { get; }

        /// <summary>
        /// The PlayableDirector responsible for the playback of the timeline currently being shown in the Timeline window.
        /// </summary>
        PlayableDirector MasterDirector { get; }

        /// <summary>
        /// The TimelineAsset currently being shown in the Timeline window.
        /// </summary>
        TimelineAsset InspectedAsset { get; }

        /// <summary>
        /// The TimelineAsset at the root of the hierarchy currently being shown in the Timeline window.
        /// </summary>
        TimelineAsset MasterAsset { get; }

        void SetAsMasterDirector(PlayableDirector director);
        void Play();
        void Pause();
        void SetGlobalTime(double time);
        void Repaint();
        List<PlayableDirector> GetSubTimelines(TimelineClip clip, PlayableDirector director);
    }

    /// <summary>
    /// Information currently being edited in the Timeline Editor Window.
    /// </summary>
    class Timeline
    {
        internal static Timeline Instance { get; } = new Timeline();

        ITimelineImpl m_Impl;

        internal void SetImpl(ITimelineImpl impl)
        {
            m_Impl = impl;
        }

        /// <summary>
        /// The PlayableDirector associated with the timeline currently being shown in the Timeline window.
        /// </summary>
        public static PlayableDirector InspectedDirector => Instance.m_Impl?.InspectedDirector;

        /// <summary>
        /// The PlayableDirector responsible for the playback of the timeline currently being shown in the Timeline window.
        /// </summary>
        public static PlayableDirector MasterDirector => Instance.m_Impl?.MasterDirector;

        /// <summary>
        /// The TimelineAsset currently being shown in the Timeline window.
        /// </summary>
        public static TimelineAsset InspectedAsset => Instance.m_Impl?.InspectedAsset;

        /// <summary>
        /// The TimelineAsset at the root of the hierarchy currently being shown in the Timeline window.
        /// </summary>
        public static TimelineAsset MasterAsset => Instance.m_Impl?.MasterAsset;


        /// <summary>
        /// Checks if the Timeline window is previewing a Timeline.
        /// </summary>
        /// <returns><see langword="true"/> if the Timeline window is previewing a Timeline; otherwise, <see langword="false"/>.</returns>
        public static bool IsActive()
        {
            return MasterDirector != null && MasterDirector.playableGraph.IsValid();
        }

        public static void SetAsMasterDirector(PlayableDirector director)
        {
            Instance.m_Impl?.SetAsMasterDirector(director);
        }

        public static void Play()
        {
            Instance.m_Impl?.Play();
        }

        public static void Pause()
        {
            Instance.m_Impl?.Pause();
        }

        public static void SetGlobalTime(double time)
        {
            Instance.m_Impl?.SetGlobalTime(time);
        }

        public static void Repaint()
        {
            Instance.m_Impl?.Repaint();
        }

        public static List<PlayableDirector> GetSubTimelines(TimelineClip clip, PlayableDirector director)
        {
            return Instance.m_Impl?.GetSubTimelines(clip, director);
        }
    }
}
