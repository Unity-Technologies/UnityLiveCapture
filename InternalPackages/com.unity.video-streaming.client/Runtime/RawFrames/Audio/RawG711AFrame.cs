using System;

namespace Unity.LiveCapture.VideoStreaming.Client.RawFrames.Audio
{
    public class RawG711AFrame : RawG711Frame
    {
        public RawG711AFrame(DateTime timestamp, ArraySegment<byte> frameSegment)
            : base(timestamp, frameSegment)
        {
        }
    }
}
