using UnityEngine;

namespace Unity.LiveCapture
{
    class QuaternionKeyframeReducerImpl : IKeyframeReducerImpl<Keyframe<Quaternion>>
    {
        const float kQuaternionNormalizationError = 0.001f;
        public static QuaternionKeyframeReducerImpl Instance { get; } = new QuaternionKeyframeReducerImpl();

        public bool CanReduce(Keyframe<Quaternion> value, Keyframe<Quaternion> first, Keyframe<Quaternion> second, float maxError)
        {
            var reduced = Evaluate(value.Time, first, second);

            return !QuaternionDistanceError(value.Value, reduced, maxError);
        }

        static Quaternion Evaluate(float time, in Keyframe<Quaternion> lhs, in Keyframe<Quaternion> rhs)
        {
            var dx = rhs.Time - lhs.Time;
            var m1 = default(Quaternion);
            var m2 = default(Quaternion);
            var t = 0f;

            if (dx != 0f)
            {
                t = (time - lhs.Time) / dx;
                m1 = lhs.OutTangent.Mul(dx);
                m2 = rhs.InTangent.Mul(dx);
            }

            return MathUtility.Hermite(t, lhs.Value, m1, m2, rhs.Value);
        }

        static bool QuaternionDistanceError(Quaternion value, Quaternion reduced, float quaternionDotError)
        {
            var magnitude = reduced.Magnitude();

            if (!MathUtility.CompareApproximately(1f, magnitude, kQuaternionNormalizationError))
            {
                return true;
            }

            value.Normalize();
            reduced = reduced.Div(magnitude);

            return Quaternion.Dot(value, reduced) < quaternionDotError;
        }
    }
    class QuaternionKeyframeReducer : KeyframeReducer<Keyframe<Quaternion>>
    {
        public QuaternionKeyframeReducer() : base(QuaternionKeyframeReducerImpl.Instance) { }
    }
}
