using System;
using System.Text;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that represents a timestamp used to label a frame.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A timecode represents a time in 24 hour period. This implementation supports negative timecodes.
    /// Since a timecode contains the number of the represented frame within the current second, each timecode is implicitly
    /// associated with the frame rate specified when creating the timecode and should only be compared to timecodes created with
    /// the same frame rate, or else the comparisons may not give sensible results.
    /// </para>
    /// <para>
    /// Timecodes are typically used to synchronize content generated in a live context. By matching data samples
    /// with a timecode, samples from various sources with differing latencies can be aligned properly when played
    /// back later.
    /// </para>
    /// <para>
    /// When using NTSC frame rates (23.976, 29.970, 59.94), it is not possible to accurately represent most timecodes in
    /// whole frames, as ~5% of a frame is missing each second. In order to correctly match wall-clock time to the timecode
    /// and prevent drift over time, drop frame timecode is often used, where the first few timecodes of every minute are
    /// skipped, except on every tenth minute.
    /// </para>
    /// </remarks>
    [Serializable]
    public struct Timecode : IComparable, IComparable<Timecode>, IEquatable<Timecode>
    {
        [ThreadStatic]
        static StringBuilder s_StringBuilder;

        [SerializeField]
        byte m_IsDropFrame;
        [SerializeField]
        sbyte m_Hours;
        [SerializeField]
        sbyte m_Minutes;
        [SerializeField]
        sbyte m_Seconds;
        [SerializeField]
        int m_Frames;
        [SerializeField]
        Subframe m_Subframe;

        /// <summary>
        /// Was this timecode generated taking into account drop frame calculations.
        /// </summary>
        public readonly bool IsDropFrame => m_IsDropFrame != 0;

        /// <summary>
        /// The number of elapsed hours.
        /// </summary>
        public readonly int Hours => m_Hours;

        /// <summary>
        /// The number of elapsed minutes in the current hour.
        /// </summary>
        public readonly int Minutes => m_Minutes;

        /// <summary>
        /// The number of elapsed seconds in the current minute.
        /// </summary>
        public readonly int Seconds => m_Seconds;

        /// <summary>
        /// The number of elapsed frames in the current second.
        /// </summary>
        public readonly int Frames => m_Frames;

        /// <summary>
        /// The time within the frame.
        /// </summary>
        public readonly Subframe Subframe => m_Subframe;

        /// <summary>
        /// Constructs a new <see cref="Timecode"/> from a given time.
        /// </summary>
        /// <remarks>
        /// If the total time is greater than 24 hours, the time is wrapped around to zero.
        /// </remarks>
        /// <param name="frameRate">The number of frames per second.</param>
        /// <param name="hours">The number of hours.</param>
        /// <param name="minutes">The number of minutes.</param>
        /// <param name="seconds">The number of seconds.</param>
        /// <param name="frames">The number of frames.</param>
        /// <param name="subframe">The time within the frame.</param>
        /// <param name="isDropFrame">
        /// Is the given time provided as a valid drop frame timecode, as opposed to a real (wall clock) timecode. This parameter is
        /// only relevant if <paramref name="frameRate"/> is drop frame.
        /// </param>
        /// <returns>
        /// A new <see cref="Timecode"/> that represents the given time, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public static Timecode FromHMSF(
            FrameRate frameRate,
            int hours,
            int minutes,
            int seconds,
            int frames,
            Subframe subframe = default,
            bool isDropFrame = true)
        {
            var framesPerSecond = (int)Math.Ceiling((double)frameRate);
            var framesPerMinute = framesPerSecond * 60;
            var framesPerHour = framesPerMinute * 60;

            var frameNumber =
                hours * framesPerHour +
                minutes * framesPerMinute +
                seconds * framesPerSecond +
                frames;

            return FromFrameTime(frameRate, new FrameTime(frameNumber, subframe), isDropFrame);
        }

        /// <summary>
        /// Constructs a new <see cref="Timecode"/> from a number of elapsed seconds.
        /// </summary>
        /// <remarks>
        /// If the total time is greater than 24 hours, the time is wrapped around to zero.
        /// </remarks>
        /// <param name="frameRate">The number of frames per second.</param>
        /// <param name="time">The number of elapsed seconds.</param>
        /// <returns>
        /// A new <see cref="Timecode"/> that represents the given time, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public static Timecode FromSeconds(FrameRate frameRate, double time)
        {
            return FromFrameTime(frameRate, FrameTime.FromSeconds(frameRate, time));
        }

        /// <summary>
        /// Constructs a new <see cref="Timecode"/> from a <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>
        /// If the total time is greater than 24 hours, the time is wrapped around to zero.
        /// </remarks>
        /// <param name="frameRate">The number of frames per second.</param>
        /// <param name="time">The time span.</param>
        /// <returns>
        /// A new <see cref="Timecode"/> that represents the given time, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public static Timecode FromTimeSpan(FrameRate frameRate, TimeSpan time)
        {
            return FromSeconds(frameRate, time.TotalSeconds);
        }

        /// <summary>
        /// Constructs a new <see cref="Timecode"/> from a frame number.
        /// </summary>
        /// <remarks>
        /// If the total time is greater than 24 hours, the time is wrapped around to zero.
        /// </remarks>
        /// <param name="frameRate">The number of frames per second.</param>
        /// <param name="frameTime">The frame number.</param>
        /// <returns>
        /// A new <see cref="Timecode"/> that represents the given time, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public static Timecode FromFrameTime(FrameRate frameRate, FrameTime frameTime)
        {
            return FromFrameTime(frameRate, frameTime, false);
        }

        static Timecode FromFrameTime(FrameRate frameRate, FrameTime frameTime, bool isDropFrame)
        {
            if (!frameRate.IsValid || frameRate.Numerator == 0)
            {
                return default;
            }

            var frameNumber = frameTime.FrameNumber;

            // If using a drop frame rate and the time is given in real time (wall clock time), we need to convert the
            // time into the drop frame time space.
            if (frameRate.IsDropFrame && !isDropFrame)
            {
                // skip the first few frames of each minute, except for each tenth minute
                var fps = (double)frameRate;
                var framesPerMinuteActual = (int)Math.Floor(fps * 60);
                var framesPerTenMinutesActual = (int)Math.Floor(fps * 60 * 10);
                var framesToDropPerMinute = (int)Math.Ceiling(fps / 15.0);

                var dropFrameNumber = Math.Abs(frameNumber);
                var minutesFramesWereNotDropped = dropFrameNumber / framesPerTenMinutesActual;
                var frameInTenMinutesActual = dropFrameNumber % framesPerTenMinutesActual;

                var totalFramesDropped = framesToDropPerMinute * 9 * minutesFramesWereNotDropped;
                dropFrameNumber += totalFramesDropped;

                if (frameInTenMinutesActual > framesToDropPerMinute)
                {
                    var minuteInTen = (frameInTenMinutesActual - framesToDropPerMinute) / framesPerMinuteActual;
                    dropFrameNumber += framesToDropPerMinute * minuteInTen;
                }

                frameNumber = Math.Sign(frameNumber) * dropFrameNumber;
            }

            var framesPerSecond = (int)Math.Ceiling((double)frameRate);
            var framesPerMinute = framesPerSecond * 60;
            var framesPerHour = framesPerMinute * 60;
            var framesPerDay = framesPerHour * 24;

            // rollover every 24 hours
            frameNumber %= framesPerDay;

            var hours = frameNumber / framesPerHour;
            var minutes = (frameNumber / framesPerMinute) % 60;
            var seconds = (frameNumber / framesPerSecond) % 60;
            var frames = frameNumber % framesPerSecond;

            return new Timecode
            {
                m_IsDropFrame = (byte)(frameRate.IsDropFrame ? 1 : 0),
                m_Hours = (sbyte)hours,
                m_Minutes = (sbyte)minutes,
                m_Seconds = (sbyte)seconds,
                m_Frames = frames,
                m_Subframe = frameTime.Subframe,
            };
        }

        /// <summary>
        /// Gets the timecode rounded down to the start of the current frame.
        /// </summary>
        /// <returns>A <see cref="Timecode"/> with no subframe component.</returns>
        public readonly Timecode Floor()
        {
            return new Timecode
            {
                m_IsDropFrame = m_IsDropFrame,
                m_Hours = m_Hours,
                m_Minutes = m_Minutes,
                m_Seconds = m_Seconds,
                m_Frames = m_Frames,
                m_Subframe = new Subframe(0, m_Subframe.Resolution),
            };
        }

        /// <summary>
        /// Gets the timecode at the center of the current frame.
        /// </summary>
        /// <returns>A <see cref="Timecode"/> at the middle of the frame interval.</returns>
        public readonly Timecode Center()
        {
            return new Timecode
            {
                m_IsDropFrame = m_IsDropFrame,
                m_Hours = m_Hours,
                m_Minutes = m_Minutes,
                m_Seconds = m_Seconds,
                m_Frames = m_Frames,
                m_Subframe = Subframe.FromFloat(0.5f, m_Subframe.Resolution),
            };
        }

        /// <summary>
        /// Gets the time in seconds represented by this <see cref="Timecode"/> for a given <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <returns>
        /// The time in seconds represented by this timecode, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public readonly double ToSeconds(FrameRate frameRate)
        {
            return ToFrameTime(frameRate).ToSeconds(frameRate);
        }

        /// <summary>
        /// Gets the <see cref="FrameTime"/> represented by this <see cref="Timecode"/> for a given <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <returns>
        /// The frame time represented by this timecode, or <see langword="default"/> if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public readonly FrameTime ToFrameTime(FrameRate frameRate)
        {
            if (!frameRate.IsValid)
            {
                return default;
            }

            var fps = (double)frameRate;
            var framesPerSecond = (int)Math.Ceiling(fps);
            var framesPerMinute = framesPerSecond * 60;
            var framesPerHour = framesPerMinute * 60;

            var frameNumber =
                Hours * framesPerHour +
                Minutes * framesPerMinute +
                Seconds * framesPerSecond +
                Frames;

            if (frameRate.IsDropFrame)
            {
                var framesToDropPerMinute = (int)Math.Ceiling(fps / 15.0);
                var totalMinutes = (Hours * 60) + Minutes;
                var minutesFramesWereDropped = totalMinutes - (totalMinutes / 10);

                frameNumber -= framesToDropPerMinute * minutesFramesWereDropped;
            }

            return new FrameTime(frameNumber, m_Subframe);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Timecode"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// * Returns a negative value when this instance is less than <paramref name="other"/>.
        /// * Returns zero when this instance is the same as <paramref name="other"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="other"/>.
        /// </returns>
        public readonly int CompareTo(Timecode other)
        {
            if (m_Hours != other.m_Hours)
            {
                return m_Hours.CompareTo(other.m_Hours);
            }
            if (m_Minutes != other.m_Minutes)
            {
                return m_Minutes.CompareTo(other.m_Minutes);
            }
            if (m_Seconds != other.m_Seconds)
            {
                return m_Seconds.CompareTo(other.m_Seconds);
            }
            if (m_Frames != other.m_Frames)
            {
                return m_Frames.CompareTo(other.m_Frames);
            }
            return m_Subframe.CompareTo(other.m_Subframe);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj"/>.
        /// * Returns a negative value when <paramref name="obj"/> is not a valid <see cref="Timecode"/> instance or this instance is less than <paramref name="obj"/>.
        /// * Returns zero when this instance is the same as <paramref name="obj"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="obj"/>.
        /// </returns>
        public readonly int CompareTo(object obj)
        {
            return obj is Timecode other ? CompareTo(other) : -1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Timecode"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(Timecode other)
        {
            return
                m_Hours == other.m_Hours &&
                m_Minutes == other.m_Minutes &&
                m_Seconds == other.m_Seconds &&
                m_Frames == other.m_Frames &&
                m_Subframe == other.m_Subframe;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="Timecode"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj)
        {
            return obj is Timecode other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Hours.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Minutes.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Seconds.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Frames.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Subframe.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>A string that represents the current instance.</returns>
        public override readonly string ToString()
        {
            if (s_StringBuilder == null)
            {
                s_StringBuilder = new StringBuilder();
            }
            else
            {
                s_StringBuilder.Clear();
            }

            if (Hours < 0 || Minutes < 0 || Seconds < 0 || Frames < 0)
            {
                s_StringBuilder.Append('-');
            }

            s_StringBuilder.AppendFormat("{0:D2}", Mathf.Abs(m_Hours));
            s_StringBuilder.Append(':');
            s_StringBuilder.AppendFormat("{0:D2}", Mathf.Abs(m_Minutes));
            s_StringBuilder.Append(':');
            s_StringBuilder.AppendFormat("{0:D2}", Mathf.Abs(m_Seconds));
            s_StringBuilder.Append(IsDropFrame ? ';' : ':');
            s_StringBuilder.AppendFormat("{0:D2}", Mathf.Abs(m_Frames));

            return s_StringBuilder.ToString();
        }

        /// <summary>
        /// Indicates whether two specified instances of <see cref="Timecode"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Timecode a, Timecode b) => a.Equals(b);

        /// <summary>
        /// Indicates whether two specified instances of <see cref="Timecode"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Timecode a, Timecode b) => !a.Equals(b);

        /// <summary>
        /// Indicates whether one specified <see cref="Timecode"/> is later than or the same as another specified <see cref="Timecode"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(Timecode a, Timecode b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="Timecode"/> is earlier than or the same as another specified <see cref="Timecode"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(Timecode a, Timecode b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="Timecode"/> is later than another specified <see cref="Timecode"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(Timecode a, Timecode b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Indicates whether one specified <see cref="Timecode"/> is earlier than another specified <see cref="Timecode"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(Timecode a, Timecode b) => a.CompareTo(b) < 0;
    }
}
