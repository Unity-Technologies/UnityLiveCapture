using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector3TangentUpdaterImpl : ITangentUpdaterImpl<Keyframe<Vector3>>
    {
        const float k_CurveTimeEpsilon = 0.00001f;
        public static Vector3TangentUpdaterImpl Instance { get; } = new Vector3TangentUpdaterImpl();

        public Keyframe<Vector3> UpdateFirstTangent(in Keyframe<Vector3> keyframe, in Keyframe<Vector3> nextKeyframe)
        {
            var dx = nextKeyframe.Time - keyframe.Time;
            var dy = nextKeyframe.Value - keyframe.Value;
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Vector3> UpdateLastTangent(in Keyframe<Vector3> keyframe, in Keyframe<Vector3> prevKeyframe)
        {
            var dx = keyframe.Time - prevKeyframe.Time;
            var dy = keyframe.Value - prevKeyframe.Value;
            var m = dy.SafeDivide(dx, k_CurveTimeEpsilon);
            var result = keyframe;

            result.InTangent = m;
            result.OutTangent = m;

            return result;
        }

        public Keyframe<Vector3> UpdateTangents(in Keyframe<Vector3> keyframe, in Keyframe<Vector3> prevKeyframe, in Keyframe<Vector3> nextKeyframe)
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

    class Vector3TangentUpdater : TangentUpdater<Keyframe<Vector3>>
    {
        public Vector3TangentUpdater() : base(Vector3TangentUpdaterImpl.Instance) { }
    }
}
