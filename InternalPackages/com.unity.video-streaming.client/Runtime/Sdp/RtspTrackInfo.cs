namespace Unity.LiveCapture.VideoStreaming.Client.Sdp
{
    abstract class RtspTrackInfo
    {
        public string TrackName { get; }

        protected RtspTrackInfo(string trackName)
        {
            TrackName = trackName;
        }
    }
}
