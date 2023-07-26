using UnityEngine;

namespace Unity.LiveCapture
{
    class IntegerTangentUpdaterImpl : ITangentUpdaterImpl<Keyframe>
    {
        public static IntegerTangentUpdaterImpl Instance { get; } = new IntegerTangentUpdaterImpl();

        public Keyframe UpdateFirstTangent(in Keyframe keyframe, in Keyframe nextKeyframe)
        {
            return MakeConstant(keyframe);
        }

        public Keyframe UpdateLastTangent(in Keyframe keyframe, in Keyframe prevKeyframe)
        {
            return MakeConstant(keyframe);
        }

        public Keyframe UpdateTangents(in Keyframe keyframe, in Keyframe prevKeyframe, in Keyframe nextKeyframe)
        {
            return MakeConstant(keyframe);
        }

        Keyframe MakeConstant(in Keyframe keyframe)
        {
            var result = keyframe;

            result.inTangent = float.PositiveInfinity;
            result.outTangent = float.PositiveInfinity;

            return result;
        }
    }

    class IntegerTangentUpdater : TangentUpdater<Keyframe>
    {
        public IntegerTangentUpdater() : base(IntegerTangentUpdaterImpl.Instance) { }
    }
}
