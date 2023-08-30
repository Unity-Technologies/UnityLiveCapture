using System.IO;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    class TestDevice : LiveCaptureDevice
    {
        public LiveCaptureDevice impl = Substitute.For<LiveCaptureDevice>();

        public override bool IsReady()
        {
            return impl.IsReady();
        }

        public override void Write(ITakeBuilder takeBuilder)
        {
            impl.Write(takeBuilder);
        }
    }

    class ContextMock : ITakeRecorderContext
    {
        public IExposedPropertyTable resolver = Substitute.For<IExposedPropertyTable>();
        public double time;
        public double duration;
        public bool isPlaying;
        public Shot shot;

        public int Version { get; }
        public int Selection { get; set; }
        public Shot[] Shots => new Shot[] { shot };

        public void SetShot(int index, Shot shot)
        {
            if (index == 0)
            {
                this.shot = shot;
            }
        }
        public Object GetStorage(int index) => null;
        public void Update() {}
        public IExposedPropertyTable GetResolver(int index) => resolver;
        public double GetTime() => time;
        public void SetTime(double value)
        {
            Pause();
            time = value;
        }
        public void Play() => isPlaying = true;
        public void Pause() => isPlaying = false;
        public bool IsPlaying() => isPlaying;
        public void Rebuild(int index) {}
        public void ClearSceneBindings(int index) {}
        public void SetSceneBindings(int index) {}
        public double GetDuration() => duration;
        public bool IsValid() => true;
    }

    public class TakeRecorderTests
    {
        const string shotName = "shot name";
        const string tmpDir = "Assets/tmp";

        internal ContextMock m_Context;
        internal TestDevice m_Device;
        internal Take m_CustomRangeTake;

        [SetUp]
        public void Setup()
        {
            TakeRecorder.IsLive = true;

            m_Context = new ContextMock()
            {
                shot = new Shot()
                {
                    Name = shotName,
                    Directory = tmpDir
                }
            };

            m_Device = new GameObject("test device", typeof(TestDevice)).GetComponent<TestDevice>();
            m_Device.impl.IsReady().Returns(true);

            m_CustomRangeTake = Resources.Load<Take>("TakeRecorder/Takes/CustomTakeRange");

            TakeRecorder.SetContext(m_Context);
        }

        [TearDown]
        public void TearDown()
        {
            TakeRecorder.SetContext(null);
            TakeRecorder.ClearSubscribers();

            if (m_Device != null)
            {
                GameObject.DestroyImmediate(m_Device.gameObject);
            }

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            AssetDatabase.Refresh();
        }
    }

    // These tests were introduced to test an issue on 2021.2+ and are failing on bokken in batch mode.
#if UNITY_2020_3
    [UnityPlatform(exclude = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer})]
#endif
    [InitializeOnLoad]
    public class TakeRecorderPlaymodeTests
    {
        static TakeRecorderPlaymodeTests()
        {
            Application.logMessageReceived += OnLog;
        }

        static bool s_ReceivedLog;

        [UnityTest]
        public IEnumerator DoesNotLogErrorsOnEnterPlayMode()
        {
            s_ReceivedLog = false;

            yield return new EnterPlayMode();

            Assert.IsFalse(s_ReceivedLog, "EnterPlayMode triggered logs in the console.");

            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator IsliveStateIsKeptThroughEnterAndExitPlayMode()
        {
            TakeRecorder.IsLive = true;
            yield return new EnterPlayMode();
            Assert.IsTrue(TakeRecorder.IsLive);
            yield return new ExitPlayMode();
            Assert.IsTrue(TakeRecorder.IsLive);

            TakeRecorder.IsLive = false;
            yield return new EnterPlayMode();
            Assert.IsFalse(TakeRecorder.IsLive);
            yield return new ExitPlayMode();
            Assert.IsFalse(TakeRecorder.IsLive);
        }
        static void OnLog(string condition, string stackTrace, LogType type)
        {
            s_ReceivedLog = true;
        }
    }

    // These tests are failing intermittently (or freezing the editor) on Bokken because of a potential issue in AssetDatabase.
    // Disabling to unblock our ci pipeline but we will reevaluate from time to time.
#if UNITY_2020_3
    [UnityPlatform(exclude = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer})]
