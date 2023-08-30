using UnityEngine;

namespace Unity.LiveCapture
{
    class Vector3Interpolator : IInterpolator<Vector3>
    {
        public static Vector3Interpolator Instance { get; } = new Vector3Interpolator();

        public Vector3 Interpolate(in Vector3 a, in Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }
    }

    class Vector3Sampler : Sampler<Vector3>
    {
        public Vector3Sampler() : base(Vector3Interpolator.Instance) { }
    }
}
