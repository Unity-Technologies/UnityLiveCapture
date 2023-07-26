using System;

namespace Unity.LiveCapture.VideoStreaming.Client.RawFrames.Audio
{
    public class RawAACFrame : RawAudioFrame
    {
        public ArraySegment<byte> ConfigSegment { get; }

        public RawAACFrame(DateTime timestamp, ArraySegment<byte> frameBytes, ArraySegment<byte> configSegment)
            : base(timestamp, frameBytes)
        {
            ConfigSegment = configSegment;
        }
    }
}
