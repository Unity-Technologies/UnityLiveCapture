using UnityEngine;

namespace Unity.LiveCapture.Cameras
{
    /// <summary>
    /// A type of <see cref="LiveStreamCaptureDevice"/> that provides the common functionality required to
    /// implement support for third-party camera tracking devices.
    /// </summary>
    /// <seealso cref="LiveStreamCaptureDevice"/>
    public abstract class CameraTrackingDevice : LiveStreamCaptureDevice
    {
        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        TimedDataSource m_SyncBuffer = new TimedDataSource();

        bool m_AddingFrame;

        /// <summary>
        /// The camera currently assigned to this device.
        /// </summary>
        public Camera Camera
        {
            get => m_Camera;
            set => m_Camera = value;
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_SyncBuffer.FramePresented += PresentAt;
            m_SyncBuffer.Enable(CreateTimedDataBuffer());
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            m_SyncBuffer.Disable();
            m_SyncBuffer.FramePresented -= PresentAt;
        }

        /// <summary>
        /// Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        /// </summary>
        /// <remarks>
        /// You would usually use this to perform an action after a value changes in the Inspector;
        /// for example, making sure that data stays within a certain range.
        /// </remarks>
        protected virtual void OnValidate()
        {
            m_SyncBuffer.SourceObject = this;
        }

        /// <inheritdoc/>
        public override void Write(ITakeBuilder takeBuilder)
        {
            if (Camera == null)
            {
                return;
            }

            if (!Camera.TryGetComponent<Animator>(out var animator))
            {
                animator = Camera.gameObject.AddComponent<Animator>();
            }

            var startTime = m_SyncBuffer.IsSynchronized ? FirstFrameTime : null;

            takeBuilder.CreateAnimationTrack(
                "Camera",
                animator,
                BakeAnimationClip(),
                null,
                startTime
            );
        }

        /// <summary>
        /// Creates a new <see cref="ITimedDataBuffer" />.
        /// </summary>
        /// <returns>The new <see cref="ITimedDataBuffer" />.</returns>
        protected abstract ITimedDataBuffer CreateTimedDataBuffer();

        /// <summary>
        /// Clears all frames in the synchronization buffer.
        /// </summary>
        protected void ResetSyncBuffer()
        {
            m_SyncBuffer.ClearBuffer();
        }

        /// <summary>
        /// Process a new frame of data.
        /// </summary>
        /// <typeparam name="T">The datatype of the samples.</typeparam>
        /// <param name="frame">The frame to add.</param>
        /// <param name="frameTime">The timecode of the frame. When <see langword="null"/>, a timecode will be generated.</param>
        protected void AddFrame<T>(T frame, FrameTimeWithRate? frameTime = null)
        {
            // Guard re-entry. We are calling ProcessFrame, from which the user could attempt
            // to call AddFrame again, entering in an infinite loop.
            if (m_AddingFrame)
            {
                return;
            }

            if (frameTime == null)
            {
                if (SyncManager.TryGetSyncRate(out var frameRate))
                {
                    m_SyncBuffer.FrameRate = frameRate;
                }
                else
                {
                    m_SyncBuffer.FrameRate = TakeRecorder.FrameRate;
                }

                var presentTime = m_SyncBuffer.Synchronizer?.PresentTime;

                if (m_SyncBuffer.IsSynchronized
                    && presentTime != null
                    && presentTime.Value.Rate == m_SyncBuffer.FrameRate)
                {
                    frameTime = presentTime.Value;
                }
                else
                {
                    frameTime = TimedDataSource.GenerateFrameTime(m_SyncBuffer.FrameRate);
                }
            }
            else
            {
                m_SyncBuffer.FrameRate = frameTime.Value.Rate;
            }

            m_SyncBuffer.AddSample(frame, frameTime.Value);

            if (!m_SyncBuffer.IsSynchronized)
            {
                PresentAt(frameTime.Value);
            }
        }

        /// <summary>
        /// Gets the sample at the specified frame time.
        /// </summary>
        /// <typeparam name="T">The datatype of the samples.</typeparam>
        /// <returns>The sample at the specified frame time.</returns>
        protected T GetCurrentFrame<T>()
        {
            if (CurrentFrameTime.HasValue
                && m_SyncBuffer.TryGetSample<T>(CurrentFrameTime.Value.Time, out var sample) != TimedSampleStatus.DataMissing)
            {
                return sample;
            }

            return default(T);
        }

        void PresentAt(FrameTimeWithRate frameTime)
        {
            if (m_AddingFrame)
            {
                return;
            }

            m_AddingFrame = true;

            var root = default(Transform);

            if (m_Camera != null)
            {
                root = m_Camera.transform;
            }

            // Process every frame between the last presentation time and the current one when recording.
            if (IsRecording && CurrentFrameTime.HasValue)
            {
                var currentFrameTime = CurrentFrameTime.Value;

                while (++currentFrameTime < frameTime)
                {
                    UpdateStream(root, currentFrameTime);
                }
            }

            UpdateStream(root, frameTime);

            m_AddingFrame = false;
        }
    }
}
