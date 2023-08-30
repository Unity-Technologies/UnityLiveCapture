using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class TimedDataBufferTests
    {
        static IEnumerable TestRangeData
        {
            get
            {
                yield return new TestCaseData(
                    new FrameTime(0),
                    new FrameTime(5),
                    new string[]
                    {
                    }
                );
                yield return new TestCaseData(
                    new FrameTime(8),
                    new FrameTime(12),
                    new string[]
                    {
                        "A",
                        "B",
                        "C",
                    }
                );
                yield return new TestCaseData(
                    new FrameTime(10),
                    new FrameTime(14),
                    new string[]
                    {
                        "A",
                        "B",
                        "C",
                        "D",
                        "E",
                    }
                );
                yield return new TestCaseData(
                    new FrameTime(11),
                    new FrameTime(13),
                    new string[]
                    {
                        "B",
                        "C",
                        "D",
                    }
                );
                yield return new TestCaseData(
                    new FrameTime(13),
                    new FrameTime(18),
                    new string[]
                    {
                        "D",
                        "E",
                    }
                );
                yield return new TestCaseData(
                    new FrameTime(20),
                    new FrameTime(50),
                    new string[]
                    {
                    }
                );
            }
        }

        [Test, TestCaseSource(nameof(TestRangeData))]
        public void TestRangeBefore(FrameTime from, FrameTime to, string[] expectedResults)
        {
            var time = new FrameTimeWithRate(StandardFrameRate.FPS_30_00, new FrameTime(10));

            var buffer = new TimedDataBuffer<string>(StandardFrameRate.FPS_30_00, 5)
            {
                { "A", time++ },
                { "B", time++ },
                { "C", time++ },
                { "D", time++ },
                { "E", time++ },
            };

            var actualResults = buffer.GetSamplesInRange(from, to).Select(s => s.value).ToList();

            Assert.AreEqual(expectedResults.Length, actualResults.Count);

            for (var i = 0; i < expectedResults.Length; i++)
            {
                Assert.AreEqual(expectedResults[i], actualResults[i]);
            }
        }

        [Test]
        public void TestTryGetSample()
        {
            var time = new FrameTimeWithRate(StandardFrameRate.FPS_30_00, new FrameTime(0));

            var buffer = new TimedDataBuffer<float>(StandardFrameRate.FPS_30_00, 5)
            {
                { 1f, time++ },
                { 3f, time++ },
                { 4f, time++ },
                { 7f, time++ },
            };

            float value;

            Assert.AreEqual(TimedSampleStatus.Ok, buffer.TryGetSample(FrameTime.FromFrameTime(0.4), out value));
            Assert.AreEqual(1f, value, 0.0001);

            Assert.AreEqual(TimedSampleStatus.Ok, buffer.TryGetSample(FrameTime.FromFrameTime(1.25), out value));
            Assert.AreEqual(3f, value, 0.0001);

            Assert.AreEqual(TimedSampleStatus.Ok, buffer.TryGetSample(FrameTime.FromFrameTime(8.0 / 3.0), out value));
            Assert.AreEqual(7f, value, 0.0001);
        }

        class TestInterpolator : IInterpolator<float>
        {
            public static TestInterpolator Instance { get; } = new TestInterpolator();

            /// <inheritdoc />
            public float Interpolate(in float a, in float b, float t)
            {
                return Mathf.LerpUnclamped(a, b, t);
            }
        }

        [Test]
        public void TestTryGetSampleInterpolated()
        {
            var time = new FrameTimeWithRate(StandardFrameRate.FPS_30_00, new FrameTime(0));

            var buffer = new TimedDataBuffer<float>(StandardFrameRate.FPS_30_00, 5)
            {
                { 1f, time++ },
                { 3f, time++ },
                { 4f, time++ },
                { 7f, time++ },
            };

            buffer.Interpolator = TestInterpolator.Instance;
            float value;

            Assert.AreEqual(TimedSampleStatus.Ok, buffer.TryGetSample(FrameTime.FromFrameTime(0.5), out value));
            Assert.AreEqual(2f, value, 0.0001);

            Assert.AreEqual(TimedSampleStatus.Ok, buffer.TryGetSample(FrameTime.FromFrameTime(1.25), out value));
            Assert.AreEqual(3.25f, value, 0.0001);

            Assert.AreEqual(TimedSampleStatus.Ok, buffer.TryGetSample(FrameTime.FromFrameTime(7.0 / 3.0), out value));
            Assert.AreEqual(5f, value, 0.0001);
        }
    }
}
