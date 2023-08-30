using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class FrameTimeTests
    {
        static IEnumerable TestConstructorData
        {
            get
            {
                yield return new TestCaseData(
                    0, new Subframe(),
                    0, new Subframe()
                );
                yield return new TestCaseData(
                    -10, Subframe.FromFloat(0.5f),
                    -10, Subframe.FromFloat(0.5f)
                );
                yield return new TestCaseData(
                    10, Subframe.FromFloat(0.5f),
                    10, Subframe.FromFloat(0.5f)
                );
                yield return new TestCaseData(
                    int.MinValue, Subframe.FromFloat(0.5f),
                    int.MinValue, Subframe.FromFloat(0.5f)
                );
                yield return new TestCaseData(
                    int.MaxValue, Subframe.FromFloat(0.5f),
                    int.MaxValue, Subframe.FromFloat(0.5f)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestConstructorData))]
        public void TestConstructor(
            int frameNumber,
            Subframe subframe,
            int expectedFrameNumber,
            Subframe expectedSubframe
        )
        {
            Assert.AreEqual(expectedFrameNumber, frameNumber);
            Assert.AreEqual(expectedSubframe, subframe);
        }

        static IEnumerable TestFromFrameTimeData
        {
            get
            {
                yield return new TestCaseData(
                    double.MinValue,
                    int.MinValue, new Subframe()
                );
                yield return new TestCaseData(
                    int.MinValue,
                    int.MinValue, new Subframe()
                );
                yield return new TestCaseData(
                    -10.25,
                    -11, Subframe.FromDouble(0.75)
                );
                yield return new TestCaseData(
                    -10.0,
                    -10, new Subframe()
                );
                yield return new TestCaseData(
                    -0.326,
                    -1, Subframe.FromDouble(0.674)
                );
                yield return new TestCaseData(
                    0.0,
                    0, new Subframe()
                );
                yield return new TestCaseData(
                    0.326,
                    0, Subframe.FromDouble(0.326)
                );
                yield return new TestCaseData(
                    10.0,
                    10, new Subframe()
                );
                yield return new TestCaseData(
                    10.25,
                    10, Subframe.FromDouble(0.25)
                );
                yield return new TestCaseData(
                    (double)int.MaxValue,
                    int.MaxValue, new Subframe()
                );
                yield return new TestCaseData(
                    double.MaxValue,
                    int.MaxValue, new Subframe(Subframe.DefaultResolution, Subframe.DefaultResolution)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestFromFrameTimeData))]
        public void TestFromFrameTime(
            double frameTime,
            int expectedFrameNumber,
            Subframe expectedSubframe
        )
        {
            var fTime = FrameTime.FromFrameTime(frameTime);

            Assert.AreEqual(expectedFrameNumber, fTime.FrameNumber);
            Assert.AreEqual(expectedSubframe, fTime.Subframe);
        }

        static IEnumerable TestFromSecondsData
        {
            get
            {
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    double.MinValue,
                    int.MinValue, new Subframe()
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    -10403.4,
                    -623581, Subframe.FromDouble(0.58041958041959)
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -1.21,
                    -37, Subframe.FromFloat(0.7f)
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -1.0,
                    -30, new Subframe()
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -0.79,
                    -24, Subframe.FromFloat(0.3f)
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    0.0,
                    0, new Subframe()
                );
                yield return new TestCaseData(
                    new FrameRate(0),
                    10.0,
                    0, new Subframe()
                );
                yield return new TestCaseData(
                    new FrameRate(0, 0, false),
                    10.0,
                    0, new Subframe()
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    0.79,
                    23, Subframe.FromFloat(0.7f)
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    1.0,
                    30, new Subframe()
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    1.21,
                    36, Subframe.FromFloat(0.3f)
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    10403.4,
                    623580, Subframe.FromDouble(0.41958041958041)
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    double.MaxValue,
                    int.MaxValue, new Subframe(Subframe.DefaultResolution, Subframe.DefaultResolution)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestFromSecondsData))]
        public void TestFromSeconds(
            FrameRate frameRate,
            double seconds,
            int expectedFrameNumber,
            Subframe expectedSubframe
        )
        {
            var fTime = FrameTime.FromSeconds(frameRate, seconds);

            Assert.AreEqual(expectedFrameNumber, fTime.FrameNumber);
            Assert.AreEqual(expectedSubframe, fTime.Subframe);
        }

        static IEnumerable TestFloorData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameTime(int.MinValue, Subframe.FromDouble(0.0)),
                    new FrameTime(int.MinValue)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1029392.3),
                    new FrameTime(-1029393)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.5),
                    new FrameTime(-6)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1.0),
                    new FrameTime(-1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.9),
                    new FrameTime(-1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.1),
                    new FrameTime(-1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.0),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.1),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.9),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1.0),
                    new FrameTime(1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.5),
                    new FrameTime(5)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1029392.3),
                    new FrameTime(1029392)
                );
                yield return new TestCaseData(
                    new FrameTime(int.MaxValue, Subframe.FromDouble(1.0)),
                    new FrameTime(int.MaxValue)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestFloorData))]
        public void TestFloor(
            FrameTime frameTime,
            FrameTime expectedFrameTime
        )
        {
            Assert.AreEqual(expectedFrameTime, frameTime.Floor());
        }

        static IEnumerable TestCeilData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameTime(int.MinValue, Subframe.FromDouble(0.0)),
                    new FrameTime(int.MinValue)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1029392.3),
                    new FrameTime(-1029392)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.5),
                    new FrameTime(-5)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1.0),
                    new FrameTime(-1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.9),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.1),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.0),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.1),
                    new FrameTime(1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.9),
                    new FrameTime(1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1.0),
                    new FrameTime(1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.5),
                    new FrameTime(6)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1029392.3),
                    new FrameTime(1029393)
                );
                yield return new TestCaseData(
                    new FrameTime(int.MaxValue, Subframe.FromDouble(1.0)),
                    new FrameTime(int.MaxValue)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestCeilData))]
        public void TestCeil(
            FrameTime frameTime,
            FrameTime expectedFrameTime
        )
        {
            Assert.AreEqual(expectedFrameTime, frameTime.Ceil());
        }

        static IEnumerable TestRoundData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameTime(int.MinValue, Subframe.FromDouble(0.0)),
                    new FrameTime(int.MinValue)
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1029392.7),
                    new FrameTime(-1029393)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1029392.5),
                    new FrameTime(-1029393)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1029392.3),
                    new FrameTime(-1029392)
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.9),
                    new FrameTime(-6)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.5),
                    new FrameTime(-6)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.1),
                    new FrameTime(-5)
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1.0),
                    new FrameTime(-1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.9),
                    new FrameTime(-1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.1),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-0.0),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.1),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.9),
                    new FrameTime(1)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1.0),
                    new FrameTime(1)
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.1),
                    new FrameTime(5)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.5),
                    new FrameTime(5)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.9),
                    new FrameTime(6)
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1029392.3),
                    new FrameTime(1029392)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1029392.5),
                    new FrameTime(1029392)
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1029392.7),
                    new FrameTime(1029393)
                );

                yield return new TestCaseData(
                    new FrameTime(int.MaxValue, Subframe.FromDouble(1.0)),
                    new FrameTime(int.MaxValue)
                );
            }
        }

        [Test, TestCaseSource(nameof(TestRoundData))]
        public void TestRound(
            FrameTime frameTime,
            FrameTime expectedFrameTime
        )
        {
            Assert.AreEqual(expectedFrameTime, frameTime.Round());
        }

        static IEnumerable TestToSecondsData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-623581.5804, 10000),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    -10403.41936634
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-37.7, 10),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -1.2566666666666666666666
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-30.0),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -1.0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-24.3, 10),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -0.81
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    0.0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(10.0),
                    new FrameRate(0, 0, false),
                    0.0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(10.0),
                    new FrameRate(0),
                    0.0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    new FrameRate(0),
                    0.0
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(23.7, 10),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    0.79
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(30.0),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    1.0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(36.3, 10),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    1.21
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(623580.4195, 10000),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    10403.399998658333333333
                );

                // these correspond to the max representable seconds values, though the largest positive and negative values
                // are slightly different due to the largest representable subframe value
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MinValue, Subframe.MaxResolution),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -71582788.2666666666666666666666666666666666666666666666666666
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MaxValue, Subframe.MaxResolution),
                    StandardFrameRate.FPS_30_00.ToValue(),
                    71582788.2666661580403645833333333333333333333333333333333333
                );
            }
        }

        [Test, TestCaseSource(nameof(TestToSecondsData))]
        public void TestToSeconds(
            FrameTime frameTime,
            FrameRate frameRate,
            double expectedSeconds
        )
        {
            Assert.AreEqual(expectedSeconds, frameTime.ToSeconds(frameRate), 0.00000000000001);
        }

        static IEnumerable TestMaxRepresentableSecondsData
        {
            get
            {
                yield return new TestCaseData(
                    StandardFrameRate.FPS_23_976.ToValue(),
                    89567963.8186660302480061848958333333333333333333333333333333
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    89478485.3333326975504557291666666666666666666666666666666666
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    71582788.2666661580403645833333333333333333333333333333333333
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    35827185.5274664120992024739583333333333333333333333333333333
                );
            }
        }

        [Test, TestCaseSource(nameof(TestMaxRepresentableSecondsData))]
        public void TestMaxRepresentableSeconds(
            FrameRate frameRate,
            double expectedMaxSeconds
        )
        {
            Assert.AreEqual(expectedMaxSeconds, FrameTime.MaxRepresentableSeconds(frameRate), 0.0000001);
        }

        static IEnumerable TestCompareToData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-25.65),
                    FrameTime.FromFrameTime(-25.60),
                    -1
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-25.70),
                    FrameTime.FromFrameTime(-25.70),
                    0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-25.65),
                    FrameTime.FromFrameTime(-25.70),
                    1
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1.0),
                    FrameTime.FromFrameTime(-1.0),
                    0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(0.0),
                    0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1.0),
                    FrameTime.FromFrameTime(1.0),
                    0
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(25.65),
                    FrameTime.FromFrameTime(25.70),
                    -1
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(25.70),
                    FrameTime.FromFrameTime(25.70),
                    0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(25.65),
                    FrameTime.FromFrameTime(25.60),
                    1
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MinValue),
                    FrameTime.FromFrameTime(double.MaxValue),
                    -1
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MinValue),
                    FrameTime.FromFrameTime(double.MinValue),
                    0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MaxValue),
                    FrameTime.FromFrameTime(double.MaxValue),
                    0
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MaxValue),
                    FrameTime.FromFrameTime(double.MinValue),
                    1
                );
            }
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareTo(
            FrameTime a,
            FrameTime b,
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
            FrameTime a,
            FrameTime b,
            int expectedValue
        )
        {
            Assert.AreEqual(expectedValue == 0, a.Equals(b));
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareOperators(
            FrameTime a,
            FrameTime b,
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

        static IEnumerable TestIncrementData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MinValue),
                    FrameTime.FromFrameTime(int.MinValue + 1),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-26.65),
                    FrameTime.FromFrameTime(-25.65),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1.0),
                    FrameTime.FromFrameTime(0.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(1.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1.0),
                    FrameTime.FromFrameTime(2.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(25.65),
                    FrameTime.FromFrameTime(26.65),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MaxValue),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestIncrementData))]
        public void TestIncrement(
            FrameTime a,
            FrameTime expectedValue,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<OverflowException>(() => a++);
            }
            else
            {
                Assert.AreEqual(expectedValue, ++a);
            }
        }

        static IEnumerable TestDecrementData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MinValue),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-25.65),
                    FrameTime.FromFrameTime(-26.65),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-1.0),
                    FrameTime.FromFrameTime(-2.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(-1.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1.0),
                    FrameTime.FromFrameTime(0.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(26.65),
                    FrameTime.FromFrameTime(25.65),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MaxValue),
                    FrameTime.FromFrameTime(int.MaxValue - 1 + (double)Subframe.FromDouble(1.0, Subframe.MaxResolution)),
                    false
                );
            }
        }

        [Test, TestCaseSource(nameof(TestDecrementData))]
        public void TestDecrement(
            FrameTime a,
            FrameTime expectedValue,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<OverflowException>(() => a--);
            }
            else
            {
                Assert.AreEqual(expectedValue, --a);
            }
        }

        static IEnumerable TestAddData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(0.0),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.16),
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(5.16),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(1.24),
                    FrameTime.FromFrameTime(1.24),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.39),
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(-5.39),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(-5.39),
                    FrameTime.FromFrameTime(-5.39),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(1024332.23),
                    FrameTime.FromFrameTime(29584837.81),
                    FrameTime.FromFrameTime(30609170.04),
                    false
                );

                // test that we get exact results when using different subframe resolutions that are multiples of one another
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(12.35, 200),
                    FrameTime.FromFrameTime(25.66, 800),
                    FrameTime.FromFrameTime(38.01, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-12.35, 200),
                    FrameTime.FromFrameTime(-25.66, 800),
                    FrameTime.FromFrameTime(-38.01, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-12.35, 200),
                    FrameTime.FromFrameTime(25.66, 800),
                    FrameTime.FromFrameTime(13.31, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(12.35, 200),
                    FrameTime.FromFrameTime(-25.66, 800),
                    FrameTime.FromFrameTime(-13.31, 800),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(12.35, 800),
                    FrameTime.FromFrameTime(25.66, 200),
                    FrameTime.FromFrameTime(38.01, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-12.35, 800),
                    FrameTime.FromFrameTime(-25.66, 200),
                    FrameTime.FromFrameTime(-38.01, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-12.35, 800),
                    FrameTime.FromFrameTime(25.66, 200),
                    FrameTime.FromFrameTime(13.31, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(12.35, 800),
                    FrameTime.FromFrameTime(-25.66, 200),
                    FrameTime.FromFrameTime(-13.31, 800),
                    false
                );

                // test adding when subframe resolutions are not multiples of one another
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(12.35, 41900),
                    FrameTime.FromFrameTime(25.66, 42100),
                    FrameTime.FromFrameTime(38.01, 42100),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-12.35, 41900),
                    FrameTime.FromFrameTime(-25.66, 42100),
                    FrameTime.FromFrameTime(-38.01, 42100),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-12.35, 41900),
                    FrameTime.FromFrameTime(25.66, 42100),
                    FrameTime.FromFrameTime(13.31, 42100),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(12.35, 41900),
                    FrameTime.FromFrameTime(-25.66, 42100),
                    FrameTime.FromFrameTime(-13.31, 42100),
                    false
                );

                // test overflow
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(int.MaxValue / 2.0 + 0.4),
                    FrameTime.FromFrameTime(int.MaxValue / 2.0 + 0.4),
                    FrameTime.FromFrameTime(int.MaxValue + 0.8),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(int.MaxValue / 2.0 + 0.5),
                    FrameTime.FromFrameTime(int.MaxValue / 2.0 + 0.5),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(int.MaxValue),
                    FrameTime.FromFrameTime(int.MaxValue),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestAddData))]
        public void TestAdd(
            FrameTime a,
            FrameTime b,
            FrameTime expectedValue,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<OverflowException>(() => a += b);
            }
            else
            {
                Assert.AreEqual(expectedValue, a + b);
            }
        }

        static IEnumerable TestSubtractData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(0.0),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(5.16),
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(5.16),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(1.24),
                    FrameTime.FromFrameTime(-1.24),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-5.39),
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(-5.39),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    FrameTime.FromFrameTime(-5.39),
                    FrameTime.FromFrameTime(5.39),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(30609170.04),
                    FrameTime.FromFrameTime(1024332.23),
                    FrameTime.FromFrameTime(29584837.81),
                    false
                );

                // test that we get exact results when using different subframe resolutions that are multiples of one another
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(38.01, 800),
                    FrameTime.FromFrameTime(12.35, 200),
                    FrameTime.FromFrameTime(25.66, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-38.01, 800),
                    FrameTime.FromFrameTime(-12.35, 200),
                    FrameTime.FromFrameTime(-25.66, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(13.31, 800),
                    FrameTime.FromFrameTime(-12.35, 200),
                    FrameTime.FromFrameTime(25.66, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-13.31, 800),
                    FrameTime.FromFrameTime(12.35, 200),
                    FrameTime.FromFrameTime(-25.66, 800),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(38.01, 800),
                    FrameTime.FromFrameTime(25.66, 200),
                    FrameTime.FromFrameTime(12.35, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-38.01, 800),
                    FrameTime.FromFrameTime(-25.66, 200),
                    FrameTime.FromFrameTime(-12.35, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(13.31, 800),
                    FrameTime.FromFrameTime(25.66, 200),
                    FrameTime.FromFrameTime(-12.35, 800),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-13.31, 800),
                    FrameTime.FromFrameTime(-25.66, 200),
                    FrameTime.FromFrameTime(12.35, 800),
                    false
                );

                // test adding when subframe resolutions are not multiples of one another
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(38.01, 42100),
                    FrameTime.FromFrameTime(12.35, 41900),
                    FrameTime.FromFrameTime(25.66, 42100),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-38.01, 42100),
                    FrameTime.FromFrameTime(-12.35, 41900),
                    FrameTime.FromFrameTime(-25.66, 42100),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(13.31, 42100),
                    FrameTime.FromFrameTime(-12.35, 41900),
                    FrameTime.FromFrameTime(25.66, 42100),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-13.31, 42100),
                    FrameTime.FromFrameTime(12.35, 41900),
                    FrameTime.FromFrameTime(-25.66, 42100),
                    false
                );

                // test overflow
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(int.MinValue / 2.0),
                    FrameTime.FromFrameTime(int.MaxValue / 2.0 + 0.5),
                    FrameTime.FromFrameTime(int.MinValue),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(int.MinValue / 2.0 - 0.6),
                    FrameTime.FromFrameTime(int.MaxValue / 2.0 + 0.6),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(int.MinValue),
                    FrameTime.FromFrameTime(int.MaxValue),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestSubtractData))]
        public void TestSubtract(
            FrameTime a,
            FrameTime b,
            FrameTime expectedValue,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<OverflowException>(() => a -= b);
            }
            else
            {
                Assert.AreEqual(expectedValue, a - b);
            }
        }

        static IEnumerable TestRemapData
        {
            get
            {
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    new FrameRate(0, 1, false),
                    new FrameRate(1, 1, false),
                    FrameTime.FromFrameTime(0.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(0.0),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_48_00.ToValue(),
                    FrameTime.FromFrameTime(0.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(10.5),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    FrameTime.FromFrameTime(10.5),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(10.5),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_48_00.ToValue(),
                    FrameTime.FromFrameTime(21.0),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(21.0),
                    StandardFrameRate.FPS_48_00.ToValue(),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    FrameTime.FromFrameTime(10.5),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(194323.84),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    FrameTime.FromFrameTime(77807.265536),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(77807.26),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    FrameTime.FromFrameTime(194323.826174),
                    false
                );

                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-194323.84),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    FrameTime.FromFrameTime(-77807.265536),
                    false
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(-77807.26),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    FrameTime.FromFrameTime(-194323.826174),
                    false
                );

                // test overflow
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MaxValue),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
                yield return new TestCaseData(
                    FrameTime.FromFrameTime(double.MinValue),
                    StandardFrameRate.FPS_24_00.ToValue(),
                    StandardFrameRate.FPS_59_94_DF.ToValue(),
                    FrameTime.FromFrameTime(0.0),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestRemapData))]
        public void TestRemap(
            FrameTime frameTime,
            FrameRate srcRate,
            FrameRate dstRate,
            FrameTime expectedValue,
            bool throwsException
        )
        {
            if (throwsException)
            {
                Assert.Throws<OverflowException>(() => FrameTime.Remap(frameTime, srcRate, dstRate));
            }
            else
            {
                Assert.AreEqual(expectedValue, FrameTime.Remap(frameTime, srcRate, dstRate));
            }
        }
    }
}
