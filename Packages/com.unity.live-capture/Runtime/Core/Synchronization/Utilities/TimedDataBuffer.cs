using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents a buffer of data samples ordered by the <see cref="FrameTime"/> of each sample.
    /// </summary>
    public interface ITimedDataBuffer
    {
        /// <summary>
        /// The number of elements stored in the buffer.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The maximum number of elements which can be stored in the collection.
        /// </summary>
        /// <remarks>If the new size is smaller than the current  <see cref="Count"/>, elements will be truncated from the front.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the capacity is not greater than zero.</exception>
        int Capacity { get; set; }

        /// <summary>
        /// The frame rate of the buffered samples.
        /// </summary>
        /// <remarks>
        /// This will remap the frame time of all samples in the buffer to match the new frame rate.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if attempting to set an invalid value.</exception>
        FrameRate FrameRate { get; set; }

        /// <summary>
        /// Removes all samples in the buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the frame time of the newest and oldest samples in the buffer.
        /// </summary>
        /// <param name="oldestSample">The frame time of the oldest sample in the buffer, or <see langword="default"/> if there are no samples in the buffer.</param>
        /// <param name="newestSample">The frame time of the newest sample in the buffer, or <see langword="default"/> if there are no samples in the buffer.</param>
        /// <returns><see langword="true"/> if there are any samples in the buffer; otherwise, <see langword="false"/>.</returns>
        bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample);

        /// <summary>
        /// Retrieves the status of the buffer at the specified time.
        /// </summary>
        /// <param name="frameTime">The time to retrieve the status for, expressed relative to the <see cref="FrameRate"/>.</param>
        /// <returns>The status of the retrieved sample.</returns>
        TimedSampleStatus GetStatus(FrameTime frameTime);
    }

    /// <summary>
    /// Represents a buffer of data samples ordered by the <see cref="FrameTime"/> of each sample.
    /// </summary>
    /// <typeparam name="T">The datatype of the samples.</typeparam>
    public interface ITimedDataBuffer<T> : ITimedDataBuffer, IEnumerable<(FrameTime frameTime, T value)>
    {
        /// <summary>
        /// Gets the sample time and value at the specified index in the buffer.
        /// </summary>
        (FrameTime frameTime, T value) this[int index] { get; }

        /// <summary>
        /// The interpolator used when <see cref="TryGetSample"/> is called for a time which
        /// lies between two samples.
        /// </summary>
        IInterpolator<T> Interpolator { get; set; }

        /// <summary>
        /// Retrieves a sample from the buffer at the specified time.
        /// </summary>
        /// <param name="frame">The time to retrieve the sample data for, expressed relative to the <see cref="FrameRate"/>.</param>
        /// <param name="sampleValue">The retrieved sample, if successful.</param>
        /// <remarks>
        /// If there isn't a sample at the specified time, the following occurs:
        /// <para>
        /// a) if the time is bounded by the oldest and newest samples contained in the buffer,
        /// <paramref name="sampleValue"/> is set to the nearest sample and returns <see cref="TimedSampleStatus.Ok"/>.<br/>
        /// b) if the time is outside of the buffer but there is at least one buffered sample,
        /// <paramref name="sampleValue"/> is set to the newest or oldest buffered sample (whichever is closer)
        /// and returns <see cref="TimedSampleStatus.Behind"/> or <see cref="TimedSampleStatus.Ahead"/> respectively.<br/>
        /// c) otherwise, returns <see cref="TimedSampleStatus.DataMissing"/> and sets <paramref name="sampleValue"/> to <see langword="default"/>.
        /// </para>
        /// </remarks>
        /// <returns>The status of the retrieved sample.</returns>
        TimedSampleStatus TryGetSample(FrameTime frame, out T sampleValue);

        /// <summary>
        /// Adds a new sample to the buffer.
        /// </summary>
        /// <remarks>
        /// When the buffer is full, the oldest sample in the buffer is discarded.
        /// The sample is inserted into the buffer so that the samples are ordered by increasing time.
        /// If a sample with the specified time already exists in the buffer, its value is updated with the new value.
        /// </remarks>
        /// <param name="value">The sample value.</param>
        /// <param name="frameTime">The time of the sample in frames.</param>
        void Add(in T value, in FrameTime frameTime);
    }

    /// <summary>
    /// A circular buffer of data samples ordered by the <see cref="FrameTime"/> of each sample.
    /// </summary>
    /// <typeparam name="T">The datatype of the samples.</typeparam>
    class TimedDataBuffer<T> : ITimedDataBuffer<T>
    {
        const int k_DefaultBufferCapacity = 5;

        FrameRate m_FrameRate;
        CircularBuffer<(FrameTime frameTime, T value)> m_Buffer = new CircularBuffer<(FrameTime frameTime, T value)>(k_DefaultBufferCapacity);

        /// <inheritdoc/>
        public int Count => m_Buffer.Count;

        /// <inheritdoc/>
        public int Capacity
        {
            get => m_Buffer.Capacity;
            set => m_Buffer.Capacity = value;
        }

        /// <inheritdoc/>
        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set => SetFrameRate(value);
        }

        /// <inheritdoc/>
        public IInterpolator<T> Interpolator { get; set; }

        /// <inheritdoc />
        public (FrameTime frameTime, T value) this[int index] => m_Buffer[index];

        /// <summary>
        /// Creates a new <see cref="TimedDataBuffer{T}"/> instance.
        /// </summary>
        /// <param name="frameRate">The frame rate of the samples.</param>
        /// <param name="capacity">The capacity of the buffer.</param>
        /// <param name="sampleDiscard">A callback invoked for each sample that is discarded from the buffer. This may be used
        /// to dispose samples if needed.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="frameRate"/> is invalid.</exception>
        public TimedDataBuffer(FrameRate frameRate, int capacity = 5, Action<T> sampleDiscard = null)
        {
            if (!frameRate.IsValid)
                throw new ArgumentException("Cannot be invalid.", nameof(frameRate));

            m_FrameRate = frameRate;
            m_Buffer.Capacity = capacity;

            if (sampleDiscard != null)
            {
                m_Buffer.ElementDiscarded += (s) => sampleDiscard(s.Item2);
            }
        }

        void SetFrameRate(FrameRate frameRate)
        {
            if (!frameRate.IsValid)
                throw new ArgumentException("Cannot be invalid.", nameof(frameRate));
            if (m_FrameRate == frameRate)
                return;

            for (var i = 0; i < m_Buffer.Count; i++)
            {
                var value = m_Buffer[i];
                value.frameTime = FrameTime.Remap(value.frameTime, m_FrameRate, frameRate);
                m_Buffer[i] = value;
            }

            m_FrameRate = frameRate;
        }

        /// <inheritdoc/>
        public bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample)
        {
            if (m_Buffer.Count == 0)
            {
                oldestSample = default;
                newestSample = default;
                return false;
            }

            ref readonly var front = ref m_Buffer.PeekFront();
            ref readonly var back = ref m_Buffer.PeekBack();

            oldestSample = front.frameTime;
            newestSample = back.frameTime;
            return true;
        }

        /// <inheritdoc/>
        public TimedSampleStatus TryGetSample(FrameTime frame, out T sampleValue)
        {
            sampleValue = default;

            if (m_Buffer.Count == 0)
            {
                return TimedSampleStatus.DataMissing;
            }

            ref readonly var oldestSample = ref m_Buffer.PeekFront();

            if (frame < oldestSample.frameTime)
            {
                sampleValue = oldestSample.value;
                return TimedSampleStatus.Ahead;
            }

            ref readonly var newestSample = ref m_Buffer.PeekBack();

            if (frame > newestSample.frameTime)
            {
                sampleValue = newestSample.value;
                return TimedSampleStatus.Behind;
            }

            for (var i = 0; i < m_Buffer.Count; i++)
            {
                ref readonly var nextSample = ref m_Buffer.PeekIndex(i);

                if (nextSample.frameTime < frame)
                {
                    continue;
                }
                if (nextSample.frameTime == frame)
                {
                    sampleValue = nextSample.value;
                    return TimedSampleStatus.Ok;
                }

                ref readonly var prevSample = ref m_Buffer.PeekIndex(i - 1);

                if (Interpolator != null)
                {
                    var fac = (float)(frame - prevSample.frameTime) / (float)(nextSample.frameTime - prevSample.frameTime);
                    sampleValue = Interpolator.Interpolate(prevSample.value, nextSample.value, fac);
                }
                else
                {
                    var prevDiff = Math.Abs((double)(frame - prevSample.frameTime));
                    var nextDiff = Math.Abs((double)(frame - nextSample.frameTime));
                    sampleValue = prevDiff < nextDiff ? prevSample.value : nextSample.value;
                }

                return TimedSampleStatus.Ok;
            }

            return TimedSampleStatus.DataMissing;
        }

        /// <inheritdoc/>
        public TimedSampleStatus GetStatus(FrameTime frame)
        {
            if (m_Buffer.Count == 0)
            {
                return TimedSampleStatus.DataMissing;
            }

            ref readonly var oldestSample = ref m_Buffer.PeekFront();

            if (frame < oldestSample.frameTime)
            {
                return TimedSampleStatus.Ahead;
            }

            ref readonly var newestSample = ref m_Buffer.PeekBack();

            if (frame > newestSample.frameTime)
            {
                return TimedSampleStatus.Behind;
            }

            return TimedSampleStatus.Ok;
        }

        /// <inheritdoc/>
        public void Add(in T value, in FrameTime frameTime)
        {
            var newSample = (frameTime, value);

            // find out where to insert the sample to keep them in order by time
            for (var i = m_Buffer.Count - 1; i >= 0; i--)
            {
                ref readonly var sample = ref m_Buffer.PeekIndex(i);

                if (sample.frameTime < frameTime)
                {
                    m_Buffer.PushIndex(i + 1, newSample);
                    return;
                }

                // if there is already a sample with the specified time, replace the data with the new value
                if (sample.frameTime == frameTime)
                {
                    m_Buffer[i] = newSample;
                    return;
                }
            }

            m_Buffer.PushIndex(0, newSample);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            m_Buffer.Clear();
        }

        /// <inheritdoc/>
        public IEnumerator<(FrameTime frameTime, T value)> GetEnumerator()
        {
            return m_Buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// A set of extensiom methods for <see cref="ITimedDataBuffer{T}"/>.
    /// </summary>
    public static class TimedBufferExtensions
    {

        /// <summary>
        /// Adds a new sample to the buffer.
        /// </summary>
        /// <remarks>
        /// When the buffer is full, the oldest sample in the buffer is discarded.
        /// The sample is inserted into the buffer so that the samples are ordered by increasing time.
        /// If a sample with the specified time already exists in the buffer, its value is updated with the new value.
        /// </remarks>
        /// <typeparam name="T">The datatype of the samples.</typeparam>
        /// <param name="buffer">The buffer to add to.</param>
        /// <param name="value">The sample value.</param>
        /// <param name="time">The time of the sample in seconds.</param>
        public static void Add<T>([NotNull] this ITimedDataBuffer<T> buffer, in T value, double time)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            buffer.Add(value, FrameTime.FromSeconds(buffer.FrameRate, time));
        }

        /// <summary>
        /// Adds a new sample to the buffer.
        /// </summary>
        /// <remarks>
        /// When the buffer is full, the oldest sample in the buffer is discarded.
        /// The sample is inserted into the buffer so that the samples are ordered by increasing time.
        /// If a sample with the specified time already exists in the buffer, its value is updated with the new value.
        /// </remarks>
        /// <typeparam name="T">The datatype of the samples.</typeparam>
        /// <param name="buffer">The buffer to add to.</param>
        /// <param name="value">The sample value.</param>
        /// <param name="frameTime">The time of the sample in frames.</param>
        public static void Add<T>([NotNull] this ITimedDataBuffer<T> buffer, in T value, in FrameTimeWithRate frameTime)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            buffer.Add(value, FrameTime.Remap(frameTime.Time, frameTime.Rate, buffer.FrameRate));
        }

        /// <summary>
        /// Retrieves the buffered samples that lie in a time range.
        /// </summary>
        /// <remarks>
        /// The range bounds are inclusive.
        /// </remarks>
        /// <typeparam name="T">The datatype of the samples.</typeparam>
        /// <param name="buffer">The buffer to add to.</param>
        /// <param name="from">The start time of the range, expressed relative to the <see cref="FrameRate"/>.</param>
        /// <param name="to">The end time of the range, expressed relative to the <see cref="FrameRate"/>.</param>
        /// <returns>An iterator that returns all samples in the specified range.</returns>
        public static IEnumerable<(FrameTime time, T value)> GetSamplesInRange<T>([NotNull] this ITimedDataBuffer<T> buffer, FrameTime from, FrameTime to)
        {
            if (!buffer.TryGetBufferRange(out var oldest, out var newest))
            {
                yield break;
            }

            // ensure the range bounds are ascending in time
            if (to < from)
            {
                var temp = to;
                to = from;
                from = temp;
            }

            // early outs to prevent reading through all the samples when we know none are in range
            if (to < oldest)
            {
                yield break;
            }
            if (from > newest)
            {
                yield break;
            }

            // iterate through the samples returning all in the range
            foreach (var sample in buffer)
            {
                if (from <= sample.frameTime && sample.frameTime <= to)
                {
                    yield return sample;
                }
                else if (to < sample.frameTime)
                {
                    yield break;
                }
            }
        }
    }

    /// <summary>
    /// A set of utility methods for <see cref="ITimedDataBuffer{T}"/>.
    /// </summary>
    public static class TimedDataBuffer
    {
        const int k_DefaultCapacity = 5;
        static readonly FrameRate k_DefaultBufferFrameRate = StandardFrameRate.FPS_60_00;

        /// <summary>
        /// Creates a new <see cref="ITimedDataBuffer{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The datatype of the samples.</typeparam>
        /// <param name="interpolator">The <see cref="IInterpolator{T}"/> to use. Can be <see langword="null"/>.</param>
        /// <param name="frameRate">The frame rate of the samples.</param>
        /// <param name="sampleDiscard">A callback invoked for each sample that is discarded from the buffer. This may be used
        /// to dispose samples if needed.</param>
        /// <returns>The new <see cref="ITimedDataBuffer{T}"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="frameRate"/> is invalid.</exception>
        public static ITimedDataBuffer<T> Create<T>(
            IInterpolator<T> interpolator = null,
            FrameRate? frameRate = null,
            Action<T> sampleDiscard = null)
        {
            return new TimedDataBuffer<T>(frameRate ?? k_DefaultBufferFrameRate, k_DefaultCapacity, sampleDiscard)
            {
                Interpolator = interpolator
            };
        }
    }
}
