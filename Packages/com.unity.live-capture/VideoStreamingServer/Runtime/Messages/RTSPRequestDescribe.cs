using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestDescribe : RtspRequest
    {
        public RtspRequestDescribe()
        {
            Command = "DESCRIBE * RTSP/1.0";
        }
    }
}
