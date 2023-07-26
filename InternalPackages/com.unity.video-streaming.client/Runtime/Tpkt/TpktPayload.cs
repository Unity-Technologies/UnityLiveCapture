using System;

namespace Unity.LiveCapture.VideoStreaming.Client.Tpkt
{
    struct TpktPayload
    {
        public int Channel { get; }
        public ArraySegment<byte> PayloadSegment { get; }

        public TpktPayload(int channel, ArraySegment<byte> payloadSegment)
        {
            Channel = channel;
            PayloadSegment = payloadSegment;
        }
    }
}
