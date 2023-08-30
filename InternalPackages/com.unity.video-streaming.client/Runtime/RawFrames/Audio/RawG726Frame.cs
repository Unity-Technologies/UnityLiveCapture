using System;

namespace Unity.LiveCapture.VideoStreaming.Client.RawFrames.Audio
{
    public class RawG726Frame : RawAudioFrame
    {
        public int BitsPerCodedSample { get; }

        public RawG726Frame(DateTime timestamp, ArraySegment<byte> frameSegment, int bitsPerCodedSample)
            : base(timestamp, frameSegment)
        {
            BitsPerCodedSample = bitsPerCodedSample;
        }
    }
}
