using System;
using System.Linq;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.LiveCapture.Tests.Editor
{
    public class SamplerProcessorTests
    {
        Lens m_Lens = Lens.DefaultParams;
        Settings m_Settings = Settings.DefaultData;
        SampleProcessor m_Processor;
        static readonly FrameRate k_SamplingFrameRate = StandardFrameRate.FPS_60_00;

        [SetUp]
        public void Setup()
        {
            m_Processor = new SampleProcessor
            {
                GetLensTarget = () => m_Lens,
                GetSettings = () => m_Settings,
                TimeShiftTolerance = int.MaxValue
            };
        }

        FrameTime ToFrameTime(double time)
        {
            return FrameTime.FromSeconds(m_Processor.GetBufferFrameRate(), time);
        }

        [Test]
        public void GeneratesProperlyTimedFrames()
        {
            const double tolerance = 0.001;
            m_Lens = new Lens();
            m_Processor.Reset(ToFrameTime(1.0));

            m_Processor.AddLensKeyframe(1.0, new Lens{Aperture = 2f});
            var processedLenses = m_Processor.ProcessTo(ToFrameTime(2.0)).ToArray();
            // samples for t = [0, 1.0) are skipped
            Assert.That(processedLenses, Has.Length.EqualTo(61));

            var firstSample = processedLenses.First();
            Assert.That(firstSample.time, Is.EqualTo(1.0).Within(tolerance));
            Assert.That(firstSample.lens.Value.Aperture, Is.EqualTo(2f).Within(tolerance));

            var lastSample = processedLenses.Last();
            Assert.That(lastSample.time, Is.EqualTo(2.0).Within(tolerance));
            Assert.That(lastSample.lens.Value.Aperture, Is.EqualTo(2f).Within(tolerance));
        }


        [Test]
        public void InterpolatedValueConvergesToTarget()
        {
            const double tolerance = 0.01;
            m_Settings.FocalLengthDamping = .18f;
            m_Processor.AddFocalLengthKeyframe(1.0, 2f);
            m_Processor.AddFocalLengthKeyframe(2.0, 4f);
            m_Processor.AddFocalLengthKeyframe(3.0, 12f);

            var sampledLenses1 = m_Processor
                .ProcessTo(ToFrameTime(1.8))
                .ToArray();

            Assert.That(sampledLenses1.Last().lens.Value.FocalLength, Is.EqualTo(2f).Within(tolerance));
            Assert.That(sampledLenses1.Last().time, Is.EqualTo(1.8).Within(k_SamplingFrameRate.FrameInterval));

            var sampledLenses2 = m_Processor
                .ProcessTo(ToFrameTime(2.8))
                .ToArray();

            Assert.That(sampledLenses2.First().lens.Value.FocalLength, Is.EqualTo(2f).Within(tolerance));
            Assert.That(sampledLenses2.Last().lens.Value.FocalLength, Is.EqualTo(4f).Within(tolerance));
            Assert.That(sampledLenses2.Last().time, Is.EqualTo(2.8).Within(k_SamplingFrameRate.FrameInterval));


            var sampledLenses3 = m_Processor
                .ProcessTo(ToFrameTime(4.5))
                .ToArray();

            Assert.That(sampledLenses3.First().lens.Value.FocalLength, Is.EqualTo(4f).Within(tolerance));
            Assert.That(sampledLenses3.Last().lens.Value.FocalLength, Is.EqualTo(12f).Within(tolerance));
            Assert.That(sampledLenses3.Last().time, Is.EqualTo(4.5).Within(k_SamplingFrameRate.FrameInterval));
        }

        [Test]
        public void UpdateFocusWhileProcessing()
        {
            m_Settings.FocusDistanceDamping = .18f;

            m_Processor.AddFocusDistanceKeyframe(0d, 3f);

            var sampledLenses1 = m_Processor.ProcessTo(ToFrameTime(0.2)).ToArray();
            Assert.That(sampledLenses1.Last().lens.Value.FocusDistance, Is.GreaterThan(0).And.LessThan(3f));

            m_Processor.AddFocusDistanceKeyframe(1.0, 2f);
            _ = m_Processor.ProcessTo(ToFrameTime(2.0)).ToArray();

            m_Processor.AddFocusDistanceKeyframe(m_Processor.CurrentTime, 5f);
            var sampledLenses2 = m_Processor.ProcessTo(ToFrameTime(3.0)).ToArray();
            Assert.That(sampledLenses2.Last().lens.Value.FocusDistance, Is.GreaterThan(2f).And.LessThan(5f));
        }
    }
}
