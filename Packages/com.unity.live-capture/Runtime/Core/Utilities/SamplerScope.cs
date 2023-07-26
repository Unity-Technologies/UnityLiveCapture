using System;
using UnityEngine.Profiling;

namespace Unity.LiveCapture
{
    // A helper to simplify profiling methods with complex flow control.
    struct CustomSamplerScope : IDisposable
    {
        readonly CustomSampler m_Sampler;

        public CustomSamplerScope(CustomSampler sampler)
        {
            m_Sampler = sampler;
            m_Sampler.Begin();
        }

        public void Dispose()
        {
            m_Sampler.End();
        }
    }
}
