using System;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames;

namespace Unity.LiveCapture.VideoStreaming.Client.MediaParsers
{
    interface IMediaPayloadParser
    {
        Action<RawFrame> FrameGenerated { get; set; }

        void Parse(TimeSpan timeOffset, ArraySegment<byte> byteSegment, bool markerBit);

        void ResetState();
    }
}