#endif
    public class TakeRecorderTestsUnstable : TakeRecorderTests
    {
        [Test]
        public void StartRecordingChangesRecordingState()
        {
            TakeRecorder.StartRecording();

            Assert.True(TakeRecorder.IsRecording(), "TakeRecorder should be recording");
        }

        [Test]
        public void StopRecordingRewindsContextTimeToInitialRecordingTime()
        {
            var initialRecordingTime = 5d;

            m_Context.duration = 10d;
            m_Context.SetTime(initialRecordingTime);

            TakeRecorder.StartRecording();

            m_Context.time = 7d;

            TakeRecorder.StopRecording();

            Assert.AreEqual(initialRecordingTime, m_Context.GetTime(), "Incorrect time.");
        }

        [Test]
        public void StopRecordingDoesNotProduceTakeWhenNoDeviceIsReady()
        {
            m_Device.impl.IsReady().Returns(false);

            TakeRecorder.StartRecording();
            TakeRecorder.StopRecording();

            var shot = TakeRecorder.Shot.GetValueOrDefault();

            Assert.IsNull(shot.Take, "A take was produced but no device was ready");
        }

        [UnityTest]
        public IEnumerator StopRecordingProducesTakeWhenLive()
        {
            TakeRecorder.StartRecording();
            TakeRecorder.StopRecording();

            yield return null;

            var shot = TakeRecorder.Shot.GetValueOrDefault();

            Assert.NotNull(shot.Take, "No take was produced but device was live");
        }

        [Test]
        public void DeviceWriteCalledAfterStopRecording()
        {
            TakeRecorder.StartRecording();
            TakeRecorder.StopRecording();

            m_Device.impl.Received(1).Write(Arg.Any<ITakeBuilder>());
        }

        [Test]
        public void DeviceWriteIsNotCalledAfterStopRecordingWhenNotReady()
        {
            m_Device.impl.IsReady().Returns(false);

            TakeRecorder.StartRecording();
            TakeRecorder.StopRecording();

            m_Device.impl.DidNotReceive().Write(Arg.Any<ITakeBuilder>());
        }

        [UnityTest]
        public IEnumerator StopsRecordingWhenTimeIsEqualToContextDuration()
        {
            m_Context.time = 0d;
            m_Context.duration = 10d;

            TakeRecorder.StartRecording();

            Assert.True(m_Context.IsPlaying(), "Preview is not playing.");
            Assert.True(TakeRecorder.IsRecording(), "TakeRecorder should be recording.");

            m_Context.time = 10d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(m_Context.IsPlaying(), "Preview is playing.");
            Assert.AreEqual(0d, m_Context.time, "Did not rewind after play.");
            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder should not be recording.");
        }

        [UnityTest]
        public IEnumerator StopsRecordingWhenTimeIsGreaterThanContextDuration()
        {
            m_Context.time = 0d;
            m_Context.duration = 10d;
            TakeRecorder.StartRecording();

            Assert.True(m_Context.IsPlaying(), "Context is not playing");

            m_Context.time = 11f;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(m_Context.IsPlaying(), "Preview is playing.");
            Assert.AreEqual(0d, m_Context.time, "Did not rewind after play.");
            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder should not be recording.");
        }

        [UnityTest]
        public IEnumerator RewindsToInitialRecordingTimeWhenRecordingAndPlaybackEnds()
        {
            var initialRecordingTime = 5d;
            m_Context.time = initialRecordingTime;
            m_Context.duration = 10d;
            TakeRecorder.StartRecording();

            Assert.True(m_Context.IsPlaying(), "Context is not playing");

            m_Context.time = 11f;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(m_Context.IsPlaying(), "Preview is playing.");
            Assert.AreEqual(initialRecordingTime, m_Context.time, "Did not rewind after play.");
            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder should not be recording.");
        }

        [UnityTest]
        public IEnumerator RewindsToInitialRecordingTimeWhenRecordingAndPlaybackEnds_IgnoresPlayTakeContents()
        {
            var initialRecordingTime = 5d;
            m_Context.time = initialRecordingTime;
            m_Context.duration = 10d;
            m_Context.shot.Take = m_CustomRangeTake;
            TakeRecorder.PlayTakeContents = true;
            TakeRecorder.StartRecording();

            Assert.True(m_Context.IsPlaying(), "Context is not playing");

            m_Context.time = 11f;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(m_Context.IsPlaying(), "Preview is playing.");
            Assert.AreEqual(initialRecordingTime, m_Context.time, "Did not rewind after play.");
            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder should not be recording.");
        }

        [UnityTest]
        public IEnumerator DoesNotStopRecordingWhenContextDurationIsZero()
        {
            m_Context.time = 0d;
            m_Context.duration = 0d;
            TakeRecorder.StartRecording();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(TakeRecorder.IsRecording(), "TakeRecorder should be recording");

            m_Context.time = 5d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(m_Context.IsPlaying(), "Preview is playing.");
            Assert.True(TakeRecorder.IsRecording(), "TakeRecorder should be recording");
            Assert.AreEqual(5d, m_Context.GetTime(), "Incorrect context time.");
        }

        [UnityTest]
        public IEnumerator RecordingWorksAfterPreviewing()
        {
            m_Context.duration = 10d;
            TakeRecorder.IsLive = false;
            TakeRecorder.PlayPreview();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            TakeRecorder.SetPreviewTime(0d);

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            TakeRecorder.IsLive = true;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            TakeRecorder.StartRecording();

            yield return TestUtils.WaitForPlayerLoopUpdates(3);

            Assert.True(TakeRecorder.IsRecording(), "Incorrect record state");
            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
        }

        [UnityTest]
        public IEnumerator TakeDurationMatchesTimelineDuration()
        {
            var duration = 10d;

            m_Context.time = 0d;
            m_Context.duration = duration;

            TakeRecorder.StartRecording();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            m_Context.time = duration;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            var shot = TakeRecorder.Shot.GetValueOrDefault();

            Assert.AreEqual(duration, shot.Take.Duration, "Take duration wasn't set to the timeline duration");
            Assert.AreEqual(duration, shot.Take.Timeline.duration, "Take duration wasn't set to the timeline duration");
        }
    }

    public class TakeRecorderTestsStable : TakeRecorderTests
    {
        [Test]
        public void CustomRangeTakeIsValid()
        {
            Assert.NotNull(m_CustomRangeTake, "Failed to load take asset");

            var result = m_CustomRangeTake.TryGetContentRange(out var takeStartTime, out var takeEndTime);

            Assert.IsTrue(result, "Failed to get content range");
            Assert.AreEqual(2d, takeStartTime, "Incorrect start range");
            Assert.AreEqual(7d, takeEndTime, "Incorrect end range");
        }

        [Test]
        public void IsLiveAfterSetup()
        {
            Assert.True(TakeRecorder.IsLive, "Is not live after the setup.");
        }

        [Test]
        public void SetContextWorks()
        {
            Assert.AreEqual(m_Context, TakeRecorderImpl.Instance.Context, "Incorrect context");
            Assert.AreEqual(m_Context.GetSelectedShot(), TakeRecorder.Shot, "Incorrect shot");
        }

        [Test]
        public void NotRecordingAfterLoad()
        {
            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder shouldn't be recording");
        }

        [Test]
        public void ShotIsNotNullAfterLoad()
        {
            Assert.NotNull(TakeRecorder.Shot, "Shot should not be null");
        }

        [Test]
        public void CantStartRecordingWhenNotLive()
        {
            TakeRecorder.IsLive = false;

            Assert.False(TakeRecorder.IsLive, "Is live");

            TakeRecorder.StartRecording();

            Assert.False(TakeRecorder.IsRecording(), "Is recording");
        }

        [UnityTest]
        public IEnumerator StopsRecordingWhenLiveSetToFalse()
        {
            TakeRecorder.StartRecording();

            Assert.True(TakeRecorder.IsRecording(), "Is not recording");

            TakeRecorder.IsLive = false;

            yield return null;

            Assert.False(TakeRecorder.IsRecording(), "Is recording");
        }

        [UnityTest]
        public IEnumerator SetLiveModeStopsAndRewindsContext()
        {
            m_Context.duration = 10d;
            TakeRecorder.IsLive = false;
            TakeRecorder.PlayPreview();
            m_Context.time = 5d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(5d, m_Context.GetTime(), "Incorrect time");
            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.True(m_Context.IsPlaying(), "Incorrect play state");

            TakeRecorder.IsLive = true;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(0d, m_Context.GetTime(), "Incorrect time");
            Assert.False(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.False(m_Context.IsPlaying(), "Incorrect play state");
        }

        [UnityTest]
        public IEnumerator SetLiveModeStopsAndRewindsContextToContentsBeginningWhenUsingPlayTakeContents()
        {
            m_CustomRangeTake.TryGetContentRange(out var takeStartTime, out var _);

            m_Context.duration = 10d;
            TakeRecorder.PlayTakeContents = true;
            TakeRecorder.IsLive = false;
            m_Context.time = 5d;
            m_Context.shot.Take = m_CustomRangeTake;

            TakeRecorder.PlayPreview();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(5d, m_Context.GetTime(), "Incorrect time");
            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.True(m_Context.IsPlaying(), "Incorrect play state");

            TakeRecorder.IsLive = true;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(takeStartTime, m_Context.GetTime(), "Incorrect time");
            Assert.False(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.False(m_Context.IsPlaying(), "Incorrect play state");
        }

        [UnityTest]
        public IEnumerator PositionRotationSeparatePlayback()
        {
            var prefab = Resources.Load<GameObject>("TakeRecorderPositionRotation/Playback");
            var instance = GameObject.Instantiate(prefab);
            var masterTimeline = instance.transform.Find("MasterTimeline").GetComponent<PlayableDirector>();
            var actor = instance.transform.Find("Virtual Camera Actor");

            var comparer = new Vector3EqualityComparer(0.001f);

            Assert.NotNull(masterTimeline, "Can't find master timeline");

            Assert.AreEqual(new Vector3(0f, 0f, 0f), actor.position, "Incorrect initial position");
            Assert.That(actor.localRotation.eulerAngles, Is.EqualTo(new Vector3(0f, 315f, 0f)).Using(comparer), "Incorrect initial Y rotation");

            masterTimeline.RebuildGraph();
            masterTimeline.DeferredEvaluate();

            Assert.AreEqual(0, masterTimeline.time, "Incorrect time");

            yield return null;

            Assert.AreEqual(new Vector3(4f, 5f, 6f), actor.position, "Position clip has not modified position");
            Assert.That(actor.localRotation.eulerAngles, Is.EqualTo(new Vector3(0f, 315f, 0f)).Using(comparer), "Position clip has modified Y rotation");

            masterTimeline.time = 6d;
            masterTimeline.DeferredEvaluate();

            yield return null;

            Assert.AreEqual(new Vector3(4f, 5f, 6f), actor.position, "Rotation clip has modified position");
            Assert.That(actor.localRotation.eulerAngles, Is.EqualTo(new Vector3(15f, 30f, 45f)).Using(comparer), "Rotation clip has not modified Y rotation");

            masterTimeline.time = 12d;
            masterTimeline.DeferredEvaluate();

            yield return null;

            Assert.AreEqual(new Vector3(8f, 9f, 10f), actor.position, "2nd position clip has not modified position");
            Assert.That(actor.localRotation.eulerAngles, Is.EqualTo(new Vector3(60f, 65f, 70f)).Using(comparer), "2nd rotation clip has not modified Y rotation");

            yield return null;

            GameObject.DestroyImmediate(instance);
        }

        [UnityTest]
        public IEnumerator StopsRecordingWhenDeviceIsNotReadyAnymore()
        {
            TakeRecorder.StartRecording();

            Assert.True(TakeRecorder.IsRecording(), "TakeRecorder should be recording");
            Assert.True(m_Device.IsReady(), "Incorrect ready state");

            m_Device.impl.IsReady().Returns(false);

            yield return null;

            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder should not be recording");
        }

        [UnityTest]
        public IEnumerator StopsRecordingWhenDeviceIsNotEnabledAnymore()
        {
            TakeRecorder.StartRecording();

            Assert.True(TakeRecorder.IsRecording(), "TakeRecorder should be recording");
            Assert.True(m_Device.IsReady(), "Incorrect ready state");
            Assert.True(m_Device.enabled, "Incorrect enabled state");

            m_Device.enabled = false;

            yield return null;

            Assert.False(TakeRecorder.IsRecording(), "TakeRecorder should not be recording");
        }

        [Test]
        public void StartRecordingFiresEvent()
        {
            var fired = false;

            TakeRecorder.RecordingStateChanged += () =>
            {
                fired = TakeRecorder.IsRecording();
            };

            TakeRecorder.StartRecording();

            Assert.True(fired, "Event did not fire.");
        }

        [Test]
        public void StopRecordingFiresEvent()
        {
            var fired = false;

            TakeRecorder.RecordingStateChanged += () =>
            {
                fired = !TakeRecorder.IsRecording();
            };

            TakeRecorder.StartRecording();
            TakeRecorder.StopRecording();

            Assert.True(fired, "Event did not fire.");
        }

        [Test]
        public void StartPlaybackFiresEvent()
        {
            var fired = false;

            TakeRecorder.PlaybackStateChanged += () =>
            {
                fired = TakeRecorder.IsPreviewPlaying();
            };

            TakeRecorder.PlayPreview();

            Assert.True(fired, "Event did not fire.");
        }

        [Test]
        public void StopPlaybackFiresEvent()
        {
            var fired = false;

            TakeRecorder.PlaybackStateChanged += () =>
            {
                fired = !TakeRecorder.IsPreviewPlaying();
            };

            TakeRecorder.PlayPreview();
            TakeRecorder.PausePreview();

            Assert.True(fired, "Event did not fire.");
        }

        [UnityTest]
        public IEnumerator PausePlaybackAndResumeDoesNotRewindTime()
        {
            // Long duration to avoid hitting the final frame
            m_Context.duration = float.MaxValue;

            TakeRecorder.IsLive = false;
            TakeRecorder.PlayPreview();

            m_Context.time = 5d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.True(m_Context.IsPlaying(), "Incorrect play state");

            // Pause will set the latest time request.
            TakeRecorder.PausePreview();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(5d, m_Context.GetTime(), "Time advanced while paused");
            Assert.False(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.False(m_Context.IsPlaying(), "Incorrect play state");

            TakeRecorder.PlayPreview();

            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.True(m_Context.IsPlaying(), "Incorrect play state");

            m_Context.time = 6d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(6d, m_Context.GetTime(), "Incorrect time");
            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect play state");
            Assert.True(m_Context.IsPlaying(), "Incorrect play state");
        }
    }
}
