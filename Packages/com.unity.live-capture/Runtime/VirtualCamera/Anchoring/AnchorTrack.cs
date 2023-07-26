using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture.VirtualCamera
{
    [TrackClipType(typeof(AnchorPlayableAsset))]
    [TrackBindingType(typeof(Animator))]
    class AnchorTrack : TrackAsset
    {
        /// <inheritdoc/>
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            var actor = director.GetGenericBinding(this) as Animator;

            foreach (var clip in GetClips())
            {
                var anchorAsset = clip.asset as AnchorPlayableAsset;

                anchorAsset.Actor = actor;
            }

            return Playable.Create(graph, inputCount);
        }
    }
}
