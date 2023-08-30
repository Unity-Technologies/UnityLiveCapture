using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector2KeyframeReducerImpl : IKeyframeReducerImpl<Keyframe<Vector2>>
    {
        const float kPositionMinValue = 0.00001F;
        public static Vector2KeyframeReducerImpl Instance { get; } = new Vector2KeyframeReducerImpl();

        public bool CanReduce(Keyframe<Vector2> value, Keyframe<Vector2> first, Keyframe<Vector2> second, float maxError)
        {
            var reduced = Evaluate(value.Time, first, second);

            return !DistanceError(value.Value, reduced, maxError);
        }

        static Vector2 Evaluate(float time, in Keyframe<Vector2> lhs, in Keyframe<Vector2> rhs)
        {
            var dx = rhs.Time - lhs.Time;
            var m1 = default(Vector2);
            var m2 = default(Vector2);
            var t = 0f;

            if (dx != 0f)
            {
                t = (time - lhs.Time) / dx;
                m1 = lhs.OutTangent * dx;
                m2 = rhs.InTangent * dx;
            }

            return MathUtility.Hermite(t, lhs.Value, m1, m2, rhs.Value);
        }

        static bool DistanceError(Vector2 value, Vector2 reduced, float percentage)
        {
            var minValue = kPositionMinValue * percentage;

            // Vector2 distance as a percentage
            var distance = (value - reduced).sqrMagnitude;
            var length = value.sqrMagnitude;
            var lengthReduced = reduced.sqrMagnitude;
            //if (distance > length * Sqr(percentage))
            if (DeltaError(length, lengthReduced, distance, percentage * percentage, minValue * minValue))
                return true;

            // Distance of each axis
            var distanceX = Mathf.Abs(value.x - reduced.x);
            var distanceY = Mathf.Abs(value.y - reduced.y);

            //if (distanceX > Abs(value.x) * percentage)
            if (DeltaError(value.x, reduced.x, distanceX, percentage, minValue))
                return true;
            //if (distanceY > Abs(value.y) * percentage)
            if (DeltaError(value.y, reduced.y, distanceY, percentage, minValue))
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

    class Vector2KeyframeReducer : KeyframeReducer<Keyframe<Vector2>>
    {
        public Vector2KeyframeReducer() : base(Vector2KeyframeReducerImpl.Instance) { }
    }
}
