using UnityEngine;
using Unity.LiveCapture.Cameras;

namespace Unity.LiveCapture.MoSys
{
    /// <summary>
    /// A post-processor that records the timecode of a <see cref="LiveStreamCaptureDevice"/>.
    /// </summary>
    [RequireComponent(typeof(LiveStreamCaptureDevice))]
    [HelpURL(Documentation.baseURL + "ref-component-timecode-post-processor" + Documentation.endURL)]
    public sealed class TimecodePostProcessor : LiveStreamPostProcessor
    {
        LivePropertyHandle[] m_Handles;

        /// <inheritdoc/>
        protected override void CreateLiveProperties(LiveStream stream)
        {
            m_Handles = new LivePropertyHandle[]
            {
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_Hours", (c,v) => c.Hours = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_Minutes", (c,v) => c.Minutes = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_Seconds", (c,v) => c.Seconds = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_Frames", (c,v) => c.Frames = v),
                stream.CreateProperty<TimecodeComponent, bool>(string.Empty, "m_IsDropFrame", (c,v) => c.IsDropFrame = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_Subframe", (c,v) => c.Subframe = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_Resolution", (c,v) => c.Resolution = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_RateNumerator", (c,v) => c.RateNumerator = v),
                stream.CreateProperty<TimecodeComponent, int>(string.Empty, "m_RateDenominator", (c,v) => c.RateDenominator = v),
                stream.CreateProperty<TimecodeComponent, bool>(string.Empty, "m_RateIsDropFrame", (c,v) => c.RateIsDropFrame = v)
            };
        }

        /// <inheritdoc/>
        protected override void RemoveLiveProperties(LiveStream stream)
        {
            foreach (var handle in m_Handles)
            {
                stream.RemoveProperty(handle);
            }
        }

        /// <inheritdoc/>
        protected override void PostProcessFrame(LiveStream stream)
        {
            var frameTimeWithRate = Device.CurrentFrameTime;

            if (frameTimeWithRate.HasValue)
            {
                var frameTime = frameTimeWithRate.Value.Time;
                var frameRate = frameTimeWithRate.Value.Rate;
                var timecode = Timecode.FromFrameTime(frameRate, frameTime);

                stream.SetValue(m_Handles[0], timecode.Hours);
                stream.SetValue(m_Handles[1], timecode.Minutes);
                stream.SetValue(m_Handles[2], timecode.Seconds);
                stream.SetValue(m_Handles[3], timecode.Frames);
                stream.SetValue(m_Handles[4], timecode.IsDropFrame);
                stream.SetValue(m_Handles[5], timecode.Subframe.Value);
                stream.SetValue(m_Handles[6], timecode.Subframe.Resolution);
                stream.SetValue(m_Handles[7], frameRate.Numerator);
                stream.SetValue(m_Handles[8], frameRate.Denominator);
                stream.SetValue(m_Handles[9], frameRate.IsDropFrame);
            }
        }
    }
}
