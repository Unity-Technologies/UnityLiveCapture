using System.Collections;
using NUnit.Framework;
using Unity.LiveCapture.VideoStreaming.Server;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    public class VideoStreamSourceTests
    {
        [Test]
        public void CalculateEncoderResolutionMinTest()
        {
            var actual = VideoStreamSource.CalculateEncoderResolution(-1000000 * Vector2Int.one);
            Assert.AreEqual(VideoStreamSource.k_MinEncoderResolution, actual);
        }

        [Test]
        public void CalculateEncoderResolutionMaxTest()
        {
            var actual = VideoStreamSource.CalculateEncoderResolution(1000000 * Vector2Int.one);
            Assert.AreEqual(VideoStreamSource.k_MaxEncoderResolution, actual);
        }

        [Test]
        public void CalculateEncoderResolutionBlockSizeTest()
        {
            Assert.AreEqual(512 * Vector2Int.one, VideoStreamSource.CalculateEncoderResolution(512 * Vector2Int.one));
            Assert.AreEqual(512 * Vector2Int.one, VideoStreamSource.CalculateEncoderResolution(513 * Vector2Int.one));
            Assert.AreEqual(512 * Vector2Int.one, VideoStreamSource.CalculateEncoderResolution(514 * Vector2Int.one));
            Assert.AreEqual(512 * Vector2Int.one, VideoStreamSource.CalculateEncoderResolution(515 * Vector2Int.one));
            Assert.AreEqual(516 * Vector2Int.one, VideoStreamSource.CalculateEncoderResolution(516 * Vector2Int.one));
        }
    }
}
