using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Describes the ability for a <see cref="ITimedDataSource"/> to produce a data sample at a specific timecode.
    /// </summary>
    public enum TimedSampleStatus
    {
        /// <summary>
        /// Data is available at the specified timecode.
        /// </summary>
        [InspectorName("OK")]
        Ok,

        /// <summary>
        /// The data in the buffer is too old (i.e. latency is too high).
        /// </summary>
        [InspectorName("Behind")]
        Behind,

        /// <summary>
        /// The data in the buffer is too new (i.e. synchronization updates are happening too late, buffer is too small).
        /// </summary>
        [InspectorName("Ahead")]
        Ahead,

        /// <summary>
        /// There is no data available.
        /// </summary>
        [InspectorName("Data missing")]
        DataMissing
    }

    /// <summary>
    /// An interface for a collection of timecoded data samples that can be synchronized.
    /// </summary>
    public interface ITimedDataSource : IRegistrable
    {
        /// <summary>
        /// The number of data samples per second.
        /// </summary>
        FrameRate FrameRate { get; }

        /// <summary>
        /// The current buffer size.
        /// </summary>
        int BufferSize { get; set; }

        /// <summary>
        /// The maximum size of the sample buffer, if limited.
        /// </summary>
        int? MaxBufferSize { get; }

        /// <summary>
        /// The minimum size of the sample buffer, if limited.
        /// </summary>
        int? MinBufferSize { get; }

        /// <summary>
        /// The time offset applied to sample timecodes, in frames.
        /// </summary>
        /// <remarks>
        /// The frame duration corresponds to the <see cref="FrameRate"/> of this source.
        /// This value should typically match the time delay between timecode generation and data sampling.
        /// </remarks>
        FrameTime Offset { get; set; }

        /// <summary>
        /// Signal to the data source whether it is being synchronized via <see cref="PresentAt"/>.
        /// </summary>
        /// <remarks>
        /// When <see langword="true"/>, the data source should disable its own update mechanism.
        /// </remarks>
        bool IsSynchronized { get; set; }

        /// <summary>
        /// The <see cref="ISynchronizer"/> controlling this source.
        /// </summary>
        /// <remarks>
        /// In most cases you can simply implement this as an auto-property. The default synchronizer implementation
        /// automatically assigns this property when you call <see cref="Unity.LiveCapture.ISynchronizer.AddDataSource"/>.
        /// </remarks>
        ISynchronizer Synchronizer { get; set; }

        /// <summary>
        /// The object to record to the undo stack when the user modifies the properties of this data source.
        /// </summary>
        /// <remarks>
        /// To enable undo when using the synchronization windows, this must be the object that stores the serialized state for
        /// this data source. When <see langword="null"/>, this data source will not support undo operations.
        /// </remarks>
        Object UndoTarget { get; }

        /// <summary>
        /// Gets the frame time of the newest and oldest samples buffered by the data source.
        /// </summary>
        /// <remarks>
        /// The frame duration corresponds to the <see cref="FrameRate"/> of this source.
        /// </remarks>
        /// <param name="oldestSample">The frame time of the oldest buffered sample, or <see langword="default"/> if there are no buffered samples.</param>
        /// <param name="newestSample">The frame time of the newest buffered sample, or <see langword="default"/> if there are no buffered samples.</param>
        /// <returns><see langword="true"/> if there are any buffered samples; otherwise, <see langword="false"/>.</returns>
        bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample);

        /// <summary>
        /// Applies the data for a given timecode to the scene.
        /// </summary>
        /// <param name="presentTime">The timecode to present.</param>
        /// <returns>A status representing the ability of the source to present at the requested timecode.</returns>
        TimedSampleStatus PresentAt(FrameTimeWithRate presentTime);
    }
}
