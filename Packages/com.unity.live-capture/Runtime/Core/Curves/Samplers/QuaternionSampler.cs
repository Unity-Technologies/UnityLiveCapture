using UnityEngine;

namespace Unity.LiveCapture
{
    class QuaternionInterpolator : IInterpolator<Quaternion>
    {
        public static QuaternionInterpolator Instance { get; } = new QuaternionInterpolator();

        public Quaternion Interpolate(in Quaternion a, in Quaternion b, float t)
        {
            return Quaternion.Slerp(a, b, t);
        }
    }
    class QuaternionSampler : Sampler<Quaternion>
    {
        public QuaternionSampler() : base(QuaternionInterpolator.Instance) { }
    }
}
