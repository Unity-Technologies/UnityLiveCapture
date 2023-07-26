using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class FrameRateTests
    {
        static IEnumerable TestSimpleConstructorData
        {
            get
            {
                // invalid cases
                yield return new TestCaseData(
                    int.MinValue,
                    0, 0,
                    true
                );
                yield return new TestCaseData(
                    -24,
                    0, 0,
                    true
                );
                yield return new TestCaseData(
                    -1,
                    0, 0,
                    true
                );

                // valid cases
                yield return new TestCaseData(
                    0,
                    0, 1,
                    false
                );
                yield return new TestCaseData(
                    1,
                    1, 1,
                    false
                );
                yield return new TestCaseData(
                    5,
                    5, 1,
                    false
                );
                yield return new TestCaseData(
                    24,
                    24, 1,
                    false
                );
                yield return new TestCaseData(
                    240,
                    240, 1,
                    false
                );
                yield return new TestCaseData(
                    int.MaxValue,
                    int.MaxValue, 1,
                    false
                );
            }
        }

        [Test, TestCaseSource(nameof(TestSimpleConstructorData))]
        public void TestSimpleConstructor(
            int numerator,
            int expectedNumerator,
            int expectedDenominator,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new FrameRate(numerator));
            }
            else
            {
                var rate = new FrameRate(numerator);

                Assert.AreEqual(expectedNumerator, rate.Numerator);
                Assert.AreEqual(expectedDenominator, rate.Denominator);
            }
        }

        static IEnumerable TestRationalConstructorData
        {
            get
            {
                // invalid cases
                yield return new TestCaseData(
                    int.MinValue, 1, false,
                    0, 0, false,
                    true
                );
                yield return new TestCaseData(
                    -24, 1, false,
                    0, 0, false,
                    true
                );
                yield return new TestCaseData(
                    -1, 1, false,
                    0, 0, false,
                    true
                );
                yield return new TestCaseData(
                    1, int.MinValue, false,
                    0, 0, false,
                    true
                );
                yield return new TestCaseData(
                    1, -24, false,
                    0, 0, false,
                    true
                );
                yield return new TestCaseData(
                    1, -1, false,
                    0, 0, false,
                    true
                );

                // valid non-rational cases
                yield return new TestCaseData(
                    0, 0, false,
                    0, 0, false,
                    false
                );

                yield return new TestCaseData(
                    0, 1, false,
                    0, 1, false,
                    false
                );
                yield return new TestCaseData(
                    1, 1, false,
                    1, 1, false,
                    false
                );
                yield return new TestCaseData(
                    5, 1, false,
                    5, 1, false,
                    false
                );
                yield return new TestCaseData(
                    24, 1, false,
                    24, 1, false,
                    false
                );
                yield return new TestCaseData(
                    240, 1, false,
                    240, 1, false,
                    false
                );
                yield return new TestCaseData(
                    int.MaxValue, 1, false,
                    int.MaxValue, 1, false,
                    false
                );

                // valid rational cases
                yield return new TestCaseData(
                    1, 3, true,
                    1, 3, false,
                    false
                );
                yield return new TestCaseData(
                    31, 17, true,
                    31, 17, false,
                    false
                );
                yield return new TestCaseData(
                    51, 5, true,
                    51, 5, false,
                    false
                );
                yield return new TestCaseData(
                    26000, 1013, true,
                    26000, 1013, false,
                    false
                );

                yield return new TestCaseData(
                    24000, 1001, false,
                    24000, 1001, false,
                    false
                );
                yield return new TestCaseData(
                    30000, 1001, false,
                    30000, 1001, false,
                    false
                );
                yield return new TestCaseData(
                    60000, 1001, false,
                    60000, 1001, false,
                    false
                );

                yield return new TestCaseData(
                    24000, 1001, true,
                    24000, 1001, true,
                    false
                );
                yield return new TestCaseData(
                    30000, 1001, true,
                    30000, 1001, true,
                    false
                );
                yield return new TestCaseData(
                    60000, 1001, true,
                    60000, 1001, true,
                    false
                );

                // valid reducible rational cases
                yield return new TestCaseData(
                    51, 3, true,
                    17, 1, false,
                    false
                );
                yield return new TestCaseData(
                    48, 2, true,
                    24, 1, false,
                    false
                );
                yield return new TestCaseData(240, 10, true,
                    24, 1, false,
                    false
                );
                yield return new TestCaseData(30000, 1000, true,
                    30, 1, false,
                    false
                );

                yield return new TestCaseData(
                    48000, 2002, false,
                    24000, 1001, false,
                    false
                );
                yield return new TestCaseData(
                    60000, 2002, false,
                    30000, 1001, false,
                    false
                );
                yield return new TestCaseData(
                    120000, 2002, false,
                    60000, 1001, false,
                    false
                );

                yield return new TestCaseData(
                    48000, 2002, true,
                    24000, 1001, true,
                    false
                );
                yield return new TestCaseData(
                    60000, 2002, true,
                    30000, 1001, true,
                    false
                );
                yield return new TestCaseData(
                    120000, 2002, true,
                    60000, 1001, true,
                    false
                );
            }
        }

        [Test, TestCaseSource(nameof(TestRationalConstructorData))]
        public void TestRationalConstructor(
            int numerator,
            int denominator,
            bool isDropFrame,
            int expectedNumerator,
            int expectedDenominator,
            bool expectedIsDropFrame,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new FrameRate(numerator, denominator, isDropFrame));
            }
            else
            {
                var rate = new FrameRate(numerator, denominator, isDropFrame);

                Assert.AreEqual(expectedNumerator, rate.Numerator);
                Assert.AreEqual(expectedDenominator, rate.Denominator);
                Assert.AreEqual(expectedIsDropFrame, rate.IsDropFrame);
            }
        }

        static IEnumerable TestIsValidData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0, 0, false),
                    false
                );
                yield return new TestCaseData(
                    new FrameRate(1, 0, false),
                    false
                );

                yield return new TestCaseData(
                    new FrameRate(0),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(0, 1, false),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(1, 1, false),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(int.MaxValue, 10, false),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestIsValidData))]
        public void TestIsValid(
            FrameRate frameRate,
            bool expectedIsValid
        )
        {
            Assert.AreEqual(expectedIsValid, frameRate.IsValid);
        }

        static IEnumerable TestReciprocalData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    new FrameRate(1, 0, false)
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(1, 1, false)
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    new FrameRate(1, 24, false)
                );
                yield return new TestCaseData(
                    new FrameRate(51, 23, false),
                    new FrameRate(23, 51, false)
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, true),
                    new FrameRate(1001, 24000, false)
                );
                yield return new TestCaseData(
                    new FrameRate(int.MaxValue),
                    new FrameRate(1, int.MaxValue, false)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestReciprocalData))]
        public void TestReciprocal(
            FrameRate frameRate,
            FrameRate reciprocal
        )
        {
            Assert.AreEqual(reciprocal, frameRate.Reciprocal);
        }

        static IEnumerable TestReciprocalOfReciprocalData
        {
            get
            {
                yield return new TestCaseData(new FrameRate(0));
                yield return new TestCaseData(new FrameRate(1));
                yield return new TestCaseData(new FrameRate(24));
                yield return new TestCaseData(new FrameRate(51, 23, false));
                yield return new TestCaseData(new FrameRate(24000, 1001, true));
                yield return new TestCaseData(new FrameRate(60000, 1001, true));
                yield return new TestCaseData(new FrameRate(24000, 1001, false));
                yield return new TestCaseData(new FrameRate(60000, 1001, false));
                yield return new TestCaseData(new FrameRate(int.MaxValue));
            }
        }

        [Test, TestCaseSource(nameof(TestReciprocalOfReciprocalData))]
        public void TestReciprocalOfReciprocal(
            FrameRate frameRate
        )
        {
            Assert.AreEqual(frameRate, frameRate.Reciprocal.Reciprocal);
        }

        static IEnumerable TestIntervalData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    double.PositiveInfinity
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    1.0
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    0.04166666666
                );
                yield return new TestCaseData(
                    new FrameRate(51, 23, false),
                    0.45098039215
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, false),
                    0.04170833333
                );
                yield return new TestCaseData(
                    new FrameRate(int.MaxValue),
                    0.0
                );
            }
        }

        [Test, TestCaseSource(nameof(TestIntervalData))]
        public void TestInterval(
            FrameRate frameRate,
            double expectedInterval
        )
        {
            Assert.AreEqual(expectedInterval, frameRate.FrameInterval, 0.000000001);
        }

        static IEnumerable TestIsMultipleOfData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    new FrameRate(1),
                    false
                );
                yield return new TestCaseData(
                    new FrameRate(48),
                    new FrameRate(24),
                    false
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(1),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(24),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    new FrameRate(48),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(30),
                    new FrameRate(60),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestIsMultipleOfData))]
        public void TestIsMultipleOf(
            FrameRate a,
            FrameRate b,
            bool expectedIsMultiple
        )
        {
            Assert.AreEqual(expectedIsMultiple, a.IsMultipleOf(b));
        }

        static IEnumerable TestIsFactorOfData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    new FrameRate(1),
                    false
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    new FrameRate(48),
                    false
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(1),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    new FrameRate(1),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(48),
                    new FrameRate(24),
                    true
                );
                yield return new TestCaseData(
                    new FrameRate(60),
                    new FrameRate(30),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestIsFactorOfData))]
        public void TestIsFactorOf(
            FrameRate a,
            FrameRate b,
            bool expectedIsFactorOf
        )
        {
            Assert.AreEqual(expectedIsFactorOf, a.IsFactorOf(b));
        }

        static IEnumerable TestAsFloatData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    0.0f
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    1.0f
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    24.0f
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, false),
                    23.976023976f
                );
                yield return new TestCaseData(
                    new FrameRate(30000, 1001, false),
                    29.97002997f
                );
            }
        }

        [Test, TestCaseSource(nameof(TestAsFloatData))]
        public void TestAsFloat(
            FrameRate a,
            float expectedValue
        )
        {
            Assert.AreEqual(expectedValue, a.AsFloat(), 0.000001f);
        }

        static IEnumerable TestAsDoubleData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    0.0
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    1.0
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    24.0
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, false),
                    23.976023976
                );
                yield return new TestCaseData(
                    new FrameRate(30000, 1001, false),
                    29.97002997
                );
            }
        }

        [Test, TestCaseSource(nameof(TestAsDoubleData))]
        public void TestAsDouble(
            FrameRate a,
            double expectedValue
        )
        {
            Assert.AreEqual(expectedValue, a.AsDouble(), 0.000001);
        }

        static IEnumerable TestIsNtscData
        {
            get
            {
                yield return new TestCaseData(24000, 1001, true);
                yield return new TestCaseData(30000, 1001, true);
                yield return new TestCaseData(60000, 1001, true);

                yield return new TestCaseData(48000, 2002, true);
                yield return new TestCaseData(90000, 3003, true);
                yield return new TestCaseData(240000, 4004, true);

                yield return new TestCaseData(24000, 1000, false);
                yield return new TestCaseData(30000, 1000, false);
                yield return new TestCaseData(60000, 1000, false);
                yield return new TestCaseData(23000, 1001, false);
                yield return new TestCaseData(31000, 1001, false);
                yield return new TestCaseData(59000, 1001, false);
            }
        }

        [Test, TestCaseSource(nameof(TestIsNtscData))]
        public void TestIsNtsc(
            int numerator,
            int denominator,
            bool expectedValue
        )
        {
            Assert.AreEqual(expectedValue, FrameRate.IsNtsc(numerator, denominator));
        }

        static IEnumerable TestCompareToData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    new FrameRate(0),
                    0
                );
                yield return new TestCaseData(
                    new FrameRate(0, 0, false),
                    new FrameRate(1, 0, false),
                    0
                );
                yield return new TestCaseData(
                    new FrameRate(0, 0, false),
                    new FrameRate(1, 1, false),
                    -1
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(1),
                    0
                );

                yield return new TestCaseData(
                    new FrameRate(23),
                    new FrameRate(24),
                    -1
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    new FrameRate(24),
                    0
                );
                yield return new TestCaseData(
                    new FrameRate(25),
                    new FrameRate(24),
                    1
                );

                yield return new TestCaseData(
                    new FrameRate(23999, 1001, true),
                    new FrameRate(24000, 1001, true),
                    -1
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, true),
                    new FrameRate(24000, 1001, true),
                    0
                );
                yield return new TestCaseData(
                    new FrameRate(24001, 1001, true),
                    new FrameRate(24000, 1001, true),
                    1
                );

                yield return new TestCaseData(
                    new FrameRate(int.MaxValue - 1, int.MaxValue, false),
                    new FrameRate(int.MaxValue, int.MaxValue, false),
                    -1
                );
                yield return new TestCaseData(
                    new FrameRate(int.MaxValue, int.MaxValue, false),
                    new FrameRate(int.MaxValue, int.MaxValue, false),
                    0
                );

                // is drop frame comparison
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, false),
                    new FrameRate(24000, 1001, true),
                    -1
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, false),
                    new FrameRate(24000, 1001, false),
                    0
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, true),
                    new FrameRate(24000, 1001, false),
                    1
                );
            }
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareTo(
            FrameRate a,
            FrameRate b,
            int expectedValue
        )
        {
            if (expectedValue < 0)
            {
                Assert.Negative(a.CompareTo(b));
            }
            else if (expectedValue > 0)
            {
                Assert.Positive(a.CompareTo(b));
            }
            else
            {
                Assert.Zero(a.CompareTo(b));
            }
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestEquals(
            FrameRate a,
            FrameRate b,
            int expectedValue
        )
        {
            Assert.AreEqual(expectedValue == 0, a.Equals(b));
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareOperators(
            FrameRate a,
            FrameRate b,
            int expectedValue
        )
        {
            if (expectedValue < 0)
            {
                Assert.IsTrue(a < b);
                Assert.IsTrue(a <= b);
                Assert.IsFalse(a == b);
                Assert.IsFalse(a >= b);
                Assert.IsFalse(a > b);

                Assert.IsTrue(a != b);
            }
            else if (expectedValue > 0)
            {
                Assert.IsFalse(a < b);
                Assert.IsFalse(a <= b);
                Assert.IsFalse(a == b);
                Assert.IsTrue(a >= b);
                Assert.IsTrue(a > b);

                Assert.IsTrue(a != b);
            }
            else
            {
                Assert.IsFalse(a < b);
                Assert.IsTrue(a <= b);
                Assert.IsTrue(a == b);
                Assert.IsTrue(a >= b);
                Assert.IsFalse(a > b);

                Assert.IsFalse(a != b);
            }
        }

        static IEnumerable TestMultiplyData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    new FrameRate(0),
                    new FrameRate(0)
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(1),
                    new FrameRate(1)
                );
                yield return new TestCaseData(
                    new FrameRate(24),
                    new FrameRate(2),
                    new FrameRate(48)
                );
                yield return new TestCaseData(
                    new FrameRate(30),
                    new FrameRate(59),
                    new FrameRate(1770)
                );
                yield return new TestCaseData(
                    new FrameRate(24000, 1001, false),
                    new FrameRate(2),
                    new FrameRate(48000, 1001, false)
                );
                yield return new TestCaseData(
                    new FrameRate(1, 2, false),
                    new FrameRate(1, 4, false),
                    new FrameRate(1, 8, false)
                );
                yield return new TestCaseData(
                    new FrameRate(5, 2, false),
                    new FrameRate(4, 3, false),
                    new FrameRate(10, 3, false)
                );
                yield return new TestCaseData(
                    new FrameRate(short.MaxValue, 1000, false),
                    new FrameRate(short.MaxValue, 1000, false),
                    new FrameRate(short.MaxValue * short.MaxValue, 1000 * 1000, false)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestMultiplyData))]
        public void TestMultiply(
            FrameRate a,
            FrameRate b,
            FrameRate expectedValue
        )
        {
            Assert.AreEqual(expectedValue, a * b);
        }

        static IEnumerable TestDivideData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(0),
                    new FrameRate(0),
                    new FrameRate(0, 0, false)
                );
                yield return new TestCaseData(
                    new FrameRate(1),
                    new FrameRate(1),
                    new FrameRate(1)
                );
                yield return new TestCaseData(
                    new FrameRate(48),
                    new FrameRate(2),
                    new FrameRate(24)
                );
                yield return new TestCaseData(
                    new FrameRate(1770),
                    new FrameRate(59),
                    new FrameRate(30)
                );
                yield return new TestCaseData(
                    new FrameRate(48000, 1001, false),
                    new FrameRate(2),
                    new FrameRate(24000, 1001, false)
                );
                yield return new TestCaseData(
                    new FrameRate(1, 8, false),
                    new FrameRate(1, 4, false),
                    new FrameRate(1, 2, false)
                );
                yield return new TestCaseData(
                    new FrameRate(10, 3, false),
                    new FrameRate(4, 3, false),
                    new FrameRate(5, 2, false)
                );
                yield return new TestCaseData(
                    new FrameRate(short.MaxValue * short.MaxValue, 1000 * 1000, false),
                    new FrameRate(short.MaxValue, 1000, false),
                    new FrameRate(short.MaxValue, 1000, false)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestDivideData))]
        public void TestDivide(
            FrameRate a,
            FrameRate b,
            FrameRate expectedValue
        )
        {
            Assert.AreEqual(expectedValue, a / b);
        }
    }
}
