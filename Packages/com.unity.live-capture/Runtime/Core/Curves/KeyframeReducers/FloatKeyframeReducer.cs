using UnityEngine;

namespace Unity.LiveCapture
{
    class FloatKeyframeReducerImpl : IKeyframeReducerImpl<Keyframe>
    {
        const float kFloatMinValue = 0.00001F;

        public static FloatKeyframeReducerImpl Instance { get; } = new FloatKeyframeReducerImpl();

        public bool CanReduce(Keyframe value, Keyframe first, Keyframe second, float maxError)
        {
            var minValue = kFloatMinValue * maxError;
            var reduced = Evaluate(value.time, first, second);

            return !DeltaError(value.value, reduced, maxError, minValue);
        }

        static float Evaluate(float time, in Keyframe lhs, in Keyframe rhs)
        {
            var dx = rhs.time - lhs.time;
            var m1 = 0f;
            var m2 = 0f;
            var t = 0f;

            if (dx != 0f)
            {
                t = (time - lhs.time) / dx;
                m1 = lhs.outTangent * dx;
                m2 = rhs.inTangent * dx;
            }

            return MathUtility.Hermite(t, lhs.value, m1, m2, rhs.value);
        }

        static bool DeltaError(float value, float reduced, float percentage, float minValue)
        {
            var delta = Mathf.Abs(value - reduced);
            var absValue = Mathf.Abs(value);
            // (absValue > minValue || Abs(reducedValue) > minValue) part is necessary for reducing values which have tiny fluctuations around 0
            return (absValue > minValue || Mathf.Abs(reduced) > minValue) && (delta > absValue * percentage);
        }
    }

    class FloatKeyframeReducer : KeyframeReducer<Keyframe>
    {
        public FloatKeyframeReducer() : base(FloatKeyframeReducerImpl.Instance) { }
    }
}
