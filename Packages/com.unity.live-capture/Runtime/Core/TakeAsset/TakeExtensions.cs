using System;

namespace Unity.LiveCapture
{
    static class TakeExtensions
    {
        internal static bool TryGetContentRange(this Take take, out double start, out double end)
        {
            start = 0d;
            end = 0d;

            if (take == null)
            {
                return false;
            }

            var timeline = take.Timeline;

            if (timeline == null || timeline.outputTrackCount == 0)
            {
                return false;
            }

            var tracks = timeline.GetOutputTracks();
            var min = double.MaxValue;
            var max = double.MinValue;
            var hasValue = false;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track.muted)
                    continue;

                min = Math.Min(track.start, min);
                max = Math.Max(track.end, max);
                hasValue = true;
            }

            if (hasValue)
            {
                start = min;
                end = max;
            }

            return hasValue;
        }
    }
}
