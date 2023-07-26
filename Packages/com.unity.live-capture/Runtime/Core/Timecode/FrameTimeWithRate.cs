using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that represents a time in a frame sequence.
    /// </summary>
    /// <remarks>
    /// A time specified relative to a frame sequence cannot be converted to other representations or generally compared unless
    /// the frame rate of the sequence is also known. This struct stores both pieces of data needed to fully represent the time.
    /// </remarks>
    public readonly struct FrameTimeWithRate : IComparable, IComparable<FrameTimeWithRate>, IEquatable<FrameTimeWithRate>
    {
        readonly FrameRate m_Rate;
        readonly FrameTime m_Time;

        /// <summary>
        /// The rate of the frame sequence in Hz.
        /// </summary>
        public FrameRate Rate => m_Rate;

        /// <summary>
        /// The time relative to the frame sequence.
        /// </summary>
        public FrameTime Time => m_Time;

        /// <summary>
        /// Creates a new <see cref="FrameTimeWithRate"/> instance.
        /// </summary>
        /// <param name="rate">The rate of the frame sequence in Hz.</param>
        /// <param name="time">The time relative to the frame sequence.</param>
        public FrameTimeWithRate(FrameRate rate, FrameTime time)
        {
            m_Rate = rate;
            m_Time = time;
        }

        /// <summary>
        /// Creates a new <see cref="FrameTimeWithRate"/> instance from a time in seconds and a frame rate.
        /// </summary>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <param name="time">The time in seconds.</param>
        /// <returns>
        /// A new <see cref="FrameTimeWithRate"/> that represents the given time.
        /// </returns>
        public static FrameTimeWithRate FromSeconds(FrameRate frameRate, double time)
        {
            return new FrameTimeWithRate(frameRate, FrameTime.FromSeconds(frameRate, time));
        }

        /// <summary>
        /// Gets the time represented by this <see cref="FrameTimeWithRate"/> in seconds.
        /// </summary>
        /// <returns>
        /// The time in seconds since the start of the frame sequence, or <see langword="default"/>
        /// if <see cref="Rate"/> is invalid.
        /// </returns>
        public double ToSeconds()
        {
            return m_Time.ToSeconds(m_Rate);
        }

        /// <summary>
        /// Gets the time represented by this <see cref="FrameTimeWithRate"/> as a <see cref="Timecode"/>.
        /// </summary>
        /// <remarks>
        /// If the total time is greater than 24 hours, the time is wrapped around to zero.
        /// </remarks>
        /// <returns>
        /// A new <see cref="Timecode"/> that represents this time, or <see langword="default"/>
        /// if <see cref="Rate"/> is invalid.
        /// </returns>
        public Timecode ToTimecode()
        {
            return Timecode.FromFrameTime(m_Rate, m_Time);
        }

        /// <summary>
        /// Gets this time represented using a different <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="frameRate">The frame rate to convert to.</param>
        /// <returns>
        /// A new <see cref="FrameTimeWithRate"/> remapped to the the target frame rate, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public FrameTimeWithRate Remap(FrameRate frameRate)
        {
            return frameRate.IsValid ? new FrameTimeWithRate(frameRate, FrameTime.Remap(m_Time, m_Rate, frameRate)) : default;
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="FrameTimeWithRate"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// * Returns a negative value when this instance is less than <paramref name="other"/>.
        /// * Returns zero when this instance is the same as <paramref name="other"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(FrameTimeWithRate other)
        {
            return m_Time.CompareTo(FrameTime.Remap(other.m_Time, other.m_Rate, m_Rate));
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj"/>.
        /// * Returns a negative value when <paramref name="obj"/> is not a valid <see cref="FrameTimeWithRate"/> instance or this instance is less than <paramref name="obj"/>.
        /// * Returns zero when this instance is the same as <paramref name="obj"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="obj"/>.
        /// </returns>
        public int CompareTo(object obj)
        {
            return obj is FrameTimeWithRate other ? CompareTo(other) : -1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="FrameTimeWithRate"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public bool Equals(FrameTimeWithRate other)
        {
            return m_Rate == other.m_Rate && m_Time == other.m_Time;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="FrameTimeWithRate"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is FrameTimeWithRate other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Rate.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Time.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>A string that represents the current instance.</returns>
        public override string ToString()
        {
            return ToTimecode().ToString();
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="FrameTimeWithRate"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(FrameTimeWithRate a, FrameTimeWithRate b) => a.Equals(b);

        /// <summary>
        /// Determines whether two specified instances of <see cref="FrameTimeWithRate"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not represent the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(FrameTimeWithRate a, FrameTimeWithRate b) => !a.Equals(b);

        /// <summary>
        /// Determines whether one specified <see cref="FrameTimeWithRate"/> is later than or the same as another specified <see cref="FrameTimeWithRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(FrameTimeWithRate a, FrameTimeWithRate b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Determines whether one specified <see cref="FrameTimeWithRate"/> is earlier than or the same as another specified <see cref="FrameTimeWithRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(FrameTimeWithRate a, FrameTimeWithRate b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Determines whether one specified <see cref="FrameTimeWithRate"/> is later than another specified <see cref="FrameTimeWithRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(FrameTimeWithRate a, FrameTimeWithRate b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Determines whether one specified <see cref="FrameTimeWithRate"/> is earlier than another specified <see cref="FrameTimeWithRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(FrameTimeWithRate a, FrameTimeWithRate b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Increments a <see cref="FrameTimeWithRate"/> by a single frame.
        /// </summary>
        /// <param name="a">The frame time to increment.</param>
        /// <returns>The incremented frame time.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTimeWithRate"/> type.</exception>
        public static FrameTimeWithRate operator ++(FrameTimeWithRate a)
        {
            var time = a.m_Time;
            return new FrameTimeWithRate(a.m_Rate, ++time);
        }

        /// <summary>
        /// Calculates the sum of two specified <see cref="FrameTimeWithRate"/> values.
        /// </summary>
        /// <param name="a">The first frame time.</param>
        /// <param name="b">The second frame time.</param>
        /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTimeWithRate"/> type.</exception>
        public static FrameTimeWithRate operator +(FrameTimeWithRate a, FrameTimeWithRate b)
        {
            return new FrameTimeWithRate(a.m_Rate, a.m_Time + FrameTime.Remap(b.m_Time, b.m_Rate, a.m_Rate));
        }

        /// <summary>
        /// Decrements a <see cref="FrameTimeWithRate"/> by a single frame.
        /// </summary>
        /// <param name="a">The frame time to Decrement.</param>
        /// <returns>The decremented frame time.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTimeWithRate"/> type.</exception>
        public static FrameTimeWithRate operator --(FrameTimeWithRate a)
        {
            var time = a.m_Time;
            return new FrameTimeWithRate(a.m_Rate, --time);
        }

        /// <summary>
        /// Calculates the difference between two specified <see cref="FrameTimeWithRate"/> values.
        /// </summary>
        /// <param name="a">The frame time.</param>
        /// <param name="b">The frame time to subtract.</param>
        /// <returns>The difference of <paramref name="a"/> from <paramref name="b"/>.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTimeWithRate"/> type.</exception>
        public static FrameTimeWithRate operator -(FrameTimeWithRate a, FrameTimeWithRate b)
        {
            return new FrameTimeWithRate(a.m_Rate, a.m_Time - FrameTime.Remap(b.m_Time, b.m_Rate, a.m_Rate));
        }
    }
}
