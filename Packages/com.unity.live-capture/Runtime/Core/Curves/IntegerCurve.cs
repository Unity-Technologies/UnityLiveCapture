using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A type of <see cref="ICurve"/> that stores keyframes of type int.
    /// </summary>
    public class IntegerCurve : ICurve<int>
    {
        IntegerSampler m_Sampler = new IntegerSampler();
        IntegerTangentUpdater m_TangentUpdater = new IntegerTangentUpdater();
        IntegerKeyframeReducer m_Reducer = new IntegerKeyframeReducer();
        AnimationCurve m_Curve = new AnimationCurve();

        /// <inheritdoc/>
        public float MaxError
        {
            get => m_Reducer.MaxError;
            set => m_Reducer.MaxError = value;
        }

        /// <inheritdoc/>
        public FrameRate FrameRate
        {
            get => m_Sampler.FrameRate;
            set => m_Sampler.FrameRate = value;
        }

        /// <summary>
        /// The baked animation curve.
        /// </summary>
        internal AnimationCurve AnimationCurve => m_Curve;

        /// <inheritdoc/>
        public void AddKey(double time, in int value)
        {
            m_Sampler.Add((float)time, value);

            Sample();
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return m_TangentUpdater.IsEmpty()
                && m_Reducer.IsEmpty()
                && m_Curve.length == 0;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            m_Sampler.Reset();
            m_TangentUpdater.Reset();
            m_Reducer.Reset();
            m_Curve = new AnimationCurve();
        }

        /// <inheritdoc/>
        public void SetToAnimationClip(PropertyBinding binding, AnimationClip clip)
        {
            Flush();

            if (m_Curve.length == 0)
            {
                return;
            }

            clip.SetCurve(binding.RelativePath, binding.Type, binding.PropertyName, m_Curve);
        }

        void Flush()
        {
            m_Sampler.Flush();
            m_TangentUpdater.Flush();
            m_Reducer.Flush();

            Sample();
        }

        void Sample()
        {
            while (m_Sampler.MoveNext())
            {
                var sample = m_Sampler.Current;

                m_TangentUpdater.Add(new Keyframe(sample.Time, sample.Value));
            }

            while (m_TangentUpdater.MoveNext())
            {
                m_Reducer.Add(m_TangentUpdater.Current);
            }

            while (m_Reducer.MoveNext())
            {
                m_Curve.AddKey(m_Reducer.Current);
            }
        }
    }
}
