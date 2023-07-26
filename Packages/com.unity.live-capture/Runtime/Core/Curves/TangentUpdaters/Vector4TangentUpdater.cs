using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector4TangentUpdaterImpl : ITangentUpdaterImpl<Keyframe<Vector4>>
    {
        const float k_CurveTimeEpsilon = 0.00001f;
        public static Vector4TangentUpdaterImpl Instance { get; } = new Vector4TangentUpdaterImpl();

        public Keyframe<Vector4> UpdateFirstTangent(in Keyframe<Vector4> keyframe, in Keyframe<Vector4> nextKeyframe)
        {
            var dx = nextKeyframe.Time - keyframe.Time;
            var dy = nextKeyframe.Value - keyframe.Value;
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Vector4> UpdateLastTangent(in Keyframe<Vector4> keyframe, in Keyframe<Vector4> prevKeyframe)
        {
            var dx = keyframe.Time - prevKeyframe.Time;
            var dy = keyframe.Value - prevKeyframe.Value;
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Vector4> UpdateTangents(in Keyframe<Vector4> keyframe, in Keyframe<Vector4> prevKeyframe, in Keyframe<Vector4> nextKeyframe)
        {
            var dx1 = keyframe.Time - prevKeyframe.Time;
            var dy1 = keyframe.Value - prevKeyframe.Value;

            var dx2 = nextKeyframe.Time - keyframe.Time;
            var dy2 = nextKeyframe.Value - keyframe.Value;

            var m1 = dy1.SafeDivide(dx1, k_CurveTimeEpsilon);
            var m2 = dy2.SafeDivide(dx2, k_CurveTimeEpsilon);

            var m = m1 * 0.5f + m2 * 0.5f;
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }
    }

    class Vector4TangentUpdater : TangentUpdater<Keyframe<Vector4>>
    {
        public Vector4TangentUpdater() : base(Vector4TangentUpdaterImpl.Instance) { }
    }
}
