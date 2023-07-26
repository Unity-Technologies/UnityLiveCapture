using System.IO;

namespace Unity.LiveCapture.VideoStreaming.Client.Rtcp
{
    abstract class RtcpSdesItem
    {
        public abstract int SerializedLength { get; }

        public abstract void Serialize(Stream stream);
    }
}
