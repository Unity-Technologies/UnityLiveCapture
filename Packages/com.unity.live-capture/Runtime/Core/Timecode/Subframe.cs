using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A struct that represents a time within a frame interval.
    /// </summary>
    /// <remarks>
    /// The value is stored as a fractional number so that computations are guaranteed to have predictable results and consistent precision.
    /// </remarks>
    [Serializable]
    public struct Subframe : IComparable, IComparable<Subframe>, IEquatable<Subframe>
    {
        /// <summary>
        /// The maximum supported subframe resolution.
        /// </summary>
        public const int MaxResolution = ushort.MaxValue;

        /// <summary>
        /// The default resolution of the subframe value.
        /// </summary>
        /// <remarks>
        /// Values such as 80 or 100 are typical for SMTPE timecode. This chosen value is the lowest common
        /// multiple of 80, 100, and 2048, which allows for exact precision when using those values or smaller
        /// powers of 2.
        /// </remarks>
        public const int DefaultResolution = 51200;

        [SerializeField]
        ushort m_Subframe;
        [SerializeField]
        ushort m_Resolution;

        /// <summary>
        /// The subframe within the frame.
        /// </summary>
        /// <remarks>
        /// This value is in the range [0, <see name="resolution"/> - 1].
        /// </remarks>
        public readonly int Value => Mathf.Clamp(m_Subframe, 0, Resolution - 1);

        /// <summary>
        /// The number of possible subframe values in the frame.
        /// </summary>
        /// <remarks>
        /// This value is in the range [1, <see cref="MaxResolution"/>]
        /// </remarks>
        public readonly int Resolution => Mathf.Clamp(m_Resolution, 1, MaxResolution);

        /// <summary>
        /// Creates a new <see cref="Subframe"/> instance.
        /// </summary>
        /// <param name="subframe">The subframe value used to indicate a time somewhere within the duration of the frame.
        /// This value is clamped to the range [0, <paramref name="resolution"/> - 1].</param>
        /// <param name="resolution">The number of possible subframe values in the frame. If this value is not greater than zero,
        /// the subframe value is treated as zero.</param>
        public Subframe(int subframe, int resolution)
        {
            var divisor = Mathf.Clamp(resolution, 1, MaxResolution);

            m_Subframe = (ushort)Mathf.Clamp(subframe, 0, divisor - 1);
            m_Resolution = (ushort)divisor;
        }

        /// <summary>
        /// Creates a new <see cref="Subframe"/> value from a <see cref="float"/>.
        /// </summary>
        /// <param name="subframe">A subframe value used to indicate a time somewhere within the duration of the frame.
        /// It is clamped to the range [0, 1].</param>
        /// <param name="resolution">The number of possible subframe values in the frame. If this value is not greater than zero,
        /// the subframe value is treated as zero.</param>
        /// <returns>A new <see cref="Subframe"/> that represents the given frame time.</returns>
        public static Subframe FromFloat(float subframe, int resolution = DefaultResolution)
        {
            return FromDouble(subframe, resolution);
        }

        /// <summary>
        /// Creates a new <see cref="Subframe"/> value from a <see cref="double"/>.
        /// </summary>
        /// <param name="subframe">A subframe value used to indicate a time somewhere within the duration of the frame.
        /// It is clamped to the range [0, 1].</param>
        /// <param name="resolution">The number of possible subframe values in the frame. If this value is not greater than zero,
        /// the subframe value is treated as zero.</param>
        /// <returns>A new <see cref="Subframe"/> that represents the given frame time.</returns>
        public static Subframe FromDouble(double subframe, int resolution = DefaultResolution)
        {
            return new Subframe(
                (int)Math.Min(Math.Max(Math.Round(subframe * resolution), 0.0), int.MaxValue),
                resolution
            );
        }

        /// <summary>
        /// Gets the subframe value as a <see cref="float"/>.
        /// </summary>
        /// <returns>The subframe value in the range [0, 1].</returns>
        public readonly float AsFloat()
        {
            return (float)Value / Resolution;
        }

        /// <summary>
        /// Gets the subframe value as a <see cref="double"/>.
        /// </summary>
        /// <returns>The subframe value in the range [0, 1].</returns>
        public readonly double AsDouble()
        {
            return (double)Value / Resolution;
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Subframe"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.
        /// * Returns a negative value when this instance is less than <paramref name="other"/>.
        /// * Returns zero when this instance is the same as <paramref name="other"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="other"/>.
        /// </returns>
        public readonly int CompareTo(Subframe other)
        {
            GetComparableValues(in this, in other, out var a, out var b);
            return a.CompareTo(b);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj"/>.
        /// * Returns a negative value when <paramref name="obj"/> is not a valid <see cref="Subframe"/> instance or this instance is less than <paramref name="obj"/>.
        /// * Returns zero when this instance is the same as <paramref name="obj"/>.
        /// * Returns a positive value when this instance is greater than <paramref name="obj"/>.
        /// </returns>
        public readonly int CompareTo(object obj)
        {
            return obj is Subframe other ? CompareTo(other) : -1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Subframe"/>.
        /// </summary>
        /// <param name="other">A value to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(Subframe other)
        {
            GetComparableValues(in this, in other, out var a, out var b);
            return a == b;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="Subframe"/> and equals
        /// the value of this instance; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj)
        {
            return obj is Subframe other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hashCode = Value;
                hashCode = (hashCode * 397) ^ Resolution;
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>A string that represents the current instance.</returns>
        public override readonly string ToString()
        {
            return AsFloat().ToString();
        }

        /// <summary>
        /// Indicates whether two specified instances of <see cref="Subframe"/> are equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Subframe a, Subframe b) => a.Equals(b);

        /// <summary>
        /// Indicates whether two specified instances of <see cref="Subframe"/> are not equal.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> do not have the same value; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Subframe a, Subframe b) => !a.Equals(b);

        /// <summary>
        /// Indicates whether one specified <see cref="Subframe"/> is later than or the same as another specified <see cref="Subframe"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(Subframe a, Subframe b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="Subframe"/> is earlier than or the same as another specified <see cref="Subframe"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than or the same as <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(Subframe a, Subframe b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Indicates whether one specified <see cref="Subframe"/> is later than another specified <see cref="Subframe"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is later than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(Subframe a, Subframe b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Indicates whether one specified <see cref="Subframe"/> is earlier than another specified <see cref="Subframe"/>.
        /// </summary>
        /// <param name="a">The first instance to compare.</param>
        /// <param name="b">The second instance to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is earlier than <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(Subframe a, Subframe b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Gets the value of a <see cref="Subframe"/> as a <see cref="float"/>.
        /// </summary>
        /// <param name="subframe">The value to cast.</param>
        /// <returns>The subframe value in the range [0, 1].</returns>
        public static explicit operator float(Subframe subframe)
        {
            return subframe.AsFloat();
        }

        /// <summary>
        /// Gets the value of a <see cref="Subframe"/> as a <see cref="double"/>.
        /// </summary>
        /// <param name="subframe">The value to cast.</param>
        /// <returns>The subframe value in the range [0, 1].</returns>
        public static explicit operator double(Subframe subframe)
        {
            return subframe.AsDouble();
        }

        static void GetComparableValues(in Subframe s0, in Subframe s1, out uint v0, out uint v1)
        {
            // For comparisons we use the exact value of the subframe without using floating point math using the classic change of base
            // formula. Care must by taken to ensure that the multiplied values cannot overflow by casting to a type big enough to contain
            // the result. For safety, we could use the checked keyword, but there is a performance cost.
            v0 = (uint)s0.Value * (uint)s1.Resolution;
            v1 = (uint)s1.Value * (uint)s0.Resolution;
        }
    }
}
