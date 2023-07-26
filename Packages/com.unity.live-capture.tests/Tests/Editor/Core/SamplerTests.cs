using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.Tests.Editor
{
    public class SamplerTests
    {
        FloatSampler m_Sampler;

        [SetUp]
        public void Setup()
        {
            m_Sampler = new FloatSampler();
        }

        static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(0.25f, 0f),
                        new Keyframe(0.5f, 0f),
                        new Keyframe(0.75f, 0.75f),
                        new Keyframe(1.25f, 1.25f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(1f, 1f)
                    }).SetName("Sample rate is lower than input sample rate.");
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(1.25f, 1.25f),
                        new Keyframe(2.5f, 2.5f),
                        new Keyframe(3.75f, 3.75f),
                        new Keyframe(5f, 5f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(1f, 1f),
                        new Keyframe(2f, 2f),
                        new Keyframe(3f, 3f),
                        new Keyframe(4f, 4f),
                        new Keyframe(5f, 5f)
                    }).SetName("Sample rate is greater than input sample rate.");
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(0.95f, 0f),
                        new Keyframe(1.5f, 0f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(0.95f, 0f),
                        new Keyframe(1f, 0f)
                    }).SetName("Initial time is close to frame time");
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(1f, 0f),
                        new Keyframe(1.5f, 0f),
                        new Keyframe(2.5f, 0f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(1f, 0f),
                        new Keyframe(2f, 0f)
                    }).SetName("Initial time is equal to frame time");
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(1f, 1f),
                        new Keyframe(2.5f, 2.5f),
                        new Keyframe(1.5f, 1.5f),
                        new Keyframe(3.5f, 3.5f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(1f, 1f),
                        new Keyframe(2f, 2f),
                        new Keyframe(3f, 3f)
                    }).SetName("Unordered samples are discarted");
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(0.5f, 0f),
                        new Keyframe(1.25f, 0f),
                        new Keyframe(6.5f, 0f),
                        new Keyframe(7.25f, 0f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(1f, 0f),
                        new Keyframe(1.25f, 0f),
                        new Keyframe(5f, 0f),
                        new Keyframe(6.5f, 0f),
                        new Keyframe(7f, 0f)
                    }).SetName("Samples with max frame distance");
            }
        }

        static IEnumerable FlushCases
        {
            get
            {
                yield return new TestCaseData(
                    new FrameRate(1),
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(0.5f, 0f),
                        new Keyframe(1.25f, 0f)
                    },
                    new Keyframe[]
                    {
                        new Keyframe(0f, 0f),
                        new Keyframe(0.5f, 0f),
                        new Keyframe(1.25f, 0f)
                    }).SetName("Flush samples");
            }
        }

        IEnumerable<T> ToEnumerable<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        [Test, TestCaseSource("TestCases")]
        public void SequenceTest(
            FrameRate frameRate,
            Keyframe[] input,
            Keyframe[] output)
        {
            m_Sampler.FrameRate = frameRate;

            AddSamples(input);
            AssertSequence(output);
        }

        [Test, TestCaseSource("FlushCases")]
        public void FlushTest(
            FrameRate frameRate,
            Keyframe[] input,
            Keyframe[] output)
        {
            m_Sampler.FrameRate = frameRate;

            AddSamples(input);

            m_Sampler.Flush();

            AssertSequence(output);
        }

        void AddSamples(Keyframe[] input)
        {
            foreach (var keyframe in input)
            {
                m_Sampler.Add(keyframe.time, keyframe.value);
            }
        }

        void AssertSequence(Keyframe[] output)
        {
            var comparer = new FloatEqualityComparer(0.001f);
            var samplerOutput = ToEnumerable(m_Sampler).ToArray();

            for (var i = 0; i < Mathf.Min(output.Length, samplerOutput.Length); ++i)
            {
                var expectedTime = output[i].time;
                var samplerTime = samplerOutput[i].Time;

                Assert.That(samplerTime, Is.EqualTo(expectedTime).Using(comparer),
                    $"Unexpected time at sample {i}: expected {expectedTime} but was {samplerTime}");

                var expectedValue = output[i].value;
                var samplerValue = samplerOutput[i].Value;

                Assert.That(samplerValue, Is.EqualTo(expectedValue).Using(comparer),
                    $"Unexpected value at sample {i}: expected {expectedValue} but was {samplerValue}");
            }

            Assert.AreEqual(output.Length, samplerOutput.Length, "Incorrect number of keyframes.");
        }
    }
}
