namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    class RtspRequestRecord : RtspRequest
    {
        public RtspRequestRecord()
        {
            Command = "RECORD * RTSP/1.0";
        }
    }
}
