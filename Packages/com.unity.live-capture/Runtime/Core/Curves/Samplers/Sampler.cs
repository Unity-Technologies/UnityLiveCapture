using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.LiveCapture
{
    struct Sample<T> where T : struct
    {
        public float Time { get; set; }
        public T Value { get; set; }
    }

    class Sampler<T> : IEnumerator<Sample<T>> where T : struct
    {
        enum State
        {
            BeginSampling,
            Sampling,
            EndSampling
        }

        const int k_MaxFramesBetweenSamples = 5;

        IInterpolator<T> m_Interpolator;
        Queue<Sample<T>> m_Samples = new Queue<Sample<T>>();
        Sample<T> m_LastSample;
        State m_State = State.BeginSampling;
        bool m_Flushing;
        FrameRate m_FrameRate = StandardFrameRate.FPS_30_00;
        FrameTime m_CurrentFrameTime;

        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set
            {
                if (m_FrameRate != value)
                {
                    m_CurrentFrameTime = FrameTime.Remap(m_CurrentFrameTime, m_FrameRate, value);
                    m_FrameRate = value;
                }
            }
        }

        public FrameTime FrameTime => m_CurrentFrameTime;
        public Sample<T> Current { get; private set; }
        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        protected Sampler(IInterpolator<T> interpolator)
        {
            if (interpolator == null)
                throw new ArgumentNullException(nameof(interpolator));

            m_Interpolator = interpolator;
        }

        public void Reset()
        {
            m_Samples.Clear();
            m_CurrentFrameTime = default(FrameTime);
            m_State = State.BeginSampling;
            m_Flushing = false;
        }

        public void Add(float time, T value)
        {
            m_Samples.Enqueue(new Sample<T>()
            {
                Time = time,
                Value = value
            });
        }

        public void Flush()
        {
            m_Flushing = true;
        }

        public bool IsEmpty()
        {
            return m_Samples.Count == 0;
        }

        public bool MoveNext()
        {
            if (m_Samples.Count == 0)
            {
                m_Flushing = false;

                return false;
            }

            if (m_Flushing)
            {
                m_State = State.BeginSampling;
            }

            if (m_State == State.BeginSampling)
            {
                m_State = State.Sampling;
                m_LastSample = m_Samples.Dequeue();
                m_CurrentFrameTime = FrameTime.FromSeconds(m_FrameRate, m_LastSample.Time);
                Current = m_LastSample;
            }
            else if (m_State == State.EndSampling)
            {
                if (!TryGetNextValidSample(out var nextSample))
                {
                    return false;
                }

                var nextFrameTime = FrameTime.FromSeconds(m_FrameRate, nextSample.Time);

                m_CurrentFrameTime = new FrameTime(nextFrameTime.FrameNumber - 1);

                Current = new Sample<T>()
                {
                    Time = (float)m_CurrentFrameTime.ToSeconds(m_FrameRate),
                    Value = Current.Value
                };

                m_State = State.BeginSampling;
            }
            else if (m_State == State.Sampling)
            {
                if (!TryGetNextValidSample(out var nextSample))
                {
                    return false;
                }

                var nextSampleFrameTime = FrameTime.FromSeconds(m_FrameRate, nextSample.Time);
                var nextFrameTime = new FrameTime(m_CurrentFrameTime.FrameNumber + 1);

                while (nextFrameTime > nextSampleFrameTime)
                {
                    m_LastSample = m_Samples.Dequeue();

                    if (!TryGetNextValidSample(out nextSample))
                    {
                        return false;
                    }

                    nextSampleFrameTime = FrameTime.FromSeconds(m_FrameRate, nextSample.Time);
                }

                var maxFramesCheck = (nextSampleFrameTime - m_CurrentFrameTime) >= new FrameTime(k_MaxFramesBetweenSamples);

                if (maxFramesCheck)
                {
                    Current = m_LastSample;
                    m_State = State.EndSampling;
                }
                else
                {
                    m_CurrentFrameTime = nextFrameTime;

                    var prevTime = m_LastSample.Time;
                    var nextTime = (float)nextFrameTime.ToSeconds(m_FrameRate);

                    if (MathUtility.CompareApproximately(prevTime, nextTime))
                    {
                        Current = m_LastSample;
                    }
                    else
                    {
                        var t = (nextTime - prevTime) / (nextSample.Time - prevTime);
                        var a = m_LastSample.Value;
                        var b = nextSample.Value;
                        var value = m_Interpolator.Interpolate(a, b, t);

                        Current = new Sample<T>()
                        {
                            Time = nextTime,
                            Value = value
                        };
                    }
                }
            }

            return true;
        }

        bool TryGetNextValidSample(out Sample<T> sample)
        {
            if (!m_Samples.TryPeek(out sample))
            {
                return false;
            }

            while (sample.Time <= m_LastSample.Time)
            {
                m_Samples.Dequeue();

                if (!m_Samples.TryPeek(out sample))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
