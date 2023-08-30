using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Unity.LiveCapture.Tests.Editor
{
    /// <summary>
    /// A fake timecode source that lets us control the time.
    /// </summary>
    class MockTimecodeSource : ITimecodeSource
    {
        FrameTimeWithRate m_Now;

        public string Id => "123";

        public string FriendlyName => "Mock timecode";

        public FrameRate FrameRate { get; }

        public FrameTimeWithRate? CurrentTime => m_Now;

        public MockTimecodeSource(FrameTime now, FrameRate frameRate)
        {
            FrameRate = frameRate;
            m_Now = new FrameTimeWithRate(FrameRate, now);
        }

        public void AdvanceFrame(FrameRate frameRate)
        {
            m_Now += new FrameTimeWithRate(frameRate, new FrameTime(1));
        }
    }

    /// <summary>
    /// A simulated timed data source, with buffered data.
    /// </summary>
    /// <remarks>
    /// Simulate the reception of timecoded data by invoking <see cref="Update"/>.
    /// This source simulate a delay and network jitter. It can also represent itself visually (using a rotation).
    /// </remarks>
    [ExecuteAlways]
    class MockTimedDataSource : MonoBehaviour, ITimedDataSource
    {
        public string Name { get; set; }
        public int SimulatedDelay { get; set; }

        public string Id { get; } = Guid.NewGuid().ToString();

        public string FriendlyName => Name;

        /// <inheritdoc />
        FrameRate ITimedDataSource.FrameRate => FrameRate;

        public int BufferSize
        {
            get => m_Buffer?.Capacity ?? 0;
            set
            {
                if (m_Buffer != null)
                {
                    m_Buffer.Capacity = value;
                }
            }
        }

        public int? MaxBufferSize { get; set; }
        public int? MinBufferSize { get; }
        public FrameTime Offset { get; set; }

        public ISynchronizer Synchronizer { get; set; }

        public Object UndoTarget => null;

        public bool IsSynchronized { get; set; }

        public int Jitter { get; set; }

        public ITimecodeSource TimecodeSource
        {
            get => m_TimecodeSource;
            set
            {
                m_TimecodeSource = value;
                if (m_TimecodeSource != null)
                {
                    m_Buffer = new TimedDataBuffer<FrameTime>(m_TimecodeSource.FrameRate, 1);
                }
            }
        }

        readonly Queue<FrameTime> m_DelayedSamples = new Queue<FrameTime>();
        TimedDataBuffer<FrameTime> m_Buffer;

        FrameRate FrameRate => TimecodeSource?.FrameRate ?? default;

        string m_Label = "Not synced";
        ITimecodeSource m_TimecodeSource;

        void OnEnable()
        {
            m_Label = "";
        }

        /// <inheritdoc />
        public bool TryGetBufferRange(out FrameTime oldestSample, out FrameTime newestSample)
        {
            if (m_Buffer != null)
            {
                return m_Buffer.TryGetBufferRange(out oldestSample, out newestSample);
            }

            oldestSample = default;
            newestSample = default;
            return false;
        }

        public TimedSampleStatus PresentAt(FrameTimeWithRate presentTime)
        {
            if (TimecodeSource == null || !FrameRate.IsValid || m_Buffer == null)
                return TimedSampleStatus.DataMissing;

            // Convert timecode into this device's internal frame rate, and add the offset
            var frameToPresent = FrameTime.Remap(presentTime.Time, presentTime.Rate, FrameRate);
            frameToPresent -= Offset;

            // The element in the buffer that "best" matches the requested frame.
            var status = m_Buffer.TryGetSample(frameToPresent, out var frameTimeToShow);

            // If we have a frame to present, show it visually via a rotation.
            if (status != TimedSampleStatus.DataMissing)
            {
                var angle = 100f * (float)frameTimeToShow.ToSeconds(presentTime.Rate);
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
            }

            m_Label = status switch
            {
                TimedSampleStatus.Ok => Timecode.FromFrameTime(presentTime.Rate, frameTimeToShow).ToString(),
                TimedSampleStatus.Behind => "Behind",
                TimedSampleStatus.Ahead => "Ahead",
                TimedSampleStatus.DataMissing => "Missing",
                _ => throw new ArgumentOutOfRangeException(nameof(status))
            };

            return status;
        }

        public void Update()
        {
            if (TimecodeSource == null || !FrameRate.IsValid)
            {
                return;
            }

            var frameTime = TimecodeSource.CurrentTime ?? default;
            var value = FrameTime.Remap(frameTime.Time, TimecodeSource.FrameRate, FrameRate);

            // Simulate a delay by stashing the latest data instead of putting it
            // directly into the buffer.
            var delay = SimulatedDelay + Random.Range(0, Jitter + 1);
            m_DelayedSamples.Enqueue(value);

            // Get the delayed sample
            value = m_DelayedSamples.Peek();
            while (m_DelayedSamples.Count > delay)
            {
                value = m_DelayedSamples.Dequeue();
            }

            m_Buffer.Add(value, new FrameTimeWithRate(FrameRate, value));
        }

        void OnDrawGizmos()
        {
            Handles.Label(transform.position, m_Label);
        }
    }

    [TestFixture]
    public class SynchronizerTests
    {
        const int k_CurrentFrame = 10;
        const int k_MaxIterations = 1200;
        SynchronizerComponent m_Synchronizer;
        MockTimecodeSource m_TimecodeSource;
        readonly FrameRate m_FrameRateSource = StandardFrameRate.FPS_23_976_DF;

        MockTimedDataSource m_GoodDataSource;
        MockTimedDataSource m_FastDataSource;
        MockTimedDataSource m_SlowDataSource;
        MockTimedDataSource m_FlakyDataSource;
        GameObject m_DataSourceContainer;
        TimecodeSourceManager m_TimecodeSourceManager;
        TimedDataSourceManager m_TimedDataSourceManager;

        [SetUp]
        public void Setup()
        {
            var currentFrameTime = new FrameTime(k_CurrentFrame);

            m_TimecodeSourceManager = new TimecodeSourceManager(Guid.NewGuid().ToString());
            TimecodeSourceRef.Manager = m_TimecodeSourceManager;

            m_TimecodeSource = new MockTimecodeSource(
                currentFrameTime,
                m_FrameRateSource
            );
            m_TimecodeSourceManager.Register(m_TimecodeSource);

            m_TimedDataSourceManager = new TimedDataSourceManager(Guid.NewGuid().ToString());
            TimedDataSourceRef.Manager = m_TimedDataSourceManager;

            m_DataSourceContainer = new GameObject("Mock data sources");
            m_GoodDataSource = m_DataSourceContainer.AddComponent<MockTimedDataSource>();
            m_GoodDataSource.TimecodeSource = m_TimecodeSource;
            m_GoodDataSource.Name = "Good";
            m_GoodDataSource.SimulatedDelay = 5;
            m_GoodDataSource.BufferSize = 10;
            m_TimedDataSourceManager.Register(m_GoodDataSource);

            m_FastDataSource = m_DataSourceContainer.AddComponent<MockTimedDataSource>();
            m_FastDataSource.TimecodeSource = m_TimecodeSource;
            m_FastDataSource.Name = "Fast";
            m_TimedDataSourceManager.Register(m_FastDataSource);

            m_SlowDataSource = m_DataSourceContainer.AddComponent<MockTimedDataSource>();
            m_SlowDataSource.TimecodeSource = m_TimecodeSource;
            m_SlowDataSource.Name = "Slow";
            m_SlowDataSource.SimulatedDelay = 15;
            m_TimedDataSourceManager.Register(m_SlowDataSource);

            m_FlakyDataSource = m_DataSourceContainer.AddComponent<MockTimedDataSource>();
            m_FlakyDataSource.TimecodeSource = null;
            m_FlakyDataSource.Name = "Flaky";
            m_FlakyDataSource.BufferSize = 10;
            m_TimedDataSourceManager.Register(m_FlakyDataSource);

            m_Synchronizer = new GameObject("Synchronizer", typeof(SynchronizerComponent))
                .GetComponent<SynchronizerComponent>();
            m_Synchronizer.Impl.TimecodeSource = m_TimecodeSource;
            m_Synchronizer.Impl.Delay = new FrameTime(10);
        }

        [TearDown]
        public void TearDown()
        {
#if UNITY_2023_1_OR_NEWER
            foreach (var gameObject in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
#else
            foreach (var gameObject in Object.FindObjectsOfType<GameObject>())
#endif
            {
                if (gameObject.GetComponent<ITimedDataSource>() != null
                    || gameObject.GetComponent<SynchronizerComponent>() != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }

            TimecodeSourceRef.Manager = TimecodeSourceManager.Instance;
            TimedDataSourceRef.Manager = TimedDataSourceManager.Instance;
        }

        [Test]
        public void SynchronizerCanAddAndRemoveDataSources()
        {
            var synchronizer = m_Synchronizer.Impl;

            Assert.That(synchronizer.DataSourceCount, Is.Zero);
            Assert.That(m_FastDataSource.IsSynchronized, Is.False);
            Assert.That(synchronizer.AddDataSource(m_FastDataSource), Is.True);
            synchronizer.Update();
            Assert.That(m_FastDataSource.IsSynchronized, Is.True);
            Assert.That(synchronizer.DataSourceCount, Is.EqualTo(1));

            // Try to add a duplicate
            Assert.That(synchronizer.AddDataSource(m_FastDataSource), Is.False);
            Assert.That(synchronizer.DataSourceCount, Is.EqualTo(1));

            Assert.That(synchronizer.AddDataSource(m_FlakyDataSource), Is.True);
            Assert.That(synchronizer.DataSourceCount, Is.EqualTo(2));

            synchronizer.RemoveDataSource(m_FastDataSource);
            Assert.That(m_FastDataSource.IsSynchronized, Is.False);
            Assert.That(synchronizer.DataSourceCount, Is.EqualTo(1));
            Assert.That(synchronizer.GetDataSource(1), Is.Null);

            Assert.That(synchronizer.GetDataSource(0), Is.EqualTo(m_FlakyDataSource));

            // Can remove (by index) a source that's been disabled/destroyed
            m_TimedDataSourceManager.Unregister(m_FlakyDataSource);
            Object.DestroyImmediate(m_FlakyDataSource);
            Assert.That(m_TimedDataSourceManager.Entries, Has.None.Matches<ITimedDataSource>(source => source?.FriendlyName == "Flaky"));
            Assert.That(synchronizer.DataSourceCount, Is.EqualTo(1));
            synchronizer.RemoveDataSource(0);
            Assert.That(synchronizer.DataSourceCount, Is.Zero);
        }

        [Test]
        public void SynchronizerCanReportStatuses()
        {
            var synchronizer = m_Synchronizer.Impl;
            var dataSources = new List<MockTimedDataSource>
            {
                m_GoodDataSource,
                m_FastDataSource,
                m_SlowDataSource,
                m_FlakyDataSource
            };

            dataSources.ForEach(s => synchronizer.AddDataSource(s));

            Assert.That(synchronizer.GetCurrentDataStatus(0), Is.EqualTo(TimedSampleStatus.DataMissing));
            Assert.That(synchronizer.DataSourceCount, Is.EqualTo(dataSources.Count));

            // Advance the time - wait for the results to settle
            for (int i = 0; i < 25; ++i)
            {
                m_TimecodeSource.AdvanceFrame(m_TimecodeSource.FrameRate);
                foreach (var dataSource in dataSources)
                {
                    dataSource.Update();
                }

                synchronizer.Update();
            }

            var status = Enumerable.Range(0, synchronizer.DataSourceCount)
                .Select(synchronizer.GetCurrentDataStatus)
                .ToArray();
            Assert.That(status, Is.EquivalentTo(new[]
            {
                TimedSampleStatus.Ok,
                TimedSampleStatus.Ahead,
                TimedSampleStatus.Behind,
                TimedSampleStatus.DataMissing
            }));
        }

        [UnityTest]
        public IEnumerator SynchronizerCanPause()
        {
            var synchronizer = m_Synchronizer.Impl;
            var dataSources = new List<MockTimedDataSource>
            {
                m_GoodDataSource,
                m_FastDataSource,
                m_SlowDataSource,
                m_FlakyDataSource
            };
            dataSources.ForEach(s => synchronizer.AddDataSource(s));

            yield return null;

            Assert.That(dataSources.Select(s => s.IsSynchronized), Is.All.True);
            m_Synchronizer.enabled = false;

            yield return null;

            Assert.That(dataSources.Select(s => s.IsSynchronized), Is.All.False);
        }

        [UnityTest]
        public IEnumerator SynchronizerCanInitiateCalibration()
        {
            new List<ITimedDataSource>
                    {m_GoodDataSource, m_FastDataSource, m_SlowDataSource, m_FlakyDataSource}
                .ForEach(s => m_Synchronizer.Impl.AddDataSource(s));

            // Start the calibration
            m_Synchronizer.StartCalibration();

            // Run the game loop until calibration is complete
            var iterations = 0;
            while (m_Synchronizer.Impl.CalibrationStatus == CalibrationStatus.InProgress &&
                   iterations < k_MaxIterations)
            {
                ++iterations;
                m_TimecodeSource.AdvanceFrame(m_TimecodeSource.FrameRate);
                yield return null;
            }

            Assert.That(m_Synchronizer.Impl.CalibrationStatus, Is.EqualTo(CalibrationStatus.Completed));
            // The slow data source has a latency of 15 frames
            Assert.That(m_Synchronizer.Impl.Delay.FrameNumber, Is.EqualTo(15));
            // The fast data source should have an expanded buffer size
            Assert.That(m_FastDataSource.BufferSize, Is.EqualTo(17));
        }

        [Test]
        public void DefaultCalibratorCanSynchronizeWithJitter([NUnit.Framework.Range(0, 100)] int seed)
        {
            Random.InitState(seed);
            m_GoodDataSource.Jitter = 1;
            m_FastDataSource.Jitter = 2;
            m_SlowDataSource.Jitter = 3;

            MockTimedDataSource[] dataSources = {m_GoodDataSource, m_FastDataSource, m_SlowDataSource};

            // For these tests, we can ensure we get a reliable estimate of the upper bound of the delays
            // by requiring a large number of samples.
            var calibrator = new DefaultSyncCalibrator {RequiredGoodSamples = 150};

            var iterations = 0;
            using var result =
                calibrator.Execute(m_TimecodeSource, dataSources).GetEnumerator();
            while (result.MoveNext() && iterations < k_MaxIterations)
            {
                m_TimecodeSource.AdvanceFrame(m_TimecodeSource.FrameRate);
                foreach (var dataSource in dataSources)
                {
                    dataSource.Update();
                }

                iterations++;
            }

            var calibrationResult = result.Current;

            Assert.That(calibrationResult.Status, Is.EqualTo(CalibrationStatus.Completed));

            var statuses = dataSources.Select(s =>
                s.PresentAt(m_TimecodeSource.CurrentTime.Value + new FrameTimeWithRate(m_FrameRateSource, calibrationResult.Delay))
            );

            Assert.That(statuses, Is.All.EqualTo(TimedSampleStatus.Ok));

            // The largest latency we should see in this test is 13 frames: 15 to account for slow source,
            // and 3 for its jitter
            Assert.That(-calibrationResult.Delay.FrameNumber,
                Is.LessThanOrEqualTo(m_SlowDataSource.SimulatedDelay + m_SlowDataSource.Jitter));

            // Buffer size of fast source should account for our global offset
            Assert.That(m_FastDataSource.BufferSize,
                Is.GreaterThanOrEqualTo(calibrationResult.Delay.FrameNumber));
        }

        [Test]
        public void NonConvergentCalibrationCompletes()
        {
            // The maximum buffers size is set too small to be synchronizable
            m_FastDataSource.MaxBufferSize = 2;
            MockTimedDataSource[] dataSources = {m_GoodDataSource, m_FastDataSource, m_SlowDataSource};

            ISynchronizationCalibrator calibrator = new DefaultSyncCalibrator();
            using var result = calibrator.Execute(m_TimecodeSource, dataSources).GetEnumerator();
            int iterations = 0;
            while (result.MoveNext() && iterations < k_MaxIterations)
            {
                m_TimecodeSource.AdvanceFrame(m_TimecodeSource.FrameRate);
                foreach (var dataSource in dataSources)
                {
                    dataSource.Update();
                }

                iterations++;
            }

            Assert.That(result.Current.Status, Is.EqualTo(CalibrationStatus.Completed));
        }

        /// <summary>
        /// Test the calibration visually.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestSynchronizationVisually()
        {
            for (var i = 0; i < 3; i++)
            {
                var testDevice = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testDevice.name = $"Device{i}";
                testDevice.transform.localScale = new Vector3(1, 1, 2);
                testDevice.transform.position = new Vector3(i * 4, 0, 0);
                var testDataSource = testDevice.AddComponent<MockTimedDataSource>();
                testDataSource.Jitter = i * 2;
                testDataSource.SimulatedDelay = i * 5;
                m_TimedDataSourceManager.Register(testDataSource);

                Assert.That(testDataSource, Is.Not.Null);
                m_Synchronizer.Impl.AddDataSource(testDataSource);
                testDataSource.TimecodeSource = m_TimecodeSource;
            }

            for (int i = 0; i < 48; i++)
            {
                m_TimecodeSource.AdvanceFrame(m_TimecodeSource.FrameRate);
                yield return null;
            }

            m_Synchronizer.StartCalibration();

            var iterations = 0;
            while (m_Synchronizer.Impl.CalibrationStatus == CalibrationStatus.InProgress &&
                   iterations < k_MaxIterations)
            {
                ++iterations;
                m_TimecodeSource.AdvanceFrame(m_TimecodeSource.FrameRate);
                yield return null;
            }

            var statuses = Enumerable.Range(0, m_Synchronizer.Impl.DataSourceCount).Select(i => m_Synchronizer.Impl.GetCurrentDataStatus(i));
            Assert.That(statuses, Is.All.EqualTo(TimedSampleStatus.Ok));
        }
    }
}
