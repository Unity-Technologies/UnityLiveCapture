using UnityEngine;

namespace Unity.LiveCapture
{
    class FloatInterpolator : IInterpolator<float>
    {
        public static FloatInterpolator Instance { get; } = new FloatInterpolator();

        public float Interpolate(in float a, in float b, float t)
        {
            return Mathf.Lerp(a, b, t);
        }
    }

    class FloatSampler : Sampler<float>
    {
        public FloatSampler() : base(FloatInterpolator.Instance) { }
    }
}
