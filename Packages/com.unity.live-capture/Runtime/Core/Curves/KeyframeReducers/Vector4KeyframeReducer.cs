using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector4KeyframeReducerImpl : IKeyframeReducerImpl<Keyframe<Vector4>>
    {
        const float kPositionMinValue = 0.00001F;
        public static Vector4KeyframeReducerImpl Instance { get; } = new Vector4KeyframeReducerImpl();

        public bool CanReduce(Keyframe<Vector4> value, Keyframe<Vector4> first, Keyframe<Vector4> second, float maxError)
        {
            var reduced = Evaluate(value.Time, first, second);

            return !DistanceError(value.Value, reduced, maxError);
        }

        static Vector4 Evaluate(float time, in Keyframe<Vector4> lhs, in Keyframe<Vector4> rhs)
        {
            var dx = rhs.Time - lhs.Time;
            var m1 = default(Vector4);
            var m2 = default(Vector4);
            var t = 0f;

            if (dx != 0f)
            {
                t = (time - lhs.Time) / dx;
                m1 = lhs.OutTangent * dx;
                m2 = rhs.InTangent * dx;
            }

            return MathUtility.Hermite(t, lhs.Value, m1, m2, rhs.Value);
        }

        static bool DistanceError(Vector4 value, Vector4 reduced, float percentage)
        {
            var minValue = kPositionMinValue * percentage;

            // Vector4 distance as a percentage
            var distance = (value - reduced).sqrMagnitude;
            var length = value.sqrMagnitude;
            var lengthReduced = reduced.sqrMagnitude;
            //if (distance > length * Sqr(percentage))
            if (DeltaError(length, lengthReduced, distance, percentage * percentage, minValue * minValue))
                return true;

            // Distance of each axis
            var distanceX = Mathf.Abs(value.x - reduced.x);
            var distanceY = Mathf.Abs(value.y - reduced.y);
            var distanceZ = Mathf.Abs(value.z - reduced.z);
            var distanceW = Mathf.Abs(value.w - reduced.w);

            //if (distanceX > Abs(value.x) * percentage)
            if (DeltaError(value.x, reduced.x, distanceX, percentage, minValue))
                return true;
            //if (distanceY > Abs(value.y) * percentage)
            if (DeltaError(value.y, reduced.y, distanceY, percentage, minValue))
                return true;
            //if (distanceZ > Abs(value.z) * percentage)
            if (DeltaError(value.z, reduced.z, distanceZ, percentage, minValue))
                return true;
            //if (distanceW > Abs(value.w) * percentage)
            if (DeltaError(value.w, reduced.w, distanceW, percentage, minValue))
                return true;

            return false;
        }

        static bool DeltaError(float value, float reduced, float delta, float percentage, float minValue)
        {
            var absValue = Mathf.Abs(value);
            // (absValue > minValue || Abs(reducedValue) > minValue) part is necessary for reducing values which have tiny fluctuations around 0
            return (absValue > minValue || Mathf.Abs(reduced) > minValue) && (delta > absValue * percentage);
        }
    }

    class Vector4KeyframeReducer : KeyframeReducer<Keyframe<Vector4>>
    {
        public Vector4KeyframeReducer() : base(Vector4KeyframeReducerImpl.Instance) { }
    }
}
