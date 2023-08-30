using System;
using System.Runtime.Serialization;

namespace Unity.LiveCapture.VideoStreaming.Client.Sdp
{
    [Serializable]
    public class SdpParserException : Exception
    {
        public SdpParserException()
        {
        }

        public SdpParserException(string message) : base(message)
        {
        }

        public SdpParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SdpParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
