using System;

namespace Unity.LiveCapture.VideoStreaming.Server.Messages
{
    /// <summary>
    /// Class containing helper constant for general use headers.
    /// </summary>
    static class RtspHeaderNames
    {
        public const string ContentBase = "Content-Base";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentType = "Content-Type";

        public const string Public = "Public";
        public const string Session = "Session";
        public const string Transport = "Transport";

        public const string WWWAuthenticate = "WWW-Authenticate";
        public const string Authorization = "Authorization";
    }
}
