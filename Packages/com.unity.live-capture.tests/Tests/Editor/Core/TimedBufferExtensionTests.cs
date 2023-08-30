using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TimedBufferExtensionTests
    {
        TimedDataBuffer<float> m_KeyFrames;
        static readonly FrameRate k_FrameRate = StandardFrameRate.FPS_30_00;

        static readonly FrameTime k_OneSecond = FrameTime.FromSeconds(k_FrameRate, 1);
        static readonly FrameTime k_TwoSeconds = FrameTime.FromSeconds(k_FrameRate, 2);
        static readonly FrameTime k_ThreeSeconds = FrameTime.FromSeconds(k_FrameRate, 3);


        [SetUp]
        public void Setup()
        {
            m_KeyFrames = new TimedDataBuffer<float>(k_FrameRate, 10);
        }

        [Test]
        public void FindValueBeforeCutoff()
        {
            Assert.That(m_KeyFrames.GetLatest(k_OneSecond), Is.Null);
            m_KeyFrames.Add(3, k_TwoSeconds);
            Assert.That(m_KeyFrames.GetLatest(k_OneSecond), Is.Null);
            Assert.That(m_KeyFrames.GetLatest(k_TwoSeconds), Is.EqualTo(3));
            m_KeyFrames.Add(2, k_ThreeSeconds);
            Assert.That(m_KeyFrames.GetLatest(k_ThreeSeconds), Is.EqualTo(2));
            m_KeyFrames.Add(4, k_ThreeSeconds + new FrameTime(1));
            Assert.That(m_KeyFrames.GetLatest(k_TwoSeconds + new FrameTime(10)), Is.EqualTo(3));
            Assert.That(m_KeyFrames.GetLatest(k_ThreeSeconds + new FrameTime(10)), Is.EqualTo(4));
        }
    }
}
