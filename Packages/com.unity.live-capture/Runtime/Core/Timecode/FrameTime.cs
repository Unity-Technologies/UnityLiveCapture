using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that represents a time relative to a frame sequence.
    /// </summary>
    [Serializable]
    public struct FrameTime : IComparable, IComparable<FrameTime>, IEquatable<FrameTime>
    {
        [SerializeField]
        int m_FrameNumber;
        [SerializeField]
        Subframe m_Subframe;

        /// <summary>
        /// The number of the frame in the sequence.
        /// </summary>
        public readonly int FrameNumber => m_FrameNumber;

        /// <summary>
        /// The time within the frame.
        /// </summary>
        public readonly Subframe Subframe => m_Subframe;

        /// <summary>
        /// Creates a new <see cref="FrameTime"/> instance.
        /// </summary>
        /// <param name="frameNumber">The number of the frame in the sequence.</param>
        /// <param name="subframe">The subframe value used to indicate a time somewhere within the duration of the frame.</param>
        public FrameTime(int frameNumber, Subframe subframe = default)
        {
            m_FrameNumber = frameNumber;
            m_Subframe = subframe;
        }

        /// <summary>
        /// Creates a new <see cref="FrameTime"/> instance from a frame time.
        /// </summary>
        /// <param name="frameTime">The frame time with the frame number in the integer part and the subframe in the decimal part.</param>
        /// <param name="subframeResolution">The number of possible subframe values in the frame. If this value is not greater than zero,
        /// the subframe value is treated as zero.</param>
        /// <returns>A new <see cref="FrameTime"/> that represents the given frame time.</returns>
        public static FrameTime FromFrameTime(double frameTime, int subframeResolution = Subframe.DefaultResolution)
        {
            var maxTime = int.MaxValue + (double)new Subframe(Subframe.MaxResolution, Subframe.MaxResolution);
            frameTime = Math.Max(Math.Min(frameTime, maxTime), int.MinValue);

            return FromFrameTimeChecked(frameTime, subframeResolution);
        }

        static FrameTime FromFrameTimeChecked(double frameTime, int subframeResolution)
        {
            var frameNumber = Math.Floor(frameTime);
            var subframe = Subframe.FromDouble(frameTime - frameNumber, subframeResolution);

            checked
            {
                return new FrameTime((int)frameNumber, subframe);
            }
        }

        /// <summary>
        /// Creates a new <see cref="FrameTime"/> instance from a time in seconds and a frame rate.
        /// </summary>
        /// <remarks>
        /// The input time is clamped to the range [-<see cref="MaxRepresentableSeconds"/>, <see cref="MaxRepresentableSeconds"/>] defined
        /// for the provided <see cref="FrameRate"/>.
        /// </remarks>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <param name="time">The time in seconds.</param>
        /// <param name="subframeResolution">The number of possible subframe values in the frame. If this value is not greater than zero,
        /// the subframe value is treated as zero.</param>
        /// <returns>
        /// A new <see cref="FrameTime"/> that represents the given time, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public static FrameTime FromSeconds(FrameRate frameRate, double time, int subframeResolution = Subframe.DefaultResolution)
        {
            return frameRate.IsValid ? FromFrameTime(time * frameRate.AsDouble(), subframeResolution) : default;
        }

        /// <summary>
        /// Gets the frame time rounded down to the start of the current frame.
        /// </summary>
        /// <returns>A <see cref="FrameTime"/> with no subframe component.</returns>
        public readonly FrameTime Floor()
        {
            var subframe = new Subframe(0, m_Subframe.Resolution);
            return new FrameTime(m_FrameNumber, subframe);
        }

        /// <summary>
        /// Gets the frame time rounded up to the start of the next frame.
        /// </summary>
        /// <returns>A <see cref="FrameTime"/> with no subframe component.</returns>
        public readonly FrameTime Ceil()
        {
            var subframe = new Subframe(0, m_Subframe.Resolution);

            if (m_Subframe.Value == 0)
            {
                return new FrameTime(m_FrameNumber, subframe);
            }
            if (m_FrameNumber >= int.MaxValue)
            {
                return new FrameTime(int.MaxValue, subframe);
            }

            return new FrameTime(m_FrameNumber + 1, subframe);
        }

        /// <summary>
        /// Gets the frame time rounded to the nearest start of a frame.
        /// </summary>
        /// <remarks>
        /// Subframe values exactly half way in a frame are rounded towards negative infinity.
        /// </remarks>
        /// <returns>A <see cref="FrameTime"/> with no subframe component.</returns>
        public readonly FrameTime Round()
        {
            var subframe = new Subframe(0, m_Subframe.Resolution);

            if (m_Subframe.Value <= (m_Subframe.Resolution / 2))
            {
                return new FrameTime(m_FrameNumber, subframe);
            }
            if (m_FrameNumber >= int.MaxValue)
            {
                return new FrameTime(int.MaxValue, subframe);
            }

            return new FrameTime(m_FrameNumber + 1, subframe);
        }

        /// <summary>
        /// Gets the time in seconds represented by this <see cref="FrameTime"/> for a given <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <returns>
        /// The time in seconds since the start of the frame sequence, or <see langword="default"/>
        /// if <paramref name="frameRate"/> is invalid.
        /// </returns>
        public readonly double ToSeconds(FrameRate frameRate)
        {
            return frameRate.IsValid && frameRate.Numerator != 0 ? (double)this * frameRate.FrameInterval : default;
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="FrameTime"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// * Returns a negative value when this instance is less than <paramref name="other"/>.
        /// * Returns zero when this instance is the same as <paramref name="other"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="other"/>.
        /// </returns>
        public readonly int CompareTo(FrameTime other)
        {
            if (m_FrameNumber != other.m_FrameNumber)
            {
                return m_FrameNumber.CompareTo(other.m_FrameNumber);
            }
            return m_Subframe.CompareTo(other.m_Subframe);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj"/>.
        /// * Returns a negative value when <paramref name="obj"/> is not a valid <see cref="FrameTime"/> instance or this instance is less than <paramref name="obj"/>.
        /// * Returns zero when this instance is the same as <paramref name="obj"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="obj"/>.
        /// </returns>
        public readonly int CompareTo(object obj)
        {
            return obj is FrameTime other ? CompareTo(other) : -1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(FrameTime other)
        {
            return m_FrameNumber == other.m_FrameNumber && m_Subframe == other.m_Subframe;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="FrameTime"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj)
        {
            return obj is FrameTime other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_FrameNumber;
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
            return ((double)this).ToString();
        }

        /// <summary>
        /// Calculates the maximum length of a frame sequence in seconds that can be represented by a <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="frameRate">The frame rate of the frame sequence.</param>
        /// <returns>The maximum time in seconds, or <see langword="default"/> if <paramref name="frameRate"/> is invalid.</returns>
        public static double MaxRepresentableSeconds(FrameRate frameRate)
        {
            if (!frameRate.IsValid)
            {
                return default;
            }

            var maxSubframe = new Subframe(Subframe.MaxResolution, Subframe.MaxResolution);
            return new FrameTime(int.MaxValue, maxSubframe).ToSeconds(frameRate);
        }

        /// <summary>
        /// Converts a <see cref="FrameTime"/> in a frame sequence with a given <see cref="FrameRate"/> to a time
        /// in a frame sequence with a different <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="frameTime">The frame time to remap.</param>
        /// <param name="srcRate">The original frame rate.</param>
        /// <param name="dstRate">The frame rate to convert to.</param>
        /// <returns>
        /// A new <see cref="FrameTime"/> remapped to the the target frame rate, or <see langword="default"/>
        /// if <paramref name="srcRate"/> or <paramref name="dstRate"/> is invalid.
        /// </returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTime"/> type.</exception>
        public static FrameTime Remap(FrameTime frameTime, FrameRate srcRate, FrameRate dstRate)
        {
            if (!srcRate.IsValid || srcRate.Numerator == 0 || !dstRate.IsValid)
            {
                return default;
            }
            if (srcRate == dstRate)
            {
                return frameTime;
            }

            var numerator = (long)dstRate.Numerator * srcRate.Denominator;
            var denominator = (long)dstRate.Denominator * srcRate.Numerator;
            var conversionRate = (double)numerator / denominator;

            return FromFrameTimeChecked((double)frameTime * conversionRate, frameTime.m_Subframe.Resolution);
        }

        /// <summary>
        /// Indicates whether two specified instances of <see cref="FrameTime"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(FrameTime a, FrameTime b) => a.Equals(b);

        /// <summary>
        /// Indicates whether two specified instances of <see cref="FrameTime"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(FrameTime a, FrameTime b) => !a.Equals(b);

        /// <summary>
        /// Indicates whether one specified <see cref="FrameTime"/> is later than or the same as another specified <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(FrameTime a, FrameTime b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="FrameTime"/> is earlier than or the same as another specified <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(FrameTime a, FrameTime b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="FrameTime"/> is later than another specified <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(FrameTime a, FrameTime b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Indicates whether one specified <see cref="FrameTime"/> is earlier than another specified <see cref="FrameTime"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(FrameTime a, FrameTime b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Increments a <see cref="FrameTime"/> by a single frame.
        /// </summary>
        /// <param name="a">The frame time to increment.</param>
        /// <returns>The incremented frame time.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTime"/> type.</exception>
        public static FrameTime operator ++(FrameTime a)
        {
            var frameNumber = a.m_FrameNumber;

            checked
            {
                frameNumber++;
            }

            return new FrameTime(frameNumber, a.m_Subframe);
        }

        /// <summary>
        /// Calculates the sum of two specified <see cref="FrameTime"/> values.
        /// </summary>
        /// <param name="a">The first frame time.</param>
        /// <param name="b">The second frame time.</param>
        /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTime"/> type.</exception>
        public static FrameTime operator +(FrameTime a, FrameTime b)
        {
            // If one of the subframe resolutions is a multiple of the other other, we can produce an exact result.
            // Otherwise, we compute the best approximate result with the larger resolution.
            var aRes = a.m_Subframe.Resolution;
            var bRes = b.m_Subframe.Resolution;

            if (aRes % bRes == 0)
            {
                var totalSubframes = a.m_Subframe.Value + (b.m_Subframe.Value * (aRes / bRes));
                var subframes = totalSubframes % aRes;
                var extraFrames = totalSubframes / aRes;
                int frameNumber;

                checked
                {
                    frameNumber = a.m_FrameNumber + b.m_FrameNumber + extraFrames;
                }

                return new FrameTime(frameNumber, new Subframe(subframes, aRes));
            }
            if (bRes % aRes == 0)
            {
                var totalSubframes = (a.m_Subframe.Value * (bRes / aRes)) + b.m_Subframe.Value;
                var subframes = totalSubframes % bRes;
                var extraFrames = totalSubframes / bRes;
                int frameNumber;

                checked
                {
                    frameNumber = a.m_FrameNumber + b.m_FrameNumber + extraFrames;
                }

                return new FrameTime(frameNumber, new Subframe(subframes, bRes));
            }

            return FromFrameTimeChecked((double)a + (double)b, Mathf.Max(aRes, bRes));
        }

        /// <summary>
        /// Decrements a <see cref="FrameTime"/> by a single frame.
        /// </summary>
        /// <param name="a">The frame time to Decrement.</param>
        /// <returns>The decremented frame time.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTime"/> type.</exception>
        public static FrameTime operator --(FrameTime a)
        {
            var frameNumber = a.m_FrameNumber;

            checked
            {
                frameNumber--;
            }

            return new FrameTime(frameNumber, a.m_Subframe);
        }

        /// <summary>
        /// Calculates the difference between two specified <see cref="FrameTime"/> values.
        /// </summary>
        /// <param name="a">The frame time.</param>
        /// <param name="b">The frame time to subtract.</param>
        /// <returns>The subtraction of <paramref name="b"/> from <paramref name="a"/>.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting value is outside the range representable
        /// by the <see cref="FrameTime"/> type.</exception>
        public static FrameTime operator -(FrameTime a, FrameTime b)
        {
            // If one of the subframe resolutions is a multiple of the other other, we can produce an exact result.
            // Otherwise, we compute the best approximate result with the larger resolution.
            var aRes = a.m_Subframe.Resolution;
            var bRes = b.m_Subframe.Resolution;

            if (aRes % bRes == 0)
            {
                var subframes = a.m_Subframe.Value - (b.m_Subframe.Value * (aRes / bRes));
                var extraFrames = 0;

                if (subframes < 0)
                {
                    subframes += aRes;
                    extraFrames = 1;
                }

                int frameNumber;

                checked
                {
                    frameNumber = a.m_FrameNumber - (b.m_FrameNumber + extraFrames);
                }

                return new FrameTime(frameNumber, new Subframe(subframes, aRes));
            }
            if (bRes % aRes == 0)
            {
                var subframes = (a.m_Subframe.Value * (bRes / aRes)) - b.m_Subframe.Value;
                var extraFrames = 0;

                if (subframes < 0)
                {
                    subframes += bRes;
                    extraFrames = 1;
                }

                int frameNumber;

                checked
                {
                    frameNumber = a.m_FrameNumber - (b.m_FrameNumber + extraFrames);
                }

                return new FrameTime(frameNumber, new Subframe(subframes, bRes));
            }

            return FromFrameTimeChecked((double)a - (double)b, Mathf.Max(aRes, bRes));
        }

        /// <summary>
        /// Gets the value of a <see cref="FrameTime"/> as a <see cref="float"/>.
        /// </summary>
        /// <param name="rate">The value to cast.</param>
        /// <returns>The frame time with the frame number in the integer part and the subframe in the decimal part.</returns>
        public static explicit operator float(FrameTime rate)
        {
            return rate.m_FrameNumber + rate.m_Subframe.AsFloat();
        }

        /// <summary>
        /// Gets the value of a <see cref="FrameTime"/> as a <see cref="double"/>.
        /// </summary>
        /// <param name="rate">The value to cast.</param>
        /// <returns>The frame time with the frame number in the integer part and the subframe in the decimal part.</returns>
        public static explicit operator double(FrameTime rate)
        {
            return rate.m_FrameNumber + rate.m_Subframe.AsDouble();
        }
    }
}
