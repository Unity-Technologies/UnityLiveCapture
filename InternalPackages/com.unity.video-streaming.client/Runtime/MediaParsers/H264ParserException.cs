using System;
using System.Runtime.Serialization;

namespace Unity.LiveCapture.VideoStreaming.Client.MediaParsers
{
    [Serializable]
    public class H264ParserException : Exception
    {
        public H264ParserException()
        {
        }

        public H264ParserException(string message) : base(message)
        {
        }

        public H264ParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected H264ParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
