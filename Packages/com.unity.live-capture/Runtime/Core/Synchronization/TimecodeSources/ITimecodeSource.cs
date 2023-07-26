namespace Unity.LiveCapture
{
    /// <summary>
    /// The interface for sources that provide timecodes used for synchronization.
    /// </summary>
    public interface ITimecodeSource : IRegistrable
    {
        /// <summary>
        /// The frame rate of the timecode source.
        /// </summary>
        /// <remarks>
        /// This value may change during a frame.
        /// </remarks>
        FrameRate FrameRate { get; }

        /// <summary>
        /// The frame time and frame rate to use for the current frame, if available.
        /// </summary>
        /// <remarks>
        /// This value is only changed at the beginning of each frame.
        /// </remarks>
        FrameTimeWithRate? CurrentTime { get; }
    }
}
