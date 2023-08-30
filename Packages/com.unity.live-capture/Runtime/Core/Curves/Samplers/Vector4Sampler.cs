using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector4Interpolator : IInterpolator<Vector4>
    {
        public static Vector4Interpolator Instance { get; } = new Vector4Interpolator();

        public Vector4 Interpolate(in Vector4 a, in Vector4 b, float t)
        {
            return Vector4.Lerp(a, b, t);
        }
    }

    class Vector4Sampler : Sampler<Vector4>
    {
        public Vector4Sampler() : base(Vector4Interpolator.Instance) { }
    }
}
