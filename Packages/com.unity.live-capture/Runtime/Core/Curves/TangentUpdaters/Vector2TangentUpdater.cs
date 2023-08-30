using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector2TangentUpdaterImpl : ITangentUpdaterImpl<Keyframe<Vector2>>
    {
        const float k_CurveTimeEpsilon = 0.00001f;
        public static Vector2TangentUpdaterImpl Instance { get; } = new Vector2TangentUpdaterImpl();

        public Keyframe<Vector2> UpdateFirstTangent(in Keyframe<Vector2> keyframe, in Keyframe<Vector2> nextKeyframe)
        {
            var dx = nextKeyframe.Time - keyframe.Time;
            var dy = nextKeyframe.Value - keyframe.Value;
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Vector2> UpdateLastTangent(in Keyframe<Vector2> keyframe, in Keyframe<Vector2> prevKeyframe)
        {
            var dx = keyframe.Time - prevKeyframe.Time;
            var dy = keyframe.Value - prevKeyframe.Value;
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Vector2> UpdateTangents(in Keyframe<Vector2> keyframe, in Keyframe<Vector2> prevKeyframe, in Keyframe<Vector2> nextKeyframe)
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

    class Vector2TangentUpdater : TangentUpdater<Keyframe<Vector2>>
    {
        public Vector2TangentUpdater() : base(Vector2TangentUpdaterImpl.Instance) { }
    }
}
