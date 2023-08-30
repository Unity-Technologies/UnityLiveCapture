using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector2Interpolator : IInterpolator<Vector2>
    {
        public static Vector2Interpolator Instance { get; } = new Vector2Interpolator();

        public Vector2 Interpolate(in Vector2 a, in Vector2 b, float t)
        {
            return Vector2.Lerp(a, b, t);
        }
    }

    class Vector2Sampler : Sampler<Vector2>
    {
        public Vector2Sampler() : base(Vector2Interpolator.Instance) { }
    }
}
