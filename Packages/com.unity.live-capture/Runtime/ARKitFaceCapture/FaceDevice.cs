using System;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// A device used to control face animation capture and playback.
    /// </summary>
    /// <remarks>
    /// The face capture data is in the format of Apple's ARKit face tracking.
    /// </remarks>
    /// <seealso href="https://developer.apple.com/documentation/arkit/tracking_and_visualizing_faces"/>
    [ExcludeFromPreset]
    [CreateDeviceMenuItemAttribute("ARKit Face Device")]
    [AddComponentMenu("Live Capture/ARKit Face Capture/ARKit Face Device")]
    [HelpURL(Documentation.baseURL + "ref-component-arkit-face-device" + Documentation.endURL)]
    public class FaceDevice : CompanionAppDevice<IFaceClient>
    {
        class Interpolator : IInterpolator<FacePose>
        {
            public static Interpolator Instance { get; } = new Interpolator();

            public FacePose Interpolate(in FacePose a, in FacePose b, float t)
            {
                FacePose.Interpolate(a, b, t, out var result);
                return result;
            }
        }

        [SerializeField]
        FaceActor m_Actor;

        [SerializeField, HideInInspector, EnumFlagButtonGroup(100f)]
        FaceChannelFlags m_Channels = FaceChannelFlags.All;

        [SerializeField]
        FaceDeviceRecorder m_Recorder = new FaceDeviceRecorder();

        [SerializeField]
        TimedDataSource m_SyncBuffer = new TimedDataSource();

        [SerializeField, HideInInspector]
        FacePose m_Pose = FacePose.Identity;

        FacePose m_ReceivedPose;
        FacePose m_SynchronizedPose;
        FrameTimeWithRate? m_LastRecordTime;

        /// <summary>
        /// Gets the <see cref="FaceActor"/> currently assigned to this device.
        /// </summary>
        /// <returns>The assigned actor, or null if none is assigned.</returns>
        public FaceActor Actor
        {
            get => m_Actor;
            set
            {
                if (m_Actor != value)
                {
                    m_Actor = value;
                    RegisterLiveProperties();
                }
            }
        }

        /// <summary>
        /// The synchronized data buffer.
        /// </summary>
        public ITimedDataSource SyncBuffer => m_SyncBuffer;

        bool TryGetInternalClient(out IFaceClientInternal client)
        {
            client = GetClient() as IFaceClientInternal;

            return client != null;
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_SyncBuffer.FramePresented += PresentAt;
            m_SyncBuffer.Enable(TimedDataBuffer.Create<FacePose>(Interpolator.Instance));
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled.
        /// </summary>
        /// <remaks>
        /// This is also called when the object is destroyed and can be used for any cleanup code.
        ///  When scripts are reloaded after compilation has finished, OnDisable will be called, followed by an OnEnable after the script has been loaded.
        /// </remaks>
        protected override void OnDisable()
        {
            base.OnDisable();

            m_SyncBuffer.Disable();
            m_SyncBuffer.FramePresented -= PresentAt;
        }

        void OnValidate()
        {
            m_SyncBuffer.SourceObject = this;

            m_Recorder.Validate();
        }

        /// <summary>
        /// Indicates whether a device is ready for recording.
        /// </summary>
        /// <returns>
        /// true if ready for recording; otherwise, false.
        /// </returns>
        public override bool IsReady()
        {
            return base.IsReady() && m_Actor != null;
        }

        /// <summary>
        /// The device calls this method when the recording state has changed.
        /// </summary>
        protected override void OnStartRecording()
        {
            base.OnStartRecording();

            m_Recorder.FrameRate = TakeRecorder.FrameRate;
            m_Recorder.Channels = m_Channels;
            m_Recorder.OnReset = () =>
            {
                m_Recorder.Record(ref m_Pose);
            };
            m_Recorder.Prepare();

            m_LastRecordTime = null;
        }

        /// <summary>
        /// Gets the name used for the take asset name.
        /// </summary>
        /// <returns>The name of the asset.</returns>
        protected override string GetAssetName()
        {
            return m_Actor != null ? m_Actor.name : name;
        }

        /// <summary>
        /// The device calls this method when a new client is assigned.
        /// </summary>
        protected override void OnClientAssigned()
        {
            if (TryGetInternalClient(out var client))
            {
                client.FacePoseSampleReceived += OnFacePoseSampleReceived;
            }
        }

        /// <summary>
        /// The device calls this method when the client is unassigned.
        /// </summary>
        protected override void OnClientUnassigned()
        {
            if (TryGetInternalClient(out var client))
            {
                client.FacePoseSampleReceived -= OnFacePoseSampleReceived;
            }
        }

        /// <summary>
        /// The device calls this method when a live performance starts and properties are about to change.
        /// </summary>
        /// <param name="previewer">The <see cref="IPropertyPreviewer"/> to use to register live properties.</param>
        protected override void OnRegisterLiveProperties(IPropertyPreviewer previewer)
        {
            Debug.Assert(m_Actor != null);

            foreach (var previewable in m_Actor.GetComponents<IPreviewable>())
            {
                previewable.Register(previewer);
            }
        }

        /// <inheritdoc/>
        public override void Write(ITakeBuilder takeBuilder)
        {
            if (m_Actor == null || m_Recorder.IsEmpty())
            {
                return;
            }

            var startTime = m_SyncBuffer.IsSynchronized ? m_Recorder.InitialTime : null;

            takeBuilder.CreateAnimationTrack(
                "Face",
                m_Actor.Animator,
                m_Recorder.Bake(),
                alignTime: startTime);
        }

        /// <inheritdoc/>
        protected override void UpdateDevice()
        {
            UpdateClient();
        }

        /// <inheritdoc/>
        protected override void LiveUpdate()
        {
            Debug.Assert(m_Actor != null, "Actor is null");

            m_Pose = m_SyncBuffer.IsSynchronized ? m_SynchronizedPose : m_ReceivedPose;

            if (m_Channels.HasFlag(FaceChannelFlags.BlendShapes))
            {
                m_Actor.BlendShapes = m_Pose.BlendShapes;
            }

            if (m_Channels.HasFlag(FaceChannelFlags.HeadPosition))
            {
                m_Actor.HeadPosition = m_Pose.HeadPosition;
            }

            if (m_Channels.HasFlag(FaceChannelFlags.HeadRotation))
            {
                m_Actor.HeadOrientation = m_Pose.HeadOrientation.eulerAngles;
            }

            if (m_Channels.HasFlag(FaceChannelFlags.Eyes))
            {
                m_Actor.LeftEyeOrientation = m_Pose.LeftEyeOrientation.eulerAngles;
                m_Actor.RightEyeOrientation = m_Pose.RightEyeOrientation.eulerAngles;
            }

            m_Actor.BlendShapesEnabled = m_Channels.HasFlag(FaceChannelFlags.BlendShapes);
            m_Actor.HeadPositionEnabled = m_Channels.HasFlag(FaceChannelFlags.HeadPosition);
            m_Actor.HeadOrientationEnabled = m_Channels.HasFlag(FaceChannelFlags.HeadRotation);
            m_Actor.EyeOrientationEnabled = m_Channels.HasFlag(FaceChannelFlags.Eyes);
        }

        void OnFacePoseSampleReceived(FaceSample sample)
        {
            m_ReceivedPose = sample.FacePose;

            var time = FrameTimeWithRate.FromSeconds(m_SyncBuffer.FrameRate, sample.Time);

            m_SyncBuffer.AddSample(sample.FacePose, time);

            if (!m_SyncBuffer.IsSynchronized)
            {
                Record(ref sample.FacePose, time);
            }

            Refresh();
        }

        void PresentAt(FrameTimeWithRate frameTime)
        {
            if (m_SyncBuffer.TryGetSample<FacePose>(frameTime.Time, out var frame) != TimedSampleStatus.DataMissing)
            {
                PresentAt(frame, frameTime);
            }
        }

        void PresentAt(in FacePose value, FrameTimeWithRate time)
        {
            m_SynchronizedPose = value;

            if (m_LastRecordTime != null)
            {
                foreach (var sample in m_SyncBuffer.GetSamplesInRange<FacePose>(m_LastRecordTime.Value.Time, time.Time))
                {
                    var sampleValue = sample.value;
                    var sampleTime = new FrameTimeWithRate(m_SyncBuffer.FrameRate, sample.time);

                    Record(ref sampleValue, sampleTime);
                }
            }
            else
            {
                Record(ref m_SynchronizedPose, time);
            }
        }

        void Record(ref FacePose pose, FrameTimeWithRate time)
        {
            if (IsRecording)
            {
                m_Recorder.Channels = m_Channels;
                m_Recorder.Update(time.ToSeconds());
                m_Recorder.Record(ref pose);
                m_LastRecordTime = time;
            }
        }
    }
}
