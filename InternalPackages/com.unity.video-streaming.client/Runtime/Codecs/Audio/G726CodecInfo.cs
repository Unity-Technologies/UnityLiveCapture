namespace Unity.LiveCapture.VideoStreaming.Client.Codecs.Audio
{
    class G726CodecInfo : AudioCodecInfo
    {
        public int Bitrate { get; }

        public int SampleRate { get; set; } = 8000;

        public int Channels { get; set; } = 1;

        public G726CodecInfo(int bitrate)
        {
            Bitrate = bitrate;
        }
    }
}
