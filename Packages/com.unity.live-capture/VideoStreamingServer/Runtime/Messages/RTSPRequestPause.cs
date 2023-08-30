using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestPause : RtspRequest
    {
        public RtspRequestPause()
        {
            Command = "PAUSE * RTSP/1.0";
        }
    }
}
