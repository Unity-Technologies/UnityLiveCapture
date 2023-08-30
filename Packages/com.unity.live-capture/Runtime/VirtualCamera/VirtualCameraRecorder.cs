using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [Serializable]
    class VirtualCameraRecorder
    {
        ICurve[] m_Curves =
        {
            new Vector3Curve(),
            new EulerCurve(),
            new FloatCurve(),
            new FloatCurve(),
            new FloatCurve(),
            new BooleanCurve(),
            new Vector2Curve(),
            new FloatCurve(),
            new Vector2Curve(),
            new Vector2Curve(),
            new IntegerCurve(),
            new Vector2Curve(),
            new FloatCurve(),
            new FloatCurve(),
            new FloatCurve(),
            new BooleanCurve(),
            new BooleanCurve(),
        };

        PropertyBinding[] m_Bindings =
        {
            new PropertyBinding(string.Empty, "m_LocalPosition", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LocalEulerAngles", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_Lens.m_FocalLength", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_Lens.m_FocusDistance", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_Lens.m_Aperture", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_DepthOfField", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_FocalLengthRange", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_CloseFocusDistance", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_ApertureRange", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_LensShift", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_BladeCount", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_Curvature", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_BarrelClipping", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LensIntrinsics.m_Anamorphism", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_CropAspect", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LocalPositionEnabled", typeof(VirtualCameraActor)),
            new PropertyBinding(string.Empty, "m_LocalEulerAnglesEnabled", typeof(VirtualCameraActor)),
        };

        [SerializeField]
        [Tooltip("The relative tolerance, in percent, for reducing position keyframes")]
        float m_PositionError = 0.5f;

        [SerializeField]
        [Tooltip("The tolerance, in degrees, for reducing rotation keyframes")]
        float m_RotationError = 0.5f;

        [SerializeField]
        [Tooltip("The relative tolerance, in percent, for reducing lens keyframes")]
        float m_LensError = 0.5f;

        /// <summary>
        /// The time when the recording started in seconds.
        /// </summary>
        public double? InitialTime { get; private set; }

        /// <summary>
        /// The time used by the recorder in seconds.
        /// </summary>
        public double Time { get; private set; }

        /// <summary>
        /// The elapsed time since the recording started in seconds.
        /// </summary>
        public double ElapsedTime => InitialTime.HasValue ? Time - InitialTime.Value : 0d;

        public Action OnReset { get; set; }

        public float PositionError
        {
            get => m_PositionError;
            set => m_PositionError = value;
        }

        public float RotationError
        {
            get => m_RotationError;
            set => m_RotationError = value;
        }

        public float LensError
        {
            get => m_LensError;
            set => m_LensError = value;
        }

        /// <summary>
        /// The data channels to record as enum flags.
        /// </summary>
        public VirtualCameraChannelFlags Channels { get; private set; }

        /// <summary>
        /// Sets all the keyframe reduction parameters within their valid bounds.
        /// </summary>
        public void Validate()
        {
            m_PositionError = Mathf.Clamp(m_PositionError, 0f, 100f);
            m_RotationError = Mathf.Clamp(m_RotationError, 0f, 10f);
            m_LensError = Mathf.Clamp(m_LensError, 0f, 100f);
        }

        /// <summary>
        /// Checks if the recording contains no recorded samples.
        /// </summary>
        /// <returns>
        /// true if the recording contains no samples; otherwise, false.
        /// </returns>
        public bool IsEmpty()
        {
            foreach (var curve in m_Curves)
            {
                if (!curve.IsEmpty())
                {
                    return false;
                }
            }

            return true;
        }

        public void Update(double time)
        {
            Time = time;

            if (ElapsedTime < 0d)
            {
                InitialTime = null;
            }

            if (!InitialTime.HasValue)
            {
                InitialTime = time;

                Reset();
            }
        }

        /// <summary>
        /// Initializes the recorder parameters for a new recording session.
        /// </summary>
        public void Prepare(VirtualCameraChannelFlags channels, FrameRate frameRate)
        {
            InitialTime = null;
            Time = 0d;
            Channels = channels;

            foreach (var curve in m_Curves)
            {
                curve.FrameRate = frameRate;
            }

            GetReduceable(0).MaxError = PositionError / 100f;
            GetReduceable(1).MaxError = RotationError;
            GetReduceable(2).MaxError = LensError / 100f;
            GetReduceable(3).MaxError = LensError / 100f;
            GetReduceable(4).MaxError = LensError / 100f;
            Reset();
        }

        void Reset()
        {
            foreach (var curve in m_Curves)
            {
                curve.Clear();
            }

            OnReset?.Invoke();
        }

        /// <summary>
        /// Records a position sample.
        /// </summary>
        /// <param name="sample">The position sample to record</param>
        public void RecordPosition(Vector3 sample)
        {
            if (Channels.HasFlag(VirtualCameraChannelFlags.Position))
            {
                GetCurve<Vector3>(0).AddKey(ElapsedTime, sample);
                RecordLocalPositionEnabled(true);
            }
        }

        /// <summary>
        /// Records a rotation sample.
        /// </summary>
        /// <param name="sample">The rotation sample to record</param>
        public void RecordRotation(Quaternion sample)
        {
            if (Channels.HasFlag(VirtualCameraChannelFlags.Rotation))
            {
                GetCurve<Quaternion>(1).AddKey(ElapsedTime, sample);
                RecordLocalEulerAnglesEnabled(true);
            }
        }

        /// <summary>
        /// Records a focal length sample.
        /// </summary>
        /// <param name="sample">The focal length sample to record</param>
        public void RecordFocalLength(float sample)
        {
            if (Channels.HasFlag(VirtualCameraChannelFlags.FocalLength))
            {
                GetCurve<float>(2).AddKey(ElapsedTime, sample);
            }
        }

        /// <summary>
        /// Records a focus distance sample.
        /// </summary>
        /// <param name="sample">The focus distance sample to record</param>
        public void RecordFocusDistance(float sample)
        {
            if (Channels.HasFlag(VirtualCameraChannelFlags.FocusDistance))
            {
                GetCurve<float>(3).AddKey(ElapsedTime, sample);
            }
        }

        /// <summary>
        /// Records an aperture sample.
        /// </summary>
        /// <param name="sample">The aperture sample to record</param>
        public void RecordAperture(float sample)
        {
            if (Channels.HasFlag(VirtualCameraChannelFlags.Aperture))
            {
                GetCurve<float>(4).AddKey(ElapsedTime, sample);
            }
        }

        /// <summary>
        /// Records the enabled state of the depth of field.
        /// </summary>
        /// <param name="sample">The enabled state to record</param>
        public void RecordEnableDepthOfField(bool sample)
        {
            if (Channels.HasFlag(VirtualCameraChannelFlags.FocusDistance))
            {
                GetCurve<bool>(5).AddKey(ElapsedTime, sample);
            }
        }

        /// <summary>
        /// Records the aspect of the crop mask.
        /// </summary>
        /// <param name="sample">The crop aspect to record</param>
        public void RecordCropAspect(float sample)
        {
            GetCurve<float>(14).AddKey(ElapsedTime, sample);
        }

        /// <summary>
        /// Records the local position enabled state.
        /// </summary>
        /// <param name="sample">The state to record</param>
        void RecordLocalPositionEnabled(bool sample)
        {
            GetCurve<bool>(15).AddKey(ElapsedTime, sample);
        }

        /// <summary>
        /// Records the local euler angles enabled state.
        /// </summary>
        /// <param name="sample">The state to record</param>
        void RecordLocalEulerAnglesEnabled(bool sample)
        {
            GetCurve<bool>(16).AddKey(ElapsedTime, sample);
        }

        /// <summary>
        /// Records the <see cref="LensIntrinsics"/> of the camera.
        /// </summary>
        /// <param name="intrinsics">The lens intrinsic parameters to record</param>
        public void RecordLensIntrinsics(LensIntrinsics intrinsics)
        {
            GetCurve<Vector2>(6).AddKey(ElapsedTime, intrinsics.FocalLengthRange);
            GetCurve<float>(7).AddKey(ElapsedTime, intrinsics.CloseFocusDistance);
            GetCurve<Vector2>(8).AddKey(ElapsedTime, intrinsics.ApertureRange);
            GetCurve<Vector2>(9).AddKey(ElapsedTime, intrinsics.LensShift);
            GetCurve<int>(10).AddKey(ElapsedTime, intrinsics.BladeCount);
            GetCurve<Vector2>(11).AddKey(ElapsedTime, intrinsics.Curvature);
            GetCurve<float>(12).AddKey(ElapsedTime, intrinsics.BarrelClipping);
            GetCurve<float>(13).AddKey(ElapsedTime, intrinsics.Anamorphism);
        }

        /// <summary>
        /// Produces an AnimationClip from the recording.
        /// </summary>
        /// <returns>
        /// An AnimationClip created from the recording.
        /// </returns>
        public AnimationClip Bake()
        {
            var animationClip = new AnimationClip();

            for (var i = 0; i < m_Curves.Length; ++i)
            {
                m_Curves[i].SetToAnimationClip(m_Bindings[i], animationClip);
            }

            return animationClip;
        }

        ICurve<T> GetCurve<T>(int index) where T : struct
        {
            return m_Curves[index] as ICurve<T>;
        }

        IReduceableCurve GetReduceable(int index)
        {
            return m_Curves[index] as IReduceableCurve;
        }
    }
}
