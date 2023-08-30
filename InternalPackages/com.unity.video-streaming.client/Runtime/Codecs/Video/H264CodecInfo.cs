using System;

namespace Unity.LiveCapture.VideoStreaming.Client.Codecs.Video
{
    class H264CodecInfo : VideoCodecInfo
    {
        public byte[] SpsPpsBytes { get; set; } = Array.Empty<byte>();
    }
}
