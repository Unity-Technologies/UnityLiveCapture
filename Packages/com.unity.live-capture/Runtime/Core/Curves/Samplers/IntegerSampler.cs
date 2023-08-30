namespace Unity.LiveCapture
{
    class IntegerInterpolator : IInterpolator<int>
    {
        public static IntegerInterpolator Instance { get; } = new IntegerInterpolator();

        public int Interpolate(in int a, in int b, float t)
        {
            return t == 1 ? b : a;
        }
    }

    class IntegerSampler : Sampler<int>
    {
        public IntegerSampler() : base(IntegerInterpolator.Instance) { }
    }
}
