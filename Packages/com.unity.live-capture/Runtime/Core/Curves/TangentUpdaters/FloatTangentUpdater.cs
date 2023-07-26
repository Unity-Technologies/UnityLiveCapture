using UnityEngine;

namespace Unity.LiveCapture
{
    class FloatTangentUpdaterImpl : ITangentUpdaterImpl<Keyframe>
    {
        public static FloatTangentUpdaterImpl Instance { get; } = new FloatTangentUpdaterImpl();

        public Keyframe UpdateFirstTangent(in Keyframe keyframe, in Keyframe nextKeyframe)
        {
            return AnimationCurveUtility.UpdateTangents(keyframe, keyframe, nextKeyframe);
        }

        public Keyframe UpdateLastTangent(in Keyframe keyframe, in Keyframe prevKeyframe)
        {
            return AnimationCurveUtility.UpdateTangents(keyframe, prevKeyframe, keyframe);
        }

        public Keyframe UpdateTangents(in Keyframe keyframe, in Keyframe prevKeyframe, in Keyframe nextKeyframe)
        {
            return AnimationCurveUtility.UpdateTangents(keyframe, prevKeyframe, nextKeyframe);
        }
    }

    class FloatTangentUpdater : TangentUpdater<Keyframe>
    {
        public FloatTangentUpdater() : base(FloatTangentUpdaterImpl.Instance) { }
    }
}
