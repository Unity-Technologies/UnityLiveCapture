using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestTeardown : RtspRequest
    {
        // Constructor
        public RtspRequestTeardown()
        {
            Command = "TEARDOWN * RTSP/1.0";
        }
    }
}
