using System;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// A class that records <see cref="FacePose"/> samples.
    /// </summary>
    [Serializable]
    class FaceDeviceRecorder
    {
        ICurve[] m_Curves =
        {
            new FaceBlendShapeCurves(),
            new Vector3Curve(),
            new EulerCurve(),
            new EulerCurve(),
            new EulerCurve(),
            new BooleanCurve(),
            new BooleanCurve(),
            new BooleanCurve(),
            new BooleanCurve(),
        };

        PropertyBinding[] m_Bindings =
        {
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.BlendShapes, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.HeadPosition, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.HeadOrientation, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.LeftEyeOrientation, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.RightEyeOrientation, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.BlendShapesEnabled, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.HeadPositionEnabled, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.HeadOrientationEnabled, typeof(FaceActor)),
            new PropertyBinding(string.Empty, FaceActor.PropertyNames.EyeOrientationEnabled, typeof(FaceActor)),
        };

        [SerializeField]
        [Tooltip("The relative tolerance, in percent, for reducing position keyframes")]
        float m_PositionError = 0.5f;

        [SerializeField]
        [Tooltip("The tolerance, in degrees, for reducing rotation keyframes")]
        float m_RotationError = 0.5f;

        [SerializeField]
        [Tooltip("The relative tolerance, in percent, for reducing blend shape keyframes")]
        float m_BlendShapeError = 0.5f;

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

        public float BlendShapeError
        {
            get => m_BlendShapeError;
            set => m_BlendShapeError = value;
        }

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

        /// <summary>
        /// The data channels to record as enum flags.
        /// </summary>
        public FaceChannelFlags Channels { get; set; }

        /// <summary>
        /// The frame rate to use for recording.
        /// </summary>
        public FrameRate FrameRate
        {
            get => m_Curves[0].FrameRate;
            set
            {
                foreach (var curve in m_Curves)
                {
                    curve.FrameRate = value;
                }
            }
        }

        /// <summary>
        /// Sets all the keyframe reduction parameters within their valid bounds.
        /// </summary>
        public void Validate()
        {
            m_PositionError = Mathf.Clamp(m_PositionError, 0f, 100f);
            m_RotationError = Mathf.Clamp(m_RotationError, 0f, 10f);
            m_BlendShapeError = Mathf.Clamp(m_BlendShapeError, 0f, 100f);
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
        public void Prepare()
        {
            InitialTime = null;
            Time = 0d;

            GetReduceable(0).MaxError = BlendShapeError / 100f;
            GetReduceable(1).MaxError = PositionError / 100f;
            GetReduceable(2).MaxError = RotationError;
            GetReduceable(3).MaxError = RotationError;
            GetReduceable(4).MaxError = RotationError;
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
        /// Records a <see cref="FacePose"/> sample.
        /// </summary>
        /// <param name="sample">The face pose sample to record</param>
        public void Record(ref FacePose sample)
        {
            if (Channels.HasFlag(FaceChannelFlags.BlendShapes))
            {
                (GetCurve<FaceBlendShapePose>(0) as FaceBlendShapeCurves).AddKey(ElapsedTime, sample.BlendShapes);
            }
            if (Channels.HasFlag(FaceChannelFlags.HeadPosition))
            {
                GetCurve<Vector3>(1).AddKey(ElapsedTime, sample.HeadPosition);
            }
            if (Channels.HasFlag(FaceChannelFlags.HeadRotation))
            {
                GetCurve<Quaternion>(2).AddKey(ElapsedTime, sample.HeadOrientation);
            }
            if (Channels.HasFlag(FaceChannelFlags.Eyes))
            {
                GetCurve<Quaternion>(3).AddKey(ElapsedTime, sample.LeftEyeOrientation);
                GetCurve<Quaternion>(4).AddKey(ElapsedTime, sample.RightEyeOrientation);
            }

            GetCurve<bool>(5).AddKey(ElapsedTime, Channels.HasFlag(FaceChannelFlags.BlendShapes));
            GetCurve<bool>(6).AddKey(ElapsedTime, Channels.HasFlag(FaceChannelFlags.HeadPosition));
            GetCurve<bool>(7).AddKey(ElapsedTime, Channels.HasFlag(FaceChannelFlags.HeadRotation));
            GetCurve<bool>(8).AddKey(ElapsedTime, Channels.HasFlag(FaceChannelFlags.Eyes));
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
