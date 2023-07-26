using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A base class that implements the default behaviour for a <see cref="ITimedDataSource"/>.
    /// </summary>
    [Serializable]
    public class TimedDataSource : ITimedDataSource
    {
        [SerializeField, HideInInspector]
        Object m_Object;
        [SerializeField, HideInInspector]
        string m_Guid;
        [SerializeField, HideInInspector]
        int m_BufferSize = 5;
        [SerializeField, HideInInspector]
        FrameTime m_Offset;
        [SerializeField]
        bool m_IsSynchronized;

        ITimedDataBuffer m_Buffer;
        ISynchronizer m_Synchronizer;

        /// <inheritdoc/>
        string IRegistrable.Id => m_Guid;

        /// <inheritdoc/>
        string IRegistrable.FriendlyName => m_Object != null ? m_Object.name : GetType().Name;

        /// <inheritdoc/>
        public FrameRate FrameRate
        {
            get => m_Buffer?.FrameRate ?? default;
            set
            {
                if (m_Buffer != null)
                {
                    m_Buffer.FrameRate = value;
                }
            }
        }

        /// <inheritdoc/>
        public int BufferSize
        {
            get => m_BufferSize;
            set
            {
                m_BufferSize = value;

                if (m_Buffer != null)
                {
                    m_Buffer.Capacity = value;
                }
            }
        }

        /// <inheritdoc/>
        public int? MaxBufferSize { get; set; }

        /// <inheritdoc/>
        public int? MinBufferSize { get; set; }

        /// <inheritdoc/>
        public FrameTime Offset
        {
            get => m_Offset;
            set => m_Offset = value;
        }

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            get => m_IsSynchronized;
            set => m_IsSynchronized = value;
        }

        /// <inheritdoc/>
        public ISynchronizer Synchronizer
        {
            get => m_Synchronizer;
            set => m_Synchronizer = value;
        }

        /// <inheritdoc />
        Object ITimedDataSource.UndoTarget => m_Object;

        /// <summary>
        /// The object that owns this data source.
        /// </summary>
        public Object SourceObject
        {
            get => m_Object;
            set => m_Object = value;
        }

        /// <summary>
        /// Is the data source ready to buffer samples.
        /// </summary>
        public bool Enabled => m_Buffer != null;

        /// <summary>
        /// An event invoked when this source presents a synchronized frame.
        /// </summary>
        /// <remarks>
        /// This method is only invoked when <see cref="IsSynchronized"/> is <see langword="true"/>.
        /// * The parameter is the sample time for the presented frame.
        /// </remarks>
        public event Action<FrameTimeWithRate> FramePresented;

        /// <summary>
        /// Activates the data source.
        /// </summary>
        /// <param name="buffer">The buffer to use for storing frame samples.</param>
        public void Enable(ITimedDataBuffer buffer)
        {
            if (Enabled || buffer == null)
            {
                return;
            }

            TimedDataSourceManager.Instance.EnsureIdIsValid(ref m_Guid);
            TimedDataSourceManager.Instance.Register(this);

            m_Buffer = buffer;
            m_Buffer.Capacity = m_BufferSize;
        }

        /// <summary>
        /// Deactivates the data source.
        /// </summary>
        public void Disable()
        {
            if (!Enabled)
            {
                return;
            }

            TimedDataSourceManager.Instance.Unregister(this);

            // clear so samples are discarded first and may be disposed if needed
            m_Buffer.Clear();
            m_Buffer = null;
        }

        /// <inheritdoc/>
        public bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample)
        {
            if (m_Buffer != null && m_Buffer.TryGetBufferRange(out oldestSample, out newestSample))
            {
                oldestSample += m_Offset;
                newestSample += m_Offset;
                return true;
            }

            oldestSample = default;
            newestSample = default;
            return false;
        }

        /// <inheritdoc cref="TimedDataBuffer{T}.TryGetSample"/>
        public TimedSampleStatus TryGetSample<T>(FrameTime frame, out T value)
        {
            if (m_Buffer is TimedDataBuffer<T> buffer)
            {
                return buffer.TryGetSample(frame - m_Offset, out value);
            }

            value = default;
            return TimedSampleStatus.DataMissing;
        }

        /// <inheritdoc cref="TimedDataBuffer{T}.GetSamplesInRange"/>
        public IEnumerable<(FrameTime time, T value)> GetSamplesInRange<T>(FrameTime from, FrameTime to)
        {
            if (m_Buffer is TimedDataBuffer<T> buffer)
            {
                foreach (var sample in buffer.GetSamplesInRange(from - m_Offset, to - m_Offset))
                {
                    yield return (sample.time + m_Offset, sample.value);
                }
            }
        }

        /// <inheritdoc/>
        TimedSampleStatus ITimedDataSource.PresentAt(FrameTimeWithRate presentTime)
        {
            Debug.Assert(IsSynchronized, "Attempted to call PresentAt() when data source is not being synchronized.");

            if (m_Buffer == null)
            {
                return TimedSampleStatus.DataMissing;
            }

            var localPresentTime = presentTime.Remap(FrameRate);
            var status = m_Buffer.GetStatus(localPresentTime.Time - m_Offset);

            if (status != TimedSampleStatus.DataMissing)
            {
                try
                {
                    FramePresented?.Invoke(localPresentTime);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return status;
        }

        /// <summary>
        /// Removes all frames from the data buffer.
        /// </summary>
        public void ClearBuffer()
        {
            m_Buffer?.Clear();
        }

        /// <summary>
        /// Adds a new data sample to the buffer.
        /// </summary>
        /// <typeparam name="T">The datatype of the sample.</typeparam>
        /// <param name="value">The sample value to add.</param>
        /// <param name="time">The time of the sample.</param>
        /// <seealso cref="AddSampleWithGeneratedTime"/>
        public void AddSample<T>(in T value, in FrameTimeWithRate time)
        {
            if (m_Buffer is ITimedDataBuffer<T> buffer)
            {
                buffer.Add(value, time);
            }
        }

        /// <summary>
        /// Adds a new data sample to the buffer.
        /// </summary>
        /// <remarks>
        /// Use this when a device does not provide timecode information with the samples, and instead relies on genlock to
        /// facilitate synchronization. When this data source is assigned to a synchronizer, a timecode will be generated using
        /// the synchronizer's presentation time. This should usually be called once per frame. When called multiple times
        /// in a single frame, the new value will overwrite any previous values submitted for the current frame.
        /// </remarks>
        /// <typeparam name="T">The datatype of the sample.</typeparam>
        /// <param name="value">The sample value to add.</param>
        /// <returns>The generated sample time.</returns>
        /// <seealso cref="AddSample"/>
        public FrameTimeWithRate AddSampleWithGeneratedTime<T>(in T value)
        {
            var time = default(FrameTimeWithRate);

            if (m_Buffer is TimedDataBuffer<T> buffer)
            {
                var presentTime = m_Synchronizer?.PresentTime;

                if (presentTime != null && presentTime.Value.Rate == FrameRate)
                {
                    time = presentTime.Value;
                }
                else
                {
                    time = GenerateFrameTime(FrameRate);
                }

                buffer.Add(value, time);
            }

            return time;
        }

        /// <summary>
        /// Gets a time based on the current engine time.
        /// </summary>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <returns>The generated time.</returns>
        public static FrameTimeWithRate GenerateFrameTime(in FrameRate frameRate)
        {
            return FrameTimeWithRate.FromSeconds(frameRate, Time.unscaledTimeAsDouble);
        }
    }
}
