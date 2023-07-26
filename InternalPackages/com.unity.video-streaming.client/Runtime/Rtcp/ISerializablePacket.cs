using System.IO;

namespace Unity.LiveCapture.VideoStreaming.Client.Rtcp
{
    interface ISerializablePacket
    {
        void Serialize(Stream stream);
    }
}
