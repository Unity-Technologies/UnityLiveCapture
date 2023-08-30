using UnityEngine;

namespace Unity.LiveCapture
{
    class QuaternionTangentUpdaterImpl : ITangentUpdaterImpl<Keyframe<Quaternion>>
    {
        const float k_CurveTimeEpsilon = 0.00001f;

        public static QuaternionTangentUpdaterImpl Instance { get; } = new QuaternionTangentUpdaterImpl();

        public Keyframe<Quaternion> UpdateFirstTangent(in Keyframe<Quaternion> keyframe, in Keyframe<Quaternion> nextKeyframe)
        {
            var dx = nextKeyframe.Time - keyframe.Time;
            var dy = nextKeyframe.Value.Sub(keyframe.Value);
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Quaternion> UpdateLastTangent(in Keyframe<Quaternion> keyframe, in Keyframe<Quaternion> prevKeyframe)
        {
            var dx = keyframe.Time - prevKeyframe.Time;
            var dy = keyframe.Value.Sub(prevKeyframe.Value);
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Quaternion> UpdateTangents(in Keyframe<Quaternion> keyframe, in Keyframe<Quaternion> prevKeyframe, in Keyframe<Quaternion> nextKeyframe)
        {
            var dx1 = keyframe.Time - prevKeyframe.Time;
            var dy1 = keyframe.Value.Sub(prevKeyframe.Value);

            var dx2 = nextKeyframe.Time - keyframe.Time;
            var dy2 = nextKeyframe.Value.Sub(keyframe.Value);

            var m1 = dy1.SafeDivide(dx1, k_CurveTimeEpsilon);
            var m2 = dy2.SafeDivide(dx2, k_CurveTimeEpsilon);

            var m = m1.Mul(0.5f).Add(m2.Mul(0.5f));
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }
    }

    class QuaternionTangentUpdater : TangentUpdater<Keyframe<Quaternion>>
    {
        public QuaternionTangentUpdater() : base(QuaternionTangentUpdaterImpl.Instance) { }
    }
}
