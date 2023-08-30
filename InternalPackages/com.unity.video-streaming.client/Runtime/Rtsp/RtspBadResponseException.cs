using System;

namespace Unity.LiveCapture.VideoStreaming.Client.Rtsp
{
    [Serializable]
    public class RtspBadResponseException : RtspClientException
    {
        public RtspBadResponseException(string message) : base(message)
        {
        }
    }
}
