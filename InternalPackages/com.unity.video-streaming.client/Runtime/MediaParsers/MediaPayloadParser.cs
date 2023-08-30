using System;
using Unity.LiveCapture.VideoStreaming.Client.Codecs;
using Unity.LiveCapture.VideoStreaming.Client.Codecs.Audio;
using Unity.LiveCapture.VideoStreaming.Client.Codecs.Video;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames;

namespace Unity.LiveCapture.VideoStreaming.Client.MediaParsers
{
    abstract class MediaPayloadParser : IMediaPayloadParser
    {
        private DateTime _baseTime = DateTime.MinValue;

        public Action<RawFrame> FrameGenerated { get; set; }

        public abstract void Parse(TimeSpan timeOffset, ArraySegment<byte> byteSegment, bool markerBit);

        public abstract void ResetState();

        protected DateTime GetFrameTimestamp(TimeSpan timeOffset)
        {
            if (timeOffset == TimeSpan.MinValue)
                return DateTime.UtcNow;

            if (_baseTime == DateTime.MinValue)
                _baseTime = DateTime.UtcNow;

            return _baseTime + timeOffset;
        }

        protected virtual void OnFrameGenerated(RawFrame e)
        {
            FrameGenerated?.Invoke(e);
        }

        public static IMediaPayloadParser CreateFrom(CodecInfo codecInfo)
        {
            switch (codecInfo)
            {
                case H264CodecInfo h264CodecInfo:
                    return new H264VideoPayloadParser(h264CodecInfo);
                case MJPEGCodecInfo _:
                    return new MJPEGVideoPayloadParser();
                case AACCodecInfo aacCodecInfo:
                    return new AACAudioPayloadParser(aacCodecInfo);
                case G711CodecInfo g711CodecInfo:
                    return new G711AudioPayloadParser(g711CodecInfo);
                case G726CodecInfo g726CodecInfo:
                    return new G726AudioPayloadParser(g726CodecInfo);
                case PCMCodecInfo pcmCodecInfo:
                    return new PCMAudioPayloadParser(pcmCodecInfo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(codecInfo),
                        $"Unsupported codec: {codecInfo.GetType().Name}");
            }
        }
    }
}
