using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A type of <see cref="ICurve"/> that stores keyframes of type <see cref="Vector3"/>.
    /// </summary>
    public class Vector3Curve : ICurve<Vector3>, IReduceableCurve
    {
        Vector3Sampler m_Sampler = new Vector3Sampler();
        Vector3TangentUpdater m_TangentUpdater = new Vector3TangentUpdater();
        Vector3KeyframeReducer m_Reducer = new Vector3KeyframeReducer();
        AnimationCurve[] m_Curves = new[]
        {
            new AnimationCurve(),
            new AnimationCurve(),
            new AnimationCurve()
        };

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

        /// <inheritdoc/>
        public void AddKey(double time, in Vector3 value)
        {
            m_Sampler.Add((float)time, value);

            Sample();
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return m_TangentUpdater.IsEmpty()
                && m_Reducer.IsEmpty()
                && m_Curves[0].length == 0;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Reset();
        }

        /// <inheritdoc/>
        public void SetToAnimationClip(PropertyBinding binding, AnimationClip clip)
        {
            Flush();

            if (m_Curves[0].length > 0)
                clip.SetCurve(binding.RelativePath, binding.Type, $"{binding.PropertyName}.x", m_Curves[0]);
            if (m_Curves[1].length > 0)
                clip.SetCurve(binding.RelativePath, binding.Type, $"{binding.PropertyName}.y", m_Curves[1]);
            if (m_Curves[2].length > 0)
                clip.SetCurve(binding.RelativePath, binding.Type, $"{binding.PropertyName}.z", m_Curves[2]);
        }

        void Reset()
        {
            m_Sampler.Reset();
            m_TangentUpdater.Reset();
            m_Reducer.Reset();
            m_Curves = new[]
            {
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve()
            };
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

                m_TangentUpdater.Add(new Keyframe<Vector3>()
                {
                    Time = sample.Time,
                    Value = sample.Value
                });
            }

            while (m_TangentUpdater.MoveNext())
            {
                m_Reducer.Add(m_TangentUpdater.Current);
            }

            while (m_Reducer.MoveNext())
            {
                AddKey(m_Reducer.Current);
            }
        }

        void AddKey(Keyframe<Vector3> keyframe)
        {
            for (var i = 0; i < 3; ++i)
            {
                m_Curves[i].AddKey(new Keyframe()
                {
                    time = keyframe.Time,
                    value = keyframe.Value[i],
                    inTangent = keyframe.InTangent[i],
                    outTangent = keyframe.OutTangent[i]
                });
            }
        }
    }
}
