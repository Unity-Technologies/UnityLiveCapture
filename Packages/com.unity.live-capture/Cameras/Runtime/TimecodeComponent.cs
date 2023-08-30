using UnityEngine;

namespace Unity.LiveCapture.Cameras
{
    /// <summary>
    /// A component that stores animatable bindings of a <see cref="Timecode"/> and a <see cref="FrameRate"/>.
    /// </summary>
    /// <remarks>
    /// Use this component to visualize and animate a <see cref="Timecode"/> and a <see cref="FrameRate"/> in the editor.
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [HelpURL(Documentation.baseURL + "ref-component-timecode-component" + Documentation.endURL)]
    public sealed class TimecodeComponent : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        int m_Hours;

        [SerializeField, HideInInspector]
        int m_Minutes;

        [SerializeField, HideInInspector]
        int m_Seconds;

        [SerializeField, HideInInspector]
        int m_Frames;

        [SerializeField, HideInInspector]
        int m_Subframe;

        [SerializeField, HideInInspector]
        int m_Resolution;

        [SerializeField, HideInInspector]
        bool m_IsDropFrame;

        [SerializeField, HideInInspector]
        int m_RateNumerator;

        [SerializeField, HideInInspector]
        int m_RateDenominator;

        [SerializeField, HideInInspector]
        bool m_RateIsDropFrame;

        /// <summary>
        /// The hours of the timecode.
        /// </summary>
        public int Hours
        {
            get => m_Hours;
            set => m_Hours = value;
        }

        /// <summary>
        /// The minutes of the timecode.
        /// </summary>
        public int Minutes
        {
            get => m_Minutes;
            set => m_Minutes = value;
        }

        /// <summary>
        /// The seconds of the timecode.
        /// </summary>
        public int Seconds
        {
            get => m_Seconds;
            set => m_Seconds = value;
        }

        /// <summary>
        /// The frames of the timecode.
        /// </summary>
        public int Frames
        {
            get => m_Frames;
            set => m_Frames = value;
        }

        /// <summary>
        /// The subframe of the timecode.
        /// </summary>
        public int Subframe
        {
            get => m_Subframe;
            set => m_Subframe = value;
        }

        /// <summary>
        /// The resolution of the timecode.
        /// </summary>
        public int Resolution
        {
            get => m_Resolution;
            set => m_Resolution = value;
        }

        /// <summary>
        /// Whether the timecode is drop frame.
        /// </summary>
        public bool IsDropFrame
        {
            get => m_IsDropFrame;
            set => m_IsDropFrame = value;
        }

        /// <summary>
        /// The numerator of the frame rate.
        /// </summary>
        public int RateNumerator
        {
            get => m_RateNumerator;
            set => m_RateNumerator = value;
        }

        /// <summary>
        /// The denominator of the frame rate.
        /// </summary>
        public int RateDenominator
        {
            get => m_RateDenominator;
            set => m_RateDenominator = value;
        }

        /// <summary>
        /// Whether the frame rate is drop frame.
        /// </summary>
        public bool RateIsDropFrame
        {
            get => m_RateIsDropFrame;
            set => m_RateIsDropFrame = value;
        }


#if UNITY_EDITOR
        [SerializeField]
        Timecode m_Timecode;

        [SerializeField]
        FrameRate m_FrameRate;
#endif

        /// <summary>
        /// The timecode.
        /// </summary>
        public Timecode Timecode
        {
            get => Timecode.FromHMSF(
                FrameRate,
                m_Hours,
                m_Minutes,
                m_Seconds,
                m_Frames,
                new Subframe(m_Subframe, m_Resolution),
                m_IsDropFrame);
            set
            {
                m_Hours = value.Hours;
                m_Minutes = value.Minutes;
                m_Seconds = value.Seconds;
                m_Frames = value.Frames;
                m_IsDropFrame = value.IsDropFrame;
#if UNITY_EDITOR
                m_Timecode = value;
#endif
            }
        }

        /// <summary>
        /// The frame rate of the timecode.
        /// </summary>
        public FrameRate FrameRate
        {
            get => new FrameRate(m_RateNumerator, m_RateDenominator, m_RateIsDropFrame);
            set
            {
                m_RateNumerator = value.Numerator;
                m_RateDenominator = value.Denominator;
                m_RateIsDropFrame = value.IsDropFrame;
#if UNITY_EDITOR
                m_FrameRate = value;
#endif
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Timecode = m_Timecode;
            FrameRate = m_FrameRate;
        }

        void LateUpdate()
        {
            m_Timecode = Timecode;
            m_FrameRate = FrameRate;
        }
#endif
    }
}
