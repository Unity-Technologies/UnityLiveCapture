using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestAnnounce : RtspRequest
    {
        public RtspRequestAnnounce()
        {
            Command = "ANNOUNCE * RTSP/1.0";
        }
    }
}
