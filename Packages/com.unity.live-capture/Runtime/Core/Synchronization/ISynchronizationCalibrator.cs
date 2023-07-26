using System.Collections.Generic;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An enum defining the statuses of a calibration attempt.
    /// </summary>
    public enum CalibrationStatus
    {
        /// <summary>
        /// The calibrator is currently running.
        /// </summary>
        [InspectorName("In Progress")]
        InProgress,

        /// <summary>
        /// The calibrator completed successfully.
        /// </summary>
        [InspectorName("Complete")]
        Completed,

        /// <summary>
        /// The calibrator was not able to complete successfully.
        /// </summary>
        [InspectorName("Failed")]
        Failed,
    }

    /// <summary>
    /// A struct containing the calibration parameters to apply.
    /// </summary>
    public readonly struct CalibrationResult
    {
        /// <summary>
        /// The status the calibration finished with.
        /// </summary>
        public readonly CalibrationStatus Status;

        /// <summary>
        /// The delay, in frames, to apply to the source timecode to compensate for latency.
        /// </summary>
        public readonly FrameTime Delay;

        /// <summary>
        /// Creates a new <see cref="CalibrationResult"/> instance.
        /// </summary>
        /// <param name="status">The status the calibration finished with.</param>
        /// <param name="delay">The delay, in frames, to apply to the source timecode to compensate for latency.</param>
        public CalibrationResult(CalibrationStatus status, FrameTime delay = default)
        {
            Status = status;
            Delay = delay;
        }
    }

    /// <summary>
    /// The interface for calibration strategies used to configure a synchronizer.
    /// </summary>
    public interface ISynchronizationCalibrator
    {
        /// <summary>
        /// Performs calibration on the provided data sources to determine delays and buffer sizes
        /// allowing for synchronized presentation of data samples.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use calibration to:
        /// a) Determine an optimized presentation delay allowing the data source with the highest latency
        /// to have time to receive the sample for a frame before presenting it.
        /// Ideally keep the delay as small as possible.
        /// b) Determine an optimized buffer size for each data source to prevent them from dropping samples
        /// that haven't been presented yet.
        /// </para>
        /// The result is updated iteratively; the caller should iterate one result each frame.
        /// </remarks>
        /// <param name="timecodeSource">The timecode source that provides the timecodes to present.</param>
        /// <param name="dataSources">The data sources to be synchronized.</param>
        /// <returns>The calibration result for the current frame.</returns>
        IEnumerable<CalibrationResult> Execute(ITimecodeSource timecodeSource, IEnumerable<ITimedDataSource> dataSources);
    }
}
