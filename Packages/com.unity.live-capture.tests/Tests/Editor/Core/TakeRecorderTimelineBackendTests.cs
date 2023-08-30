using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TakeRecorderTimelineBackendTests
    {
        GameObject m_Root;
        PlayableDirector m_Director;

        [SetUp]
        public void Setup()
        {
            TakeRecorder.IsLive = true;
            TakeRecorderImpl.Instance.SkipProducingAssets = true;
            TakeRecorderImpl.Instance.SkipDeviceReadyCheck = true;
            TakeRecorder.SetContext(MasterTimelineContext.Instance);

            LoadFromPrefab("TakeRecorderTrack/Root");
        }

        [TearDown]
        public void TearDown()
        {
            TakeRecorderImpl.Instance.SkipProducingAssets = false;
            TakeRecorderImpl.Instance.SkipDeviceReadyCheck = false;
            TakeRecorder.ClearSubscribers();
            TakeRecorder.SetContext(null);

            Unload();

            var window = TimelineEditor.GetWindow();

            if (window != null)
            {
                window.Close();
            }
        }

        void LoadFromPrefab(string path)
        {
            Unload();

            var prefab = Resources.Load<GameObject>(path);

            m_Root = GameObject.Instantiate(prefab);
            m_Director = m_Root.GetComponentInChildren<PlayableDirector>();

            Timeline.SetAsMasterDirector(m_Director);
            MasterTimelineContext.Instance.Update();
        }

        void Unload()
        {
            Timeline.SetAsMasterDirector(null);
            MasterTimelineContext.Instance.Update();

            if (m_Root != null)
                GameObject.DestroyImmediate(m_Root);
        }

        [Test]
        public void DirectorIsValidAfterLoad()
        {
            Assert.NotNull(m_Director, "PlayableDirector should not be null");
        }

        [Test]
        public void TakeRecorderSyncsWithTimelinePlay()
        {
            var context = MasterTimelineContext.Instance;

            Assert.False(context.IsPlaying(), "Incorrect playback state");

            Timeline.Play();

            Assert.True(context.IsPlaying(), "Incorrect playback state");
        }

        [UnityTest]
        public IEnumerator TimelinePlayTriggersTakeRecorderPlaybackChangeEvent()
        {
            var isPlaying = false;

            TakeRecorder.PlaybackStateChanged += () =>
            {
                isPlaying = TakeRecorder.IsPreviewPlaying();
            };

            Timeline.Play();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(isPlaying, "Incorrect playback state fired.");
        }

        [UnityTest]
        public IEnumerator TimelinePauseTriggersTakeRecorderPlaybackChangeEvent()
        {
            var isPlaying = false;

            TakeRecorder.PlaybackStateChanged += () =>
            {
                isPlaying = TakeRecorder.IsPreviewPlaying();
            };

            Timeline.Play();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(isPlaying, "Incorrect playback state fired.");

            Timeline.Pause();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(isPlaying, "Incorrect playback state fired.");
        }

        
        [UnityTest]
        public IEnumerator TimelinePauseInterruptsRecording()
        {
            var isRecording = false;

            TakeRecorder.RecordingStateChanged += () =>
            {
                isRecording = TakeRecorder.IsRecording();
            };

            TakeRecorder.StartRecording();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(isRecording, "Incorrect recording state fired.");
            Assert.True(TakeRecorder.IsRecording(), "Incorrect recording state.");

            Timeline.Pause();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(isRecording, "Incorrect recording state fired.");
            Assert.False(TakeRecorder.IsRecording(), "Incorrect recording state.");
        }

        [UnityTest]
        public IEnumerator TimelineUndloadInterruptsRecording()
        {
            var isRecording = false;

            TakeRecorder.RecordingStateChanged += () =>
            {
                isRecording = TakeRecorder.IsRecording();
            };

            TakeRecorder.StartRecording();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(isRecording, "Incorrect recording state fired.");
            Assert.True(TakeRecorder.IsRecording(), "Incorrect recording state.");

            m_Director.playableGraph.Destroy();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.False(isRecording, "Incorrect recording state fired.");
            Assert.False(TakeRecorder.IsRecording(), "Incorrect recording state.");
        }

        [UnityTest]
        public IEnumerator LastShotRewindsAfterPlay()
        {
            var context = MasterTimelineContext.Instance;

            Timeline.SetGlobalTime(0d);

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            context.Selection = context.Shots.Length - 1;

            Assert.True(Timeline.IsActive(), "Timeline should be in preview mode.");
            Assert.AreEqual(-15d, TakeRecorder.GetPreviewTime(), "Incorrect time.");
            
            m_Director.timeUpdateMode = DirectorUpdateMode.Manual;
            m_Director.RebuildGraph();
            m_Director.Evaluate();

            TakeRecorder.PlayPreview();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.True(TakeRecorder.IsPreviewPlaying(), "Incorrect playing state.");
            Assert.AreEqual(0d, TakeRecorder.GetPreviewTime(), "Incorrect time.");

            m_Director.time = m_Director.duration + 0.1d;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(15d, m_Director.time, "Incorrect director time.");
            Assert.AreEqual(0d, TakeRecorder.GetPreviewTime(), "Incorrect time.");
            Assert.False(TakeRecorder.IsPreviewPlaying(), "Incorrect playing state.");
        }
    }
}