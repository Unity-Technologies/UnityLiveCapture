using System;

namespace Unity.LiveCapture.VideoStreaming.Client.Rtsp
{
    [Serializable]
    public class RtspParseResponseException : RtspClientException
    {
        public RtspParseResponseException(string message) : base(message)
        {
        }
    }
}
