using UnityEngine;

namespace Unity.LiveCapture
{
    class IntegerKeyframeReducerImpl : IKeyframeReducerImpl<Keyframe>
    {
        public static IntegerKeyframeReducerImpl Instance { get; } = new IntegerKeyframeReducerImpl();

        public bool CanReduce(Keyframe value, Keyframe first, Keyframe second, float maxError)
        {
            return value.value == first.value;
        }
    }

    class IntegerKeyframeReducer : KeyframeReducer<Keyframe>
    {
        public IntegerKeyframeReducer() : base(IntegerKeyframeReducerImpl.Instance) { }
    }
}
