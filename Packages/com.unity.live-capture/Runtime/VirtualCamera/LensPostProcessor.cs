using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera.Rigs;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class SampleProcessor
    {
        // Due to a quirk in how LensPostProcessor works, if there's not enough keyframes
        // in the buffer, we can run into a situation where we constantly "miss" the
        // keyframes when performing the sampling. Setting the buffer size to some higher value
        // minimizes this possibility.
        const int k_MinBufferSize = 5;
        const int k_OutOfOrderFrameTolerance = -3;
        const int k_MinTimeShiftTolerance = 15;
        const float k_MaxSpeed = float.MaxValue;
        internal static readonly FrameRate k_KeyframeBufferFrameRate = StandardFrameRate.FPS_60_00;

        [SerializeField]
        Lens m_Lens;

        [SerializeField]
        int m_BufferSize = k_MinBufferSize;

        int m_TimeShiftTolerance = k_MinTimeShiftTolerance;
        bool m_RigNeedsInitialize;
        float m_FocusDistanceVelocity;
        float m_FocalLengthVelocity;
        float m_ApertureVelocity;

        ITimedDataBuffer<InputSample> m_InputKeyframes = TimedDataBuffer.Create<InputSample>(frameRate: k_KeyframeBufferFrameRate);
        ITimedDataBuffer<float> m_FocusDistanceKeyframes = TimedDataBuffer.Create<float>(frameRate: k_KeyframeBufferFrameRate);
        ITimedDataBuffer<float> m_FocalLengthKeyframes = TimedDataBuffer.Create<float>(frameRate: k_KeyframeBufferFrameRate);
        ITimedDataBuffer<float> m_ApertureKeyframes = TimedDataBuffer.Create<float>(frameRate: k_KeyframeBufferFrameRate);

        public int TimeShiftTolerance
        {
            get => m_TimeShiftTolerance;
            set => m_TimeShiftTolerance = Mathf.Max(value, k_MinTimeShiftTolerance);
        }

        public int MinBufferSize => k_MinBufferSize;

        public int BufferSize
        {
            get => m_BufferSize;
            set
            {
                m_BufferSize = value;
                Validate();
            }
        }

        public Func<Lens> GetLensTarget { get; set; }
        public Func<VirtualCameraRigState> GetRig { get; set; }
        public Action<VirtualCameraRigState> SetRig { get; set; }
        public Func<Lens, Lens> ValidateLens { get; set; }
        public Func<bool> ApplyDamping { get; set; }
        public Func<Settings> GetSettings { get; set; }

        public FrameTime CurrentFrameTime { get; private set; }
        public double CurrentTime => CurrentFrameTime.ToSeconds(k_KeyframeBufferFrameRate);
        public Lens CurrentLens => m_Lens;

        public void Validate()
        {
            m_BufferSize = Math.Max(MinBufferSize, m_BufferSize);
            m_InputKeyframes.Capacity = m_BufferSize;
            m_FocalLengthKeyframes.Capacity = m_BufferSize;
            m_FocusDistanceKeyframes.Capacity = m_BufferSize;
            m_ApertureKeyframes.Capacity = m_BufferSize;
        }

        public void MarkRigNeedsInitialize()
        {
            m_RigNeedsInitialize = true;
        }

        internal void Reset(FrameTime frameTime)
        {
            // Set a reasonable initial time for the processor. It doesn't have to be exact:
            // just needs to be close to the "present" time
            CurrentFrameTime = frameTime;

            Reset();
        }

        public void Reset()
        {
            m_Lens = GetLensTarget?.Invoke() ?? Lens.DefaultParams;
            m_InputKeyframes.Clear();
            m_FocalLengthKeyframes.Clear();
            m_ApertureKeyframes.Clear();
            m_FocusDistanceKeyframes.Clear();
            m_FocalLengthVelocity = 0;
            m_FocusDistanceVelocity = 0;
            m_ApertureVelocity = 0;
        }

        public void AddInputKeyframe(InputSample sample)
        {
            m_InputKeyframes.Add(sample, sample.Time);
        }

        public void AddFocusDistanceKeyframe(double time, float value)
        {
            m_FocusDistanceKeyframes.Add(value, time);
        }

        public void AddFocalLengthKeyframe(double time, float value)
        {
            m_FocalLengthKeyframes.Add(value, time);
        }

        public void AddApertureKeyframe(double time, float value)
        {
            m_ApertureKeyframes.Add(value, time);
        }

        public void AddLensKeyframe(double time, Lens lens)
        {
            AddApertureKeyframe(time, lens.Aperture);
            AddFocalLengthKeyframe(time, lens.FocalLength);
            AddFocusDistanceKeyframe(time, lens.FocusDistance);
        }

        public FrameRate GetBufferFrameRate()
        {
            return m_InputKeyframes.FrameRate;
        }

        public bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample)
        {
            return m_InputKeyframes.TryGetBufferRange(out oldestSample, out newestSample);
        }

        public TimedSampleStatus GetStatusAt(FrameTime frameTime)
        {
            return m_InputKeyframes.GetStatus(frameTime);
        }

        public IEnumerable<(double time, Pose? pose, Lens? lens)> ProcessTo(FrameTime frameTime)
        {
            var delta = (frameTime - CurrentFrameTime).FrameNumber;

            if (delta < k_OutOfOrderFrameTolerance || delta > TimeShiftTolerance)
            {
                // Device time shift detected. This can happen
                // 1) on the first update, or client was restarted
                // 2) when a timecode source was selected/changed on the device
                // 3) if there is a really long gap between pose samples, and the clocks have drifted
                Reset(frameTime);
            }

            var deltaTime = (float)k_KeyframeBufferFrameRate.FrameInterval;

            while (CurrentFrameTime <= frameTime)
            {
                var newLens = m_Lens;
                var isPoseValid = false;
                var isLensValid = false;
                var rig = GetRig?.Invoke() ?? VirtualCameraRigState.Identity;
                var settings = GetSettings?.Invoke() ?? Settings.DefaultData;
                var rigSettings = GetRigSettings(settings);
                var applyDamping = ApplyDamping?.Invoke() ?? false;

                if (m_InputKeyframes.GetLatest(CurrentFrameTime) is { } key)
                {
                    var pose = key.ARPose;

                    InitializeRigIfNeeded(ref rig, pose);

                    if (applyDamping)
                    {
                        pose = VirtualCameraDamping.Calculate(
                            rig.LastInput, pose, settings.Damping, deltaTime);
                    }

                    rig.Update(pose, rigSettings);

                    // Virtual Joysticks
                    rig.Translate(key.VirtualJoysticks, deltaTime, settings.JoystickSensitivity, settings.PedestalSpace, settings.MotionSpace, rigSettings);

                    // Gamepad
                    /// Convert look to degrees about Unity's ZXY rotation order and handedness.
                    var look = Vector3.Scale(key.GamepadLook, new Vector3(-1f, 1f, -1f));

                    rig.Rotate(look, deltaTime, Vector3.one, rigSettings);
                    rig.Translate(key.GamepadMove, deltaTime, Vector3.one, settings.PedestalSpace, settings.MotionSpace, rigSettings);

                    isPoseValid = true;
                }
                if (m_FocusDistanceKeyframes.GetLatest(CurrentFrameTime) is { } focusDistanceTarget)
                {
                    newLens.FocusDistance = Mathf.SmoothDamp(
                        newLens.FocusDistance,
                        focusDistanceTarget, ref m_FocusDistanceVelocity,
                        settings.FocusDistanceDamping, k_MaxSpeed, deltaTime);
                    isLensValid = true;
                }
                if (m_FocalLengthKeyframes.GetLatest(CurrentFrameTime) is { } focalLengthTarget)
                {
                    newLens.FocalLength = Mathf.SmoothDamp(
                        newLens.FocalLength,
                        focalLengthTarget, ref m_FocalLengthVelocity,
                        settings.FocalLengthDamping, k_MaxSpeed, deltaTime);
                    isLensValid = true;
                }
                if (m_ApertureKeyframes.GetLatest(CurrentFrameTime) is { } apertureTarget)
                {
                    newLens.Aperture = Mathf.SmoothDamp(
                        newLens.Aperture,
                        apertureTarget, ref m_ApertureVelocity,
                        settings.ApertureDamping, k_MaxSpeed, deltaTime);
                    isLensValid = true;
                }

                if (isPoseValid || isLensValid)
                {
                    Pose? pose = null;
                    Lens? lens = null;

                    if (isPoseValid)
                    {
                        SetRig?.Invoke(rig);
                        pose = rig.Pose;
                    }

                    if (isLensValid)
                    {
                        m_Lens = ValidateLens?.Invoke(newLens) ?? newLens;
                        lens = m_Lens;
                    }

                    yield return (CurrentTime, pose, lens);
                }

                CurrentFrameTime++;
            }
        }

        void InitializeRigIfNeeded(ref VirtualCameraRigState rig, in Pose pose)
        {
            if (m_RigNeedsInitialize)
            {
                rig.LastInput = pose;
                rig.RebaseOffset = Quaternion.Euler(0f, pose.rotation.eulerAngles.y - rig.ARPose.rotation.eulerAngles.y, 0f);
                m_RigNeedsInitialize = false;
            }
        }

        VirtualCameraRigSettings GetRigSettings(in Settings settings)
        {
            return new VirtualCameraRigSettings()
            {
                PositionLock = settings.PositionLock,
                RotationLock = settings.RotationLock,
                Rebasing = settings.Rebasing,
                MotionScale = settings.MotionScale,
                ErgonomicTilt = -settings.ErgonomicTilt,
                ZeroDutch = settings.AutoHorizon
            };
        }
    }
}
