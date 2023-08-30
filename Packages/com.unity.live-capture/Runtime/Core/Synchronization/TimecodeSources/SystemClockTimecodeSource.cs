using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A component that generates timecodes using the system clock.
    /// </summary>
    [ExecuteAlways]
    [CreateTimecodeSourceMenuItemAttribute("System Clock Timecode Source")]
    [AddComponentMenu("Live Capture/Timecode/System Clock Timecode Source")]
    [HelpURL(Documentation.baseURL + "ref-component-system-clock-timecode-source" + Documentation.endURL)]
    public class SystemClockTimecodeSource : TimecodeSource
    {
        /// <summary>
        /// An enum defining the source clocks that may be used to provide the time.
        /// </summary>
        public enum ClockType
        {
            /// <summary>
            /// The timecode is derived from the engine time.
            /// </summary>
            GameTime,
            /// <summary>
            /// The timecode is derived from OS system time.
            /// </summary>
            SystemClock,
        }

        [SerializeField, EnumButtonGroup(35), Tooltip(
            "The source clocks that may be used to provide the time.\n\n" +
            "GameTime - The timecode is derived from the engine time.\n" +
            "SystemClock - The timecode is derived from OS system time.")]
        ClockType m_ClockType = ClockType.SystemClock;

        [SerializeField, OnlyStandardFrameRates, Tooltip("The frame rate of the generated timecodes.")]
        FrameRate m_FrameRate = StandardFrameRate.FPS_24_00;

        /// <summary>
        /// The source clock used to generate timecodes.
        /// </summary>
        public ClockType Type
        {
            get => m_ClockType;
            set => m_ClockType = value;
        }

        /// <inheritdoc/>
        public override string FriendlyName => $"System Clock ({name})";

        /// <inheritdoc />
        public override FrameRate FrameRate => m_FrameRate;

        /// <inheritdoc />
        protected override bool TryPollTimecode(out FrameRate frameRate, out Timecode timecode)
        {
            frameRate = m_FrameRate;

            switch (Type)
            {
                case ClockType.GameTime:
                    timecode = Timecode.FromSeconds(m_FrameRate, Time.timeAsDouble);
                    return true;
                case ClockType.SystemClock:
                    timecode = Timecode.FromSeconds(m_FrameRate, DateTime.Now.TimeOfDay.TotalSeconds);
                    return true;
                default:
                    timecode = default;
                    return false;
            }
        }
    }
}
