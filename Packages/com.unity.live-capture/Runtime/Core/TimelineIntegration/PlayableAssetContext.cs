using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture
{
    class PlayableAssetContext
    {
        TrackAsset m_Track;
        ShotPlayableAsset m_Asset;
        TimelineHierarchyContext m_Hierarchy;

        public ShotPlayableAsset Asset => m_Asset;
        public TimelineHierarchyContext Hierarchy => m_Hierarchy;

        public PlayableAssetContext(TimelineClip clip, TimelineHierarchyContext hierarchyContext)
        {
            m_Track = clip.GetParentTrack();
            m_Asset = clip.asset as ShotPlayableAsset;
            m_Hierarchy = hierarchyContext;
        }

        public PlayableDirector GetDirector()
        {
            return m_Hierarchy.GetLeaf().Director;
        }

        public double GetTimeOffset()
        {
            if (TryGetClip(out var clip))
            {
                return clip.clipIn;
            }

            return 0d;
        }

        public void Play()
        {
            var root = m_Hierarchy.GetRootDirector();

            if (root != null)
            {
                PlayableDirectorControls.Play(root);
            }
        }

        public bool IsPlaying()
        {
            var root = m_Hierarchy.GetRootDirector();

            if (root != null)
            {
                return PlayableDirectorControls.IsPlaying(root);
            }

            return false;
        }

        public void Pause()
        {
            var root = m_Hierarchy.GetRootDirector();

            if (root != null)
            {
                PlayableDirectorControls.Pause(root);
            }
        }

        public double GetTime()
        {
            if (TryGetClip(out var clip))
            {
                return m_Hierarchy.GetLocalTime() - clip.start;
            }

            return 0d;
        }

        public void SetTime(double value)
        {
            if (TryGetClip(out var clip))
            {
                value = MathUtility.Clamp(value, 0, GetDuration());

                m_Hierarchy.SetLocalTime(value + clip.start);
            }
        }

        public double GetDuration()
        {
            if (TryGetClip(out var clip))
            {
                return clip.duration;
            }

            return 0d;
        }

        bool TryGetClip(out TimelineClip clip)
        {
            clip = null;

            if (m_Track != null && m_Asset != null)
            {
                clip = m_Track.GetClips().FirstOrDefault(c => c.asset == m_Asset);
            }

            return clip != null;
        }
    }
}
