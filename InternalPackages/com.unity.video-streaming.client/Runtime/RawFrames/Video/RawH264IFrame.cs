using System;

namespace Unity.LiveCapture.VideoStreaming.Client.RawFrames.Video
{
    public class RawH264IFrame : RawH264Frame
    {
        public ArraySegment<byte> SpsPpsSegment { get; }
        public int SpsSize { get; }

        public RawH264IFrame(DateTime timestamp, ArraySegment<byte> frameSegment, ArraySegment<byte> spsPpsSegment, int spsSize)
            : base(timestamp, frameSegment)
        {
            SpsPpsSegment = spsPpsSegment;
            SpsSize = spsSize;
        }
    }
}
