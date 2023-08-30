using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Unity.LiveCapture.TransformCapture
{
    /// <summary>
    /// A capture device to record transform hierarchies.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [CreateDeviceMenuItemAttribute("Transform Capture Device")]
    [HelpURL(Documentation.baseURL + "ref-component-transform-capture-device" + Documentation.endURL)]
    public class TransformCaptureDevice : LiveCaptureDevice
    {
        struct TransformCaptureRecorderUpdate
        {
        }

        [SerializeField]
        Animator m_Actor;
        [SerializeField]
        AvatarMask m_AvatarMask;
        [SerializeField]
        TransformRecorder m_Recorder = new TransformRecorder();

        /// <summary>
        /// The Animator currently assigned to this device.
        /// </summary>
        public Animator Actor
        {
            get => m_Actor;
            set => m_Actor = value;
        }

        /// <summary>
        /// The AvatarMask currently assigned to this device.
        /// </summary>
        public AvatarMask AvatarMask
        {
            get => m_AvatarMask;
            set => m_AvatarMask = value;
        }

        void OnValidate()
        {
            m_Recorder.Validate();
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            PlayerLoopExtensions.RegisterUpdate<PostLateUpdate.DirectorLateUpdate, TransformCaptureRecorderUpdate>(Record);
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            PlayerLoopExtensions.DeregisterUpdate<TransformCaptureRecorderUpdate>(Record);
        }

        /// <inheritdoc/>
        public override bool IsReady()
        {
            return m_Actor != null;
        }

        /// <inheritdoc/>
        protected override void OnStartRecording()
        {
            if (IsReady())
            {
                m_Recorder.Prepare(m_Actor, m_AvatarMask, TakeRecorder.FrameRate);

                Record();
            }
        }

        /// <inheritdoc/>
        public override void Write(ITakeBuilder takeBuilder)
        {
            if (!IsReady())
            {
                return;
            }

            var name = m_Actor.gameObject.name;
            var animationClip = new AnimationClip();

            m_Recorder.SetToAnimationClip(animationClip);

            takeBuilder.CreateAnimationTrack(name, m_Actor, animationClip);
        }

        void Record()
        {
            if (IsRecording)
            {
                var elapsedTime = (float)TakeRecorder.GetRecordingElapsedTime();

                m_Recorder.Record(elapsedTime);
            }
        }
    }
}
