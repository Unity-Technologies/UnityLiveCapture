using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestGetParameter : RtspRequest
    {
        public RtspRequestGetParameter()
        {
            Command = "GET_PARAMETER * RTSP/1.0";
        }
    }
}
