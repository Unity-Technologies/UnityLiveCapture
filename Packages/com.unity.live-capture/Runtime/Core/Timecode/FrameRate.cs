using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that stores a frame rate in Hz as a fractional number to avoid imprecision.
    /// </summary>
    [Serializable]
    public struct FrameRate : IComparable, IComparable<FrameRate>, IEquatable<FrameRate>
    {
        [SerializeField]
        uint m_Numerator;
        [SerializeField]
        uint m_Denominator;
        [SerializeField]
        bool m_IsDropFrame;

        /// <summary>
        /// The frame rate fraction numerator.
        /// </summary>
        public readonly int Numerator => (int)m_Numerator;

        /// <summary>
        /// The frame rate fraction denominator.
        /// </summary>
        public readonly int Denominator => (int)m_Denominator;

        /// <summary>
        /// Should drop frame calculations be performed when converting between clock time and frame time using this frame rate.
        /// </summary>
        /// <remarks>
        /// This can only be <see langword="true"/> for NTSC frame rates.
        /// </remarks>
        public readonly bool IsDropFrame => m_IsDropFrame && IsNtsc(m_Numerator, m_Denominator);

        /// <summary>
        /// Is the frame rate valid.
        /// </summary>
        public readonly bool IsValid => m_Denominator != 0;

        /// <summary>
        /// Gets the reciprocal of the frame rate fraction.
        /// </summary>
        public readonly FrameRate Reciprocal => new FrameRate(m_Denominator, m_Numerator, m_IsDropFrame);

        /// <summary>
        /// Gets the length of time between frames in seconds.
        /// </summary>
        public readonly double FrameInterval => (double)Reciprocal;

        /// <summary>
        /// Creates a new <see cref="FrameRate"/> instance.
        /// </summary>
        /// <param name="frameRate">The frame rate in Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="frameRate"/> is negative.</exception>
        public FrameRate(int frameRate)
        {
            if (frameRate < 0)
                throw new ArgumentOutOfRangeException(nameof(frameRate), frameRate, "Must be non-negative!");

            m_Numerator = (uint)frameRate;
            m_Denominator = 1u;
            m_IsDropFrame = false;
        }

        /// <summary>
        /// Creates a new <see cref="FrameRate"/> instance.
        /// </summary>
        /// <param name="numerator">The fraction numerator.</param>
        /// <param name="denominator">The fraction denominator.</param>
        /// <param name="isDropFrame">Should drop frame calculations be performed when converting between
        /// clock time and frame time using this frame rate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="numerator"/> or <see cref="Denominator"/> is negative.</exception>
        public FrameRate(int numerator, int denominator, bool isDropFrame)
        {
            if (numerator < 0)
                throw new ArgumentOutOfRangeException(nameof(numerator), numerator, "Must be non-negative!");
            if (denominator < 0)
                throw new ArgumentOutOfRangeException(nameof(denominator), denominator, "Must be non-negative!");

            var num = (ulong)numerator;
            var den = (ulong)denominator;

            ReduceFraction(ref num, ref den);

            m_Numerator = (uint)num;
            m_Denominator = (uint)den;
            m_IsDropFrame = isDropFrame;
        }

        FrameRate(ulong numerator, ulong denominator, bool isDropFrame)
        {
            // Reduce the fraction by the greatest common denominator. We also want to make sure that there is no silent
            // overflow. If there is, we throw an exception so the user can decide what action to take. By reducing the
            // fraction before casting to the smaller number type we are able to avoid overflow in more cases.
            ReduceFraction(ref numerator, ref denominator);

            checked
            {
                m_Numerator = (uint)numerator;
                m_Denominator = (uint)denominator;
            }

            m_IsDropFrame = isDropFrame;
        }

        /// <summary>
        /// Checks if this <see cref="FrameRate"/> is a multiple of another.
        /// </summary>
        /// <remarks>
        /// This is based on the time between frames, so 24 fps is considered a multiple of 48 fps.
        /// </remarks>
        /// <param name="other">The frame rate to compare against.</param>
        /// <returns><see langword="true"/> if this frame rate is a multiple of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public readonly bool IsMultipleOf(FrameRate other)
        {
            var a = (ulong)m_Numerator * other.m_Denominator;
            var b = (ulong)other.m_Numerator * m_Denominator;

            return a != 0 && b != 0 && (b % a) == 0;
        }

        /// <summary>
        /// Checks if this <see cref="FrameRate"/> is a factor of another.
        /// </summary>
        /// <remarks>
        /// This is based on the time between frames, so 48 fps is considered a factor of 24 fps.
        /// </remarks>
        /// <param name="other">The frame rate to compare against.</param>
        /// <returns><see langword="true"/> if this frame rate is a factor of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public readonly bool IsFactorOf(FrameRate other)
        {
            return other.IsMultipleOf(this);
        }

        /// <summary>
        /// Gets the frame rate as a <see cref="float"/>.
        /// </summary>
        /// <returns>The frame rate in Hz.</returns>
        public readonly float AsFloat()
        {
            return (float)m_Numerator / m_Denominator;
        }

        /// <summary>
        /// Gets the frame rate as a <see cref="double"/>.
        /// </summary>
        /// <returns>The frame rate in Hz.</returns>
        public readonly double AsDouble()
        {
            return (double)m_Numerator / m_Denominator;
        }

        /// <summary>
        /// Checks if a frame rate is an NTSC frame rate (23.976, 29.970, 59.94).
        /// </summary>
        /// <param name="numerator">The frame rate fraction numerator.</param>
        /// <param name="denominator">The frame rate fraction denominator.</param>
        /// <returns><see langword="true"/> if the given fraction corresponds to an NTSC frame rate; otherwise, <see langword="false"/>.</returns>
        public static bool IsNtsc(int numerator, int denominator)
        {
            var num = (ulong)numerator;
            var den = (ulong)denominator;
            ReduceFraction(ref num, ref den);
            return IsNtsc((uint)num, (uint)den);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="FrameRate"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// * Returns a negative value when this instance is less than <paramref name="other"/>.
        /// * Returns zero when this instance is the same as <paramref name="other"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="other"/>.
        /// </returns>
        public readonly int CompareTo(FrameRate other)
        {
            GetComparableValues(this, other, out var a, out var b);
            return a != b ? a.CompareTo(b) : IsDropFrame.CompareTo(other.IsDropFrame);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj"/>.
        /// * Returns a negative value when <paramref name="obj"/> is not a valid <see cref="FrameRate"/> instance or this instance is less than <paramref name="obj"/>.
        /// * Returns zero when this instance is the same as <paramref name="obj"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="obj"/>.
        /// </returns>
        public readonly int CompareTo(object obj)
        {
            if (obj is FrameRate frameRate)
            {
                return CompareTo(frameRate);
            }
            return -1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(FrameRate other)
        {
            GetComparableValues(this, other, out var a, out var b);
            return a == b && IsDropFrame == other.IsDropFrame;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="FrameRate"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj)
        {
            return obj is FrameRate other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hash = m_Numerator.GetHashCode();
                hash = (hash * 397) ^ m_Denominator.GetHashCode();
                hash = (hash * 397) ^ IsDropFrame.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>A string that represents the current instance.</returns>
        public override readonly string ToString()
        {
            var str = AsFloat().ToString("0.###");

            if (IsNtsc(m_Numerator, m_Denominator))
            {
                str += m_IsDropFrame ? " DF" : " NDF";
            }

            return str;
        }

        /// <summary>
        /// Indicates whether two specified instances of <see cref="FrameRate"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(FrameRate a, FrameRate b) => a.Equals(b);

        /// <summary>
        /// Indicates whether two specified instances of <see cref="FrameRate"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(FrameRate a, FrameRate b) => !a.Equals(b);

        /// <summary>
        /// Indicates whether one specified <see cref="FrameRate"/> is greater than or the same as another specified <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is greater than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(FrameRate a, FrameRate b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="FrameRate"/> is less than or the same as another specified <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is less than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(FrameRate a, FrameRate b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="FrameRate"/> is greater than another specified <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is greater than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(FrameRate a, FrameRate b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Indicates whether one specified <see cref="FrameRate"/> is less than another specified <see cref="FrameRate"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is less than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(FrameRate a, FrameRate b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Computes the multiple of two <see cref="FrameRate"/> values.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The multiplied frame rate.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting frame rate is not representable
        /// using the <see cref="FrameRate"/> type.</exception>
        public static FrameRate operator *(FrameRate a, FrameRate b)
        {
            return new FrameRate(
                (ulong)a.m_Numerator * b.m_Numerator,
                (ulong)a.m_Denominator * b.m_Denominator,
                a.m_IsDropFrame || b.m_IsDropFrame
            );
        }

        /// <summary>
        /// Computes the division of two <see cref="FrameRate"/> values.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="b">The divisor.</param>
        /// <returns>The quotient.</returns>
        /// <exception cref="OverflowException">Thrown if the resulting frame rate is not representable
        /// using the <see cref="FrameRate"/> type.</exception>
        public static FrameRate operator /(FrameRate a, FrameRate b)
        {
            return new FrameRate(
                (ulong)a.m_Numerator * b.m_Denominator,
                (ulong)b.m_Numerator * a.m_Denominator,
                a.m_IsDropFrame || b.m_IsDropFrame
            );
        }

        /// <summary>
        /// Gets the value of a <see cref="FrameRate"/> in Hz as a <see cref="float"/>.
        /// </summary>
        /// <param name="rate">The value to cast.</param>
        /// <returns>The frame rate in Hz.</returns>
        public static explicit operator float(FrameRate rate)
        {
            return rate.AsFloat();
        }

        /// <summary>
        /// Gets the value of a <see cref="FrameRate"/> in Hz as a <see cref="double"/>.
        /// </summary>
        /// <param name="rate">The value to cast.</param>
        /// <returns>The frame rate in Hz.</returns>
        public static explicit operator double(FrameRate rate)
        {
            return rate.AsDouble();
        }

        /// <inheritdoc cref="StandardFrameRateExtensions.ToValue"/>
        public static implicit operator FrameRate(StandardFrameRate rate)
        {
            return rate.ToValue();
        }

        static void GetComparableValues(in FrameRate r0, in FrameRate r1, out ulong v0, out ulong v1)
        {
            // We compare the exact value of the frame rates without using floating point math using the classic change of base
            // formula. Care must by taken to ensure that the multiplied values cannot overflow by casting to a type big enough
            // to contain the result. For safety, we could use the checked keyword, but there is a performance cost.
            v0 = r1.m_Denominator > 0 ? (ulong)r0.m_Numerator * r1.m_Denominator : ulong.MaxValue;
            v1 = r0.m_Denominator > 0 ? (ulong)r1.m_Numerator * r0.m_Denominator : ulong.MaxValue;
        }

        static void ReduceFraction(ref ulong numerator, ref ulong denominator)
        {
            var divisor = GreatestCommonDenominator(numerator, denominator);
            numerator /= divisor;
            denominator /= divisor;
        }

        static ulong GreatestCommonDenominator(ulong a, ulong b)
        {
            // euclidean algorithm
            while (a != 0 && b != 0)
            {
                var t = b;
                b = a % b;
                a = t;
            }
            return a == 0 ? 1 : a;
        }

        static bool IsNtsc(uint numerator, uint denominator)
        {
            if (denominator == 1001)
            {
                switch (numerator)
                {
                    case 24000:
                    case 30000:
                    case 60000:
                        return true;
                }
            }

            return false;
        }
    }
}
