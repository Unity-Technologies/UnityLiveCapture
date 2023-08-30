using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture.Editor
{
    static class TimelineExtensions
    {
        /// <summary>
        /// Information currently being edited in the Timeline Editor Window.
        /// </summary>
        /// <param name="timeline">The TimelineAsset to look into.</param>
        /// <param name="asset">The PlayableAsset that the TimelineClip should reference.</param>
        /// <param name="clip">When this method returns, contains the TimelineClip that references the provided PlayableAsset,
        /// if the key is found; otherwise, the default value for TimelineClip.</param>
        /// <returns><see langword="true"/> if the clip was found; otherwise, <see langword="false"/>.</returns>
        public static bool FindClip(this TimelineAsset timeline, PlayableAsset asset, out TimelineClip clip)
        {
            clip = default(TimelineClip);

            foreach (var track in timeline.GetOutputTracks())
            {
                foreach (var c in track.GetClips())
                {
                    if (c.asset == asset)
                    {
                        clip = c;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
