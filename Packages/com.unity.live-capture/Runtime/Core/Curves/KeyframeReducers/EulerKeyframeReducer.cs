using UnityEngine;

namespace Unity.LiveCapture
{
    class EulerKeyframeReducerImpl : IKeyframeReducerImpl<Keyframe<Vector3>>
    {
        public static EulerKeyframeReducerImpl Instance { get; } = new EulerKeyframeReducerImpl();

        public bool CanReduce(Keyframe<Vector3> value, Keyframe<Vector3> first, Keyframe<Vector3> second, float maxError)
        {
            var reduced = Vector3KeyframeReducerImpl.Evaluate(value.Time, first, second);

            return !EulerDistanceError(value.Value, reduced, maxError);
        }

        static bool EulerDistanceError(Vector3 value, Vector3 reduced, float eulerError)
        {
            return (value - reduced).sqrMagnitude > eulerError * eulerError;
        }
    }

    class EulerKeyframeReducer : KeyframeReducer<Keyframe<Vector3>>
    {
        public EulerKeyframeReducer() : base(EulerKeyframeReducerImpl.Instance) { }
    }
}
