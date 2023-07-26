using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestPlay : RtspRequest
    {
        public RtspRequestPlay()
        {
            Command = "PLAY * RTSP/1.0";
        }
    }
}
