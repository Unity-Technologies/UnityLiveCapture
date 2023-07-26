using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Basic synchronization calibrator suitable for low-latency and low-jitter scenarios.
    /// </summary>
    class DefaultSyncCalibrator : ISynchronizationCalibrator
    {
        /// <summary>
        /// The maximum data source timecode difference allowed for calibration.
        /// A data source whose timecode differs from the others by more than this number of frames is excluded from calibration.
        /// </summary>
        public FrameTime OutlierThreshold { get; set; } = new FrameTime(100);

        /// <summary>
        /// The number of consecutive successful synchronization updates required for calibration to be accepted.
        /// </summary>
        public int RequiredGoodSamples { get; set; } = 60;

        public IEnumerable<CalibrationResult> Execute(ITimecodeSource timecodeSource, IEnumerable<ITimedDataSource> dataSources)
        {
            if (timecodeSource == null || dataSources == null)
            {
                Debug.LogWarning("Timecode source or data sources are null.");
                yield return new CalibrationResult(CalibrationStatus.Failed);
                yield break;
            }
            if (timecodeSource.CurrentTime == null)
            {
                Debug.LogWarning("Timecode source does not have a valid time.");
                yield return new CalibrationResult(CalibrationStatus.Failed);
                yield break;
            }

            // We try to exclude data sources whose timecodes are outliers compared to the other data sources.
            // This helps us to ignore incorrectly configured sources and ensure the calibration completes in reasonable time.
            var sources = GetSortedSourcesWithoutOutliers(timecodeSource, dataSources, OutlierThreshold);

            if (sources.Count == 0)
            {
                yield return new CalibrationResult(CalibrationStatus.Completed);
                yield break;
            }

            // Find a decent initial guess for the delay
            var mostDelayedSource = sources.First();

            if (!mostDelayedSource.TryGetBufferRange(out _, out var newestTime))
            {
                yield return new CalibrationResult(CalibrationStatus.Failed);
                yield break;
            }

            var sourceTime = timecodeSource.CurrentTime.Value.Time;
            var newestTimeInSourceRate = FrameTime.Remap(newestTime, mostDelayedSource.FrameRate, timecodeSource.FrameRate);
            var delay = (sourceTime - newestTimeInSourceRate).Ceil();

            // Adjust the delay value until it works consistently
            foreach (var newDelay in CalibrateDelayIterative(timecodeSource, sources, delay, RequiredGoodSamples))
            {
                delay = newDelay;
                yield return new CalibrationResult(CalibrationStatus.InProgress, delay);
            }

            // Select initial buffer sizes close to the required values
            CalculateInitialBufferSizes(timecodeSource, sources, delay);

            // Increase buffer sizes until the sources are able to present samples at the delayed time
            foreach (var status in CalibrateBufferSizesIterative(timecodeSource, sources, delay, RequiredGoodSamples))
            {
                yield return new CalibrationResult(status, delay);
            }

            yield return new CalibrationResult(CalibrationStatus.Completed, delay);
        }

        static IEnumerable<FrameTime> CalibrateDelayIterative(
            ITimecodeSource timecodeSource,
            IEnumerable<ITimedDataSource> dataSources,
            FrameTime initialDelay,
            int requiredGoodSamples)
        {
            var delay = initialDelay;
            var goodSamples = 0;

            while (goodSamples < requiredGoodSamples)
            {
                var sourceTime = timecodeSource.CurrentTime;

                if (sourceTime == null)
                {
                    yield break;
                }

                var presentTime = new FrameTimeWithRate(sourceTime.Value.Rate, sourceTime.Value.Time - delay);
                var addedDelay = false;

                // grow the buffers of sources that don't have an old enough sample buffered
                foreach (var source in dataSources)
                {
                    if (!source.TryGetBufferRange(out _, out var newestSample))
                    {
                        continue;
                    }
                    if (presentTime.Time <= FrameTime.Remap(newestSample, source.FrameRate, presentTime.Rate))
                    {
                        continue;
                    }

                    delay++;
                    addedDelay = true;
                    break;
                }

                if (addedDelay)
                {
                    goodSamples = 0;
                }
                else
                {
                    goodSamples++;
                }

                yield return delay;
            }
        }

        static IEnumerable<CalibrationStatus> CalibrateBufferSizesIterative(
            ITimecodeSource timecodeSource,
            IEnumerable<ITimedDataSource> dataSources,
            FrameTime delay,
            int requiredGoodSamples)
        {
            var goodSamples = 0;

            while (goodSamples < requiredGoodSamples)
            {
                var sourceTime = timecodeSource.CurrentTime;

                if (sourceTime == null)
                {
                    yield break;
                }

                // add a one frame margin so that buffers are a little larger than strictly required
                var presentTime = new FrameTimeWithRate(sourceTime.Value.Rate, sourceTime.Value.Time - delay - new FrameTime(1));
                var waitingToFillBuffers = false;
                var grewBuffers = false;

                // grow the buffers of sources that don't have an old enough sample buffered
                foreach (var source in dataSources)
                {
                    // Only grow buffers that aren't already at their maximum size.
                    if (source.BufferSize >= (source.MaxBufferSize ?? int.MaxValue))
                    {
                        continue;
                    }

                    if (!source.TryGetBufferRange(out var oldestTime, out var newestTime))
                    {
                        continue;
                    }

                    // Before checking if an adjusted buffer size is good, we need to wait for the buffer to fill.
                    var frameDelta = (newestTime - oldestTime).Round().FrameNumber;

                    if (frameDelta < source.BufferSize - 1)
                    {
                        waitingToFillBuffers = true;
                        continue;
                    }

                    // Only grow buffers without samples older then the present time
                    if (presentTime.Time >= FrameTime.Remap(oldestTime, source.FrameRate, presentTime.Rate))
                    {
                        continue;
                    }

                    source.BufferSize++;
                    grewBuffers = true;
                }

                if (grewBuffers)
                {
                    goodSamples = 0;
                }
                else if (!waitingToFillBuffers)
                {
                    goodSamples++;
                }

                yield return CalibrationStatus.InProgress;
            }
        }

        static List<ITimedDataSource> GetSortedSourcesWithoutOutliers(ITimecodeSource timecodeSource, IEnumerable<ITimedDataSource> dataSources, FrameTime outlierThreshold)
        {
            // get the most recent sample times from all sources and sort them
            var sourceTimes = new List<(ITimedDataSource source, FrameTime time)>();

            foreach (var source in dataSources)
            {
                if (!source.TryGetBufferRange(out _, out var newestTime))
                {
                    continue;
                }

                sourceTimes.Add((source, FrameTime.Remap(newestTime, source.FrameRate, timecodeSource.FrameRate)));
            }

            if (sourceTimes.Count == 0)
            {
                return new List<ITimedDataSource>();
            }

            var sortedSources = sourceTimes
                .OrderBy(source => source.time)
                .ToArray();

            // find the largest cluster of sources by their times, breaking ties by distance from the source timecode
            var sourceTime = timecodeSource.CurrentTime;

            var largestCluster = new List<ITimedDataSource>();
            var smallestSourceDelta = default(double?);

            foreach (var source in sortedSources)
            {
                var cluster = new List<ITimedDataSource>();
                var sourceDelta = sourceTime != null ? Math.Abs((double)(sourceTime.Value.Time - source.time)) : 0.0;

                foreach (var otherSource in sortedSources)
                {
                    if (Math.Abs((double)(otherSource.time - source.time)) <= (double)outlierThreshold)
                    {
                        cluster.Add(otherSource.source);
                    }
                }

                if (largestCluster.Count < cluster.Count || (largestCluster.Count == cluster.Count && smallestSourceDelta > sourceDelta))
                {
                    largestCluster = cluster;
                    smallestSourceDelta = sourceDelta;
                }
            }

            return largestCluster;
        }

        static void CalculateInitialBufferSizes(ITimecodeSource timecodeSource, IEnumerable<ITimedDataSource> dataSources, FrameTime delay)
        {
            var sourceTime = timecodeSource.CurrentTime;

            if (sourceTime == null)
            {
                return;
            }

            var presentTime = new FrameTimeWithRate(sourceTime.Value.Rate, sourceTime.Value.Time - delay);

            foreach (var source in dataSources)
            {
                if (source.TryGetBufferRange(out _, out var newestTime))
                {
                    var delta = newestTime - presentTime.Remap(source.FrameRate).Time;
                    source.BufferSize = Mathf.Clamp(delta.Ceil().FrameNumber, source.MinBufferSize ?? 1, source.MaxBufferSize ?? int.MaxValue);
                }
            }
        }
    }
}
