using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class SubframeTests
    {
        static IEnumerable TestConstructorData
        {
            get
            {
                // test valid cases
                yield return new TestCaseData(0, 80, 0, 80);
                yield return new TestCaseData(10, 80, 10, 80);
                yield return new TestCaseData(99, 100, 99, 100);
                yield return new TestCaseData(5000, ushort.MaxValue, 5000, ushort.MaxValue);

                // test cases were the input values are clamped
                yield return new TestCaseData(10, -10, 0, 1);
                yield return new TestCaseData(10, 0, 0, 1);
                yield return new TestCaseData(10, 1, 0, 1);
                yield return new TestCaseData(-100, 10, 0, 10);
                yield return new TestCaseData(10, 10, 9, 10);
                yield return new TestCaseData(100, 10, 9, 10);
                yield return new TestCaseData(int.MaxValue, int.MaxValue, ushort.MaxValue - 1, ushort.MaxValue);
            }
        }

        [Test, TestCaseSource(nameof(TestConstructorData))]
        public void TestConstructor(
            int subframe,
            int resolution,
            int expectedSubframe,
            int expectedResolution
        )
        {
            var sub = new Subframe(subframe, resolution);

            Assert.AreEqual(expectedSubframe, sub.Value);
            Assert.AreEqual(expectedResolution, sub.Resolution);
        }

        static IEnumerable TestFromFloatData
        {
            get
            {
                // test valid cases
                yield return new TestCaseData(0f, 80, 0, 80);
                yield return new TestCaseData(0.5f, 80, 40, 80);
                yield return new TestCaseData(0.25f, 100, 25, 100);
                yield return new TestCaseData(0.50f, 100, 50, 100);
                yield return new TestCaseData(0.71f, 100, 71, 100);
                yield return new TestCaseData(0.75f, 100, 75, 100);
                yield return new TestCaseData(0.5f, ushort.MaxValue, 32768, ushort.MaxValue);

                // test cases were the input values are clamped
                yield return new TestCaseData(0.1f, -10, 0, 1);
                yield return new TestCaseData(0.1f, 0, 0, 1);
                yield return new TestCaseData(0.1f, 1, 0, 1);
                yield return new TestCaseData(-10f, 10, 0, 10);
                yield return new TestCaseData(1f, 10, 9, 10);
                yield return new TestCaseData(10f, 10, 9, 10);

                // test that extreme inputs are properly clamped, as long as float precision is good enough
                yield return new TestCaseData(10f, int.MaxValue, ushort.MaxValue - 1, ushort.MaxValue);
            }
        }

        [Test, TestCaseSource(nameof(TestFromFloatData))]
        public void TestFromFloat(
            float subframe,
            int resolution,
            int expectedSubframe,
            int expectedResolution
        )
        {
            var sub = Subframe.FromFloat(subframe, resolution);

            Assert.AreEqual(expectedSubframe, sub.Value);
            Assert.AreEqual(expectedResolution, sub.Resolution);
        }

        static IEnumerable TestFromDoubleData
        {
            get
            {
                // test valid cases
                yield return new TestCaseData(0.0, 80, 0, 80);
                yield return new TestCaseData(0.5, 80, 40, 80);
                yield return new TestCaseData(0.25, 100, 25, 100);
                yield return new TestCaseData(0.50, 100, 50, 100);
                yield return new TestCaseData(0.71, 100, 71, 100);
                yield return new TestCaseData(0.75, 100, 75, 100);
                yield return new TestCaseData(0.5, ushort.MaxValue, 32768, ushort.MaxValue);

                // test cases were the input values are clamped
                yield return new TestCaseData(0.1, -10, 0, 1);
                yield return new TestCaseData(0.1, 0, 0, 1);
                yield return new TestCaseData(0.1, 1, 0, 1);
                yield return new TestCaseData(-10.0, 10, 0, 10);
                yield return new TestCaseData(1.0, 10, 9, 10);
                yield return new TestCaseData(10.0, 10, 9, 10);

                // test the most extreme input is properly clamped
                yield return new TestCaseData(10.0, int.MaxValue, ushort.MaxValue - 1, ushort.MaxValue);
            }
        }

        [Test, TestCaseSource(nameof(TestFromDoubleData))]
        public void TestFromDouble(
            double subframe,
            int resolution,
            int expectedSubframe,
            int expectedResolution
        )
        {
            var sub = Subframe.FromDouble(subframe, resolution);

            Assert.AreEqual(expectedSubframe, sub.Value);
            Assert.AreEqual(expectedResolution, sub.Resolution);
        }

        static IEnumerable TestToFloatData
        {
            get
            {
                yield return new TestCaseData(0, 1, 0f);
                yield return new TestCaseData(1, 1, 0f);
                yield return new TestCaseData(1, 2, 0.5f);

                yield return new TestCaseData(0, 80, 0f);
                yield return new TestCaseData(20, 80, 0.25f);
                yield return new TestCaseData(40, 80, 0.5f);

                yield return new TestCaseData(65, 100, 0.65f);

                yield return new TestCaseData(32768, ushort.MaxValue, 0.5f);
            }
        }

        [Test, TestCaseSource(nameof(TestToFloatData))]
        public void TestAsFloat(
            int subframe,
            int resolution,
            float expectedValue
        )
        {
            var sub = new Subframe(subframe, resolution);

            Assert.AreEqual(expectedValue, sub.AsFloat(), 0.00001f);
        }

        static IEnumerable TestToDoubleData
        {
            get
            {
                yield return new TestCaseData(0, 1, 0.0);
                yield return new TestCaseData(1, 1, 0.0);
                yield return new TestCaseData(1, 2, 0.5);

                yield return new TestCaseData(0, 80, 0.0);
                yield return new TestCaseData(20, 80, 0.25);
                yield return new TestCaseData(40, 80, 0.5);

                yield return new TestCaseData(65, 100, 0.65);

                yield return new TestCaseData(32768, ushort.MaxValue, 0.5);
            }
        }

        [Test, TestCaseSource(nameof(TestToDoubleData))]
        public void TestAsDouble(
            int subframe,
            int resolution,
            double expectedValue
        )
        {
            var sub = new Subframe(subframe, resolution);

            Assert.AreEqual(expectedValue, sub.AsDouble(), 0.00001);
        }

        static IEnumerable TestCompareToData
        {
            get
            {
                yield return new TestCaseData(new Subframe(0, 1), new Subframe(1, 80), -1);
                yield return new TestCaseData(new Subframe(0, 1), new Subframe(0, 80), 0);

                yield return new TestCaseData(new Subframe(5, 20), new Subframe(75, 80), -1);
                yield return new TestCaseData(new Subframe(15, 20), new Subframe(60, 80), 0);
                yield return new TestCaseData(new Subframe(15, 20), new Subframe(13, 80), 1);

                yield return new TestCaseData(new Subframe(3, 35), new Subframe(9, 80), -1);
                yield return new TestCaseData(new Subframe(13, 35), new Subframe(7, 80), 1);

                yield return new TestCaseData(new Subframe(ushort.MaxValue - 2, ushort.MaxValue), new Subframe(ushort.MaxValue - 1, ushort.MaxValue), -1);
                yield return new TestCaseData(new Subframe(ushort.MaxValue, ushort.MaxValue), new Subframe(ushort.MaxValue, ushort.MaxValue), 0);
                yield return new TestCaseData(new Subframe(int.MaxValue, int.MaxValue), new Subframe(int.MaxValue, int.MaxValue), 0);
            }
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareTo(
            Subframe a,
            Subframe b,
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

        static IEnumerable TestEqualsData
        {
            get
            {
                yield return new TestCaseData(new Subframe(0, 0), new Subframe(0, 1), true);
                yield return new TestCaseData(new Subframe(0, 0), new Subframe(0, int.MaxValue), true);

                yield return new TestCaseData(new Subframe(0, 80), new Subframe(0, 80), true);
                yield return new TestCaseData(new Subframe(1, 80), new Subframe(1, 80), true);
                yield return new TestCaseData(new Subframe(1, 80), new Subframe(0, 80), false);

                yield return new TestCaseData(new Subframe(20, 80), new Subframe(15, 60), true);
                yield return new TestCaseData(new Subframe(20, 80), new Subframe(20, 60), false);
                yield return new TestCaseData(new Subframe(21, 80), new Subframe(15, 60), false);

                yield return new TestCaseData(new Subframe(0, int.MaxValue), new Subframe(0, int.MaxValue), true);
                yield return new TestCaseData(new Subframe(ushort.MaxValue - 2, int.MaxValue), new Subframe(ushort.MaxValue - 2, int.MaxValue), true);
                yield return new TestCaseData(new Subframe(ushort.MaxValue - 3, int.MaxValue), new Subframe(ushort.MaxValue - 2, int.MaxValue), false);
            }
        }

        [Test, TestCaseSource(nameof(TestEqualsData))]
        public void TestEquals(
            Subframe a,
            Subframe b,
            bool expectedValue
        )
        {
            Assert.AreEqual(expectedValue, a.Equals(b));
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareOperators(
            Subframe a,
            Subframe b,
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
    }
}
