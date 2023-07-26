using System;

namespace Unity.LiveCapture.VideoStreaming.Client
{
    [Flags]
    public enum RequiredTracks
    {
        Video = 1,
        Audio = 2,
        All = Video | Audio
    }
}
