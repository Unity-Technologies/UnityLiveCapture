using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class TimecodeTests
    {
        static IEnumerable TestFromHMSFData
        {
            get
            {
                // invalid frame rate returns default
                yield return new TestCaseData(
                    default(FrameRate),
                    3, 2, 90, 45, Subframe.FromDouble(0.3),
                    0, 0, 0, 0, new Subframe(),
                    false,
                    false
                );

                // test simple cases
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    0, 0, 0, 0, new Subframe(),
                    0, 0, 0, 0, new Subframe(),
                    false,
                    false
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    24, 0, 0, 0, new Subframe(),
                    0, 0, 0, 0, new Subframe(),
                    false,
                    false
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -24, 0, 0, 0, new Subframe(),
                    0, 0, 0, 0, new Subframe(),
                    false,
                    false
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    48, 2, 90, 45, Subframe.FromDouble(0.3),
                    0, 3, 31, 15, Subframe.FromDouble(0.3),
                    false,
                    false
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    -48, -2, -90, -45, Subframe.FromDouble(0.3),
                    0, -3, -31, -15, Subframe.FromDouble(0.3),
                    false,
                    false
                );

                // test non-drop frame
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97.ToValue(),
                    0, 1, 0, 0, Subframe.FromDouble(0.6),
                    0, 1, 0, 0, Subframe.FromDouble(0.6),
                    false,
                    false
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97.ToValue(),
                    24, 0, 0, 0, Subframe.FromDouble(0.6),
                    0, 0, 0, 0, Subframe.FromDouble(0.6),
                    false,
                    false
                );

                // test drop frame without conversion
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 0, 0, 1, Subframe.FromDouble(0.6),
                    0, 0, 0, 1, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 0, 59, 29, Subframe.FromDouble(0.6),
                    0, 0, 59, 29, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 1, 0, 0, Subframe.FromDouble(0.6),
                    0, 1, 0, 0, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 1, 59, 27, Subframe.FromDouble(0.6),
                    0, 1, 59, 27, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 1, 59, 28, Subframe.FromDouble(0.6),
                    0, 1, 59, 28, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 9, 59, 12, Subframe.FromDouble(0.6),
                    0, 9, 59, 12, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 9, 59, 13, Subframe.FromDouble(0.6),
                    0, 9, 59, 13, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 10, 0, 0, Subframe.FromDouble(0.6),
                    0, 10, 0, 0, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    20, 17, 18, 20, Subframe.FromDouble(0.6),
                    20, 17, 18, 20, Subframe.FromDouble(0.6),
                    true,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    24, 0, 0, 0, Subframe.FromDouble(0.6),
                    0, 0, 0, 0, Subframe.FromDouble(0.6),
                    true,
                    true
                );

                // test drop frame with conversion
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 0, 0, 1, Subframe.FromDouble(0.6),
                    0, 0, 0, 1, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 0, 59, 29, Subframe.FromDouble(0.6),
                    0, 0, 59, 29, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 1, 0, 0, Subframe.FromDouble(0.6),
                    0, 1, 0, 2, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 1, 59, 27, Subframe.FromDouble(0.6),
                    0, 1, 59, 29, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 1, 59, 28, Subframe.FromDouble(0.6),
                    0, 2, 0, 2, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 9, 59, 12, Subframe.FromDouble(0.6),
                    0, 10, 0, 0, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 9, 59, 13, Subframe.FromDouble(0.6),
                    0, 10, 0, 1, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    0, 10, 0, 0, Subframe.FromDouble(0.6),
                    0, 10, 0, 18, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    20, 17, 18, 20, Subframe.FromDouble(0.6),
                    20, 18, 31, 24, Subframe.FromDouble(0.6),
                    false,
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    24, 0, 0, 0, Subframe.FromDouble(0.6),
                    0, 1, 26, 14, Subframe.FromDouble(0.6),
                    false,
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestFromHMSFData))]
        public void TestFromHMSF(
            FrameRate frameRate,
            int hours,
            int minutes,
            int seconds,
            int frames,
            Subframe subframe,
            int expectedHours,
            int expectedMinutes,
            int expectedSeconds,
            int expectedFrames,
            Subframe expectedSubframe,
            bool isDropFrame,
            bool expectedIsDropFrame
        )
        {
            var timecode = Timecode.FromHMSF(frameRate, hours, minutes, seconds, frames, subframe, isDropFrame);

            Assert.AreEqual(expectedHours, timecode.Hours);
            Assert.AreEqual(expectedMinutes, timecode.Minutes);
            Assert.AreEqual(expectedSeconds, timecode.Seconds);
            Assert.AreEqual(expectedFrames, timecode.Frames);
            Assert.AreEqual(expectedSubframe, timecode.Subframe);
            Assert.AreEqual(expectedIsDropFrame, timecode.IsDropFrame);
        }

        static IEnumerable TestFromFrameTimeData
        {
            get
            {
                // invalid frame rate returns default
                yield return new TestCaseData(
                    default(FrameRate),
                    new FrameTime(0),
                    0, 0, 0, 0, new Subframe(),
                    false
                );

                yield return new TestCaseData(
                    new FrameRate(0, 1, false),
                    new FrameTime(100),
                    0, 0, 0, 0, new Subframe(),
                    false
                );

                // test simple cases
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    new FrameTime(0),
                    0, 0, 0, 0, new Subframe(),
                    false
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    new FrameTime(24 * 60 * 60 * 30),
                    0, 0, 0, 0, new Subframe(),
                    false
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    new FrameTime(-24 * 60 * 60 * 30),
                    0, 0, 0, 0, new Subframe(),
                    false
                );

                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    new FrameTime(5190345, Subframe.FromDouble(0.3)),
                    0, 3, 31, 15, Subframe.FromDouble(0.3),
                    false
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_30_00.ToValue(),
                    new FrameTime(-5190345, Subframe.FromDouble(0.3)),
                    0, -3, -31, -15, Subframe.FromDouble(0.3),
                    false
                );

                // test non-drop frame
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97.ToValue(),
                    new FrameTime(60 * 30, Subframe.FromDouble(0.6)),
                    0, 1, 0, 0, Subframe.FromDouble(0.6),
                    false
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97.ToValue(),
                    new FrameTime(24 * 60 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 0, 0, 0, Subframe.FromDouble(0.6),
                    false
                );

                // test drop frame with conversion
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1, Subframe.FromDouble(0.6)),
                    0, 0, 0, 1, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(59 * 30 + 29, Subframe.FromDouble(0.6)),
                    0, 0, 59, 29, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 1, 0, 2, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1 * 60 * 30 + 59 * 30 + 27, Subframe.FromDouble(0.6)),
                    0, 1, 59, 29, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1 * 60 * 30 + 59 * 30 + 28, Subframe.FromDouble(0.6)),
                    0, 2, 0, 2, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(9 * 60 * 30 + 59 * 30 + 12, Subframe.FromDouble(0.6)),
                    0, 10, 0, 0, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(9 * 60 * 30 + 59 * 30 + 13, Subframe.FromDouble(0.6)),
                    0, 10, 0, 1, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(10 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 10, 0, 18, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(20 * 60 * 60 * 30 + 17 * 60 * 30 + 18 * 30 + 20, Subframe.FromDouble(0.6)),
                    20, 18, 31, 24, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(24 * 60 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 1, 26, 14, Subframe.FromDouble(0.6),
                    true
                );
                // test drop frame with conversion
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1, Subframe.FromDouble(0.6)),
                    0, 0, 0, 1, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(59 * 30 + 29, Subframe.FromDouble(0.6)),
                    0, 0, 59, 29, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 1, 0, 2, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1 * 60 * 30 + 59 * 30 + 27, Subframe.FromDouble(0.6)),
                    0, 1, 59, 29, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(1 * 60 * 30 + 59 * 30 + 28, Subframe.FromDouble(0.6)),
                    0, 2, 0, 2, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(9 * 60 * 30 + 59 * 30 + 12, Subframe.FromDouble(0.6)),
                    0, 10, 0, 0, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(9 * 60 * 30 + 59 * 30 + 13, Subframe.FromDouble(0.6)),
                    0, 10, 0, 1, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(10 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 10, 0, 18, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(20 * 60 * 60 * 30 + 17 * 60 * 30 + 18 * 30 + 20, Subframe.FromDouble(0.6)),
                    20, 18, 31, 24, Subframe.FromDouble(0.6),
                    true
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_29_97_DF.ToValue(),
                    new FrameTime(24 * 60 * 60 * 30, Subframe.FromDouble(0.6)),
                    0, 1, 26, 14, Subframe.FromDouble(0.6),
                    true
                );
            }
        }

        [Test, TestCaseSource(nameof(TestFromFrameTimeData))]
        public void TestFromFrameTime(
            FrameRate frameRate,
            FrameTime frameTime,
            int expectedHours,
            int expectedMinutes,
            int expectedSeconds,
            int expectedFrames,
            Subframe expectedSubframe,
            bool expectedIsDropFrame
        )
        {
            var timecode = Timecode.FromFrameTime(frameRate, frameTime);

            Assert.AreEqual(expectedHours, timecode.Hours);
            Assert.AreEqual(expectedMinutes, timecode.Minutes);
            Assert.AreEqual(expectedSeconds, timecode.Seconds);
            Assert.AreEqual(expectedFrames, timecode.Frames);
            Assert.AreEqual(expectedSubframe, timecode.Subframe);
            Assert.AreEqual(expectedIsDropFrame, timecode.IsDropFrame);
        }

        static IEnumerable TestFloorData
        {
            get
            {
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.0))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.0)))
                );
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.6))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.0)))
                );
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(12, Subframe.FromDouble(0.3, 500))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(12, Subframe.FromDouble(0.0, 500)))
                );
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(-10, Subframe.FromDouble(0.99, 100))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(-10, Subframe.FromDouble(0.0, 100)))
                );
            }
        }

        [Test, TestCaseSource(nameof(TestFloorData))]
        public void TestFloor(
            Timecode timecode,
            Timecode expectedTimecode
        )
        {
            Assert.AreEqual(expectedTimecode, timecode.Floor());
            Assert.AreEqual(expectedTimecode.Subframe.Resolution, timecode.Subframe.Resolution);
        }

        static IEnumerable TestCenterData
        {
            get
            {
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.0))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.5)))
                );
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.6))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0, Subframe.FromDouble(0.5)))
                );
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(12, Subframe.FromDouble(0.3, 500))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(12, Subframe.FromDouble(0.5, 500)))
                );
                yield return new TestCaseData(
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(-10, Subframe.FromDouble(0.99, 100))),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(-10, Subframe.FromDouble(0.5, 100)))
                );
            }
        }

        [Test, TestCaseSource(nameof(TestCenterData))]
        public void TestCenter(
            Timecode timecode,
            Timecode expectedTimecode
        )
        {
            Assert.AreEqual(expectedTimecode, timecode.Center());
            Assert.AreEqual(expectedTimecode.Subframe.Resolution, timecode.Subframe.Resolution);
        }

        static IEnumerable TestToSecondsData
        {
            get
            {
                yield return new TestCaseData(
                    default(FrameRate),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 10.0),
                    0.0
                );
                yield return new TestCaseData(
                    new FrameRate(0, 1, false),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 10.0),
                    0.0
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 0.0),
                    0.0
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_23_976_DF.ToValue(),
                    Timecode.FromSeconds(StandardFrameRate.FPS_23_976_DF, 29430.140),
                    29430.140
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_23_976_DF.ToValue(),
                    Timecode.FromSeconds(StandardFrameRate.FPS_23_976_DF, -29430.140),
                    -29430.140
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 24 * 60 * 60 * 24 - (1.5 / 24)),
                    86399.9375
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -(24 * 60 * 60 * 24 - (1.5 / 24))),
                    -86399.9375
                );
            }
        }

        [Test, TestCaseSource(nameof(TestToSecondsData))]
        public void TestToSeconds(
            FrameRate frameRate,
            Timecode timecode,
            double expectedSeconds
        )
        {
            Assert.AreEqual(expectedSeconds, timecode.ToSeconds(frameRate), 0.000001);
        }

        static IEnumerable TestToFrameTimeData
        {
            get
            {
                yield return new TestCaseData(
                    default(FrameRate),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(10)),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(0)),
                    new FrameTime(0)
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_23_976_DF.ToValue(),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_23_976_DF, new FrameTime(1230392, Subframe.FromDouble(0.2521))),
                    new FrameTime(1230392, Subframe.FromDouble(0.2521))
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_23_976_DF.ToValue(),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_23_976_DF, new FrameTime(-1230392, Subframe.FromDouble(0.2521))),
                    new FrameTime(-1230392, Subframe.FromDouble(0.2521))
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(24 * 60 * 60 * 24 - 1, Subframe.FromDouble(0.5))),
                    new FrameTime(24 * 60 * 60 * 24 - 1, Subframe.FromDouble(0.5))
                );
                yield return new TestCaseData(
                    StandardFrameRate.FPS_24_00.ToValue(),
                    Timecode.FromFrameTime(StandardFrameRate.FPS_24_00, new FrameTime(-(24 * 60 * 60 * 24 - 1), Subframe.FromDouble(0.5))),
                    new FrameTime(-(24 * 60 * 60 * 24 - 1), Subframe.FromDouble(0.5))
                );
            }
        }

        [Test, TestCaseSource(nameof(TestToFrameTimeData))]
        public void TestToFrameTime(
            FrameRate frameRate,
            Timecode timecode,
            FrameTime expectedFrameTime
        )
        {
            Assert.AreEqual(expectedFrameTime, timecode.ToFrameTime(frameRate));
        }

        static IEnumerable TestCompareToData
        {
            get
            {
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -1.0),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -1.0),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 0.0),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 0.0),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 1.0),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 1.0),
                    0
                );

                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 19.4321),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 20.4572),
                    -1
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 20.312),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 20.312),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 21.5123),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, 20.312),
                    1
                );

                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -20.4572),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -19.4321),
                    -1
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -20.312),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -20.312),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -20.312),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, -21.5123),
                    1
                );

                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, 100.012567),
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, 100.012568),
                    -1
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, 100.012567),
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, 100.012567),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, 100.012568),
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, 100.012567),
                    1
                );

                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, -100.012568),
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, -100.012567),
                    -1
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, -100.012567),
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, -100.012567),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, -100.012567),
                    Timecode.FromSeconds(StandardFrameRate.FPS_59_94_DF, -100.012568),
                    1
                );

                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MinValue),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MaxValue),
                    -1
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MinValue),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MinValue),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MaxValue),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MaxValue),
                    0
                );
                yield return new TestCaseData(
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MaxValue),
                    Timecode.FromSeconds(StandardFrameRate.FPS_24_00, double.MinValue),
                    1
                );
            }
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareTo(
            Timecode a,
            Timecode b,
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
            Timecode a,
            Timecode b,
            int expectedValue
        )
        {
            Assert.AreEqual(expectedValue == 0, a.Equals(b));
        }

        [Test, TestCaseSource(nameof(TestCompareToData))]
        public void TestCompareOperators(
            Timecode a,
            Timecode b,
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
