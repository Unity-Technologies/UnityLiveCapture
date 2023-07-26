using System;

namespace Unity.LiveCapture.VideoStreaming.Client
{
    interface ITransportStream
    {
        void Process(ArraySegment<byte> payloadSegment);
    }
}
