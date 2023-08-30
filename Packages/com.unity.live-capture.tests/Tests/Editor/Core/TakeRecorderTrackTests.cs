using System.Linq;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Timeline;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEditor;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TakeRecorderTrackTests
    {
        GameObject m_Root;
        PlayableDirector m_Director;

        [SetUp]
        public void Setup()
        {
            MasterTimelineContext.Instance.Selection = 0;
            TakeRecorder.SetContext(MasterTimelineContext.Instance);
            TakeRecorder.IsLive = false;

            LoadFromPrefab("TakeRecorderTrack/Root");
        }

        [TearDown]
        public void TearDown()
        {
            TakeRecorder.SetContext(null);
            Timeline.SetAsMasterDirector(null);
            MasterTimelineContext.Instance.Update();

            Unload();
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
            TakeRecorder.ClearSubscribers();

            if (m_Root != null)
                GameObject.DestroyImmediate(m_Root);
        }

        [Test]
        public void DirectorIsValidAfterLoad()
        {
            Assert.NotNull(m_Director, "PlayableDirector should not be null");
        }

        [Test]
        public void TimelineContextSetOnSetup()
        {
            Assert.AreEqual(MasterTimelineContext.Instance, TakeRecorder.Context, "Incorrect context");
        }

        [Test]
        public void TimelineContextFindsShots()
        {
            Assert.AreEqual(3, TakeRecorder.Context.Shots.Length, "Incorrect shot count");
        }

        [Test]
        public void SetPreviewTimeSetsTheDirectorTimeAndPauses()
        {
            TakeRecorder.Context.Selection = 0;
            TakeRecorder.PlayPreview();

            Assert.AreEqual(PlayState.Playing, m_Director.state, "Incorrect play state");

            TakeRecorder.SetPreviewTime(1d);

            Assert.AreEqual(1d, m_Director.time, "Incorrect time");
            Assert.AreEqual(PlayState.Paused, m_Director.state, "Incorrect play state");
        }

        [Test]
        public void CurrentShotGetsSelectedBasedOnContextSelection()
        {
            var shotAsset1 = TakeRecorder.Context.GetStorage(TakeRecorder.Context.Selection);

            Assert.AreEqual(0, TakeRecorder.Context.Selection, "Incorrect selection");

            TakeRecorder.Context.Selection = 1;
            
            var shotAsset2 = TakeRecorder.Context.GetStorage(TakeRecorder.Context.Selection);

            Assert.AreNotEqual(shotAsset1, shotAsset2, "Incorrect selected shot");
        }

        [Test]
        public void SetTimeChecksClipBounds()
        {
            var comparer = new FloatEqualityComparer(0.01f);

            m_Director.RebuildGraph();
            m_Director.Evaluate();

            var duration = TakeRecorder.GetPreviewDuration();

            TakeRecorder.SetPreviewTime(100d);

            m_Director.Evaluate();

            Assert.That((float)TakeRecorder.GetPreviewTime()
                , Is.EqualTo((float)duration).Using(comparer), "Time did not clamp to duration.");

            TakeRecorder.SetPreviewTime(-100d);

            m_Director.Evaluate();

            Assert.AreEqual(0d, TakeRecorder.GetPreviewTime(), "Incorrect slate");
        }

        [Test]
        public void RecursiveSetupDoesNotStackOverflow()
        {
            LoadFromPrefab("TakeRecorderTrack/Recursive");

            m_Director.RebuildGraph();

            Assert.IsTrue(m_Director.playableGraph.IsValid(), "Graph is not built");
        }

        [Test]
        public void InstantiatesMultipleTimelinesWithOutputs()
        {
            var timeline = m_Director.playableAsset as TimelineAsset;

            Assert.AreEqual(typeof(TakeRecorderTrack), timeline.GetOutputTrack(1).GetType(), "Incorrect track type");

            //Make sure the TakeRecorder is not needed;
            m_Director.ClearGenericBinding(timeline.GetOutputTrack(1));
            m_Director.RebuildGraph();

            var graph = m_Director.playableGraph;

            Assert.True(graph.IsValid(), "Invalid graph");
            Assert.AreEqual(1, graph.GetOutputCount(), "Incorrect number of outputs");
            Assert.AreEqual(5, graph.GetPlayableCount(), "Incorrect number of Playables");

            m_Director.Evaluate();

            Assert.AreEqual(3, graph.GetOutputCount(), "Incorrect number of outputs");
            Assert.AreEqual(19, graph.GetPlayableCount(), "Incorrect number of Playables");

            // Output 0
            Assert.True(graph.GetOutput(0).IsPlayableOutputOfType<ScriptPlayableOutput>(), "Incorrect output type");
            Assert.AreEqual(typeof(TimelinePlayable), graph.GetOutput(0).GetSourcePlayable().GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(typeof(TakeRecorderTrackMixer), graph.GetOutput(0).GetSourcePlayable()
                .GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(typeof(NestedTimelinePlayable), graph.GetOutput(0).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(typeof(TimelinePlayable), graph.GetOutput(0).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(typeof(NestedTimelinePlayable), graph.GetOutput(0).GetSourcePlayable()
                .GetInput(0).GetInput(1).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(typeof(TimelinePlayable), graph.GetOutput(0).GetSourcePlayable()
                .GetInput(0).GetInput(1).GetInput(0).GetPlayableType(), "Incorrect playable type");
            
            var handle1 = graph.GetOutput(0).GetSourcePlayable().GetInput(0).GetInput(0).GetInput(0).GetHandle();
            var handle2 = graph.GetOutput(0).GetSourcePlayable().GetInput(0).GetInput(1).GetInput(0).GetHandle();
            
            // Output 1 created from the first clip in the TakeRecorderTrack. Plays an AnimationTrack
            Assert.True(graph.GetOutput(1).IsPlayableOutputOfType<AnimationPlayableOutput>(), "Incorrect output type");
            Assert.AreEqual(typeof(TimelinePlayable), graph.GetOutput(1).GetSourcePlayable().GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(handle1, graph.GetOutput(1).GetSourcePlayable().GetHandle(), "Incorrect playable handle");
            Assert.AreEqual("AnimationMotionXToDeltaPlayable", graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetPlayableType().Name, "Incorrect playable type");
            Assert.AreEqual(typeof(AnimationLayerMixerPlayable), graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual("AnimationOffsetPlayable", graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetPlayableType().Name, "Incorrect playable type");
            Assert.AreEqual(typeof(AnimationMixerPlayable), graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(1, graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInputCount(), "Incorrect input count");
            Assert.AreEqual("AnimationOffsetPlayable", graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetPlayableType().Name, "Incorrect playable type");
            Assert.AreEqual(typeof(AnimationClipPlayable), graph.GetOutput(1).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");

            // Output 2 created from the second clip in the TakeRecorderTrack. Plays an AnimationTrack
            Assert.True(graph.GetOutput(2).IsPlayableOutputOfType<AnimationPlayableOutput>(), "Incorrect output type");
            Assert.AreEqual(typeof(TimelinePlayable), graph.GetOutput(2).GetSourcePlayable().GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(handle2, graph.GetOutput(2).GetSourcePlayable().GetHandle(), "Incorrect playable handle");
            Assert.AreEqual("AnimationMotionXToDeltaPlayable", graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetPlayableType().Name, "Incorrect playable type");
            Assert.AreEqual(typeof(AnimationLayerMixerPlayable), graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual("AnimationOffsetPlayable", graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetPlayableType().Name, "Incorrect playable type");
            Assert.AreEqual(typeof(AnimationMixerPlayable), graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
            Assert.AreEqual(1, graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInputCount(), "Incorrect input count");
            Assert.AreEqual("AnimationOffsetPlayable", graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetPlayableType().Name, "Incorrect playable type");
            Assert.AreEqual(typeof(AnimationClipPlayable), graph.GetOutput(2).GetSourcePlayable()
                .GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetInput(0).GetPlayableType(), "Incorrect playable type");
        }

        [Test]
        public void NestedTimelineOutputWeights()
        {
            m_Director.RebuildGraph();
            m_Director.Evaluate();

            var graph = m_Director.playableGraph;

            Assert.AreEqual(1f, graph.GetOutput(1).GetWeight(), "Incorrect output weight");
            Assert.AreEqual(0f, graph.GetOutput(2).GetWeight(), "Incorrect output weight");

            graph.Evaluate(5f);

            Assert.AreEqual(0f, graph.GetOutput(1).GetWeight(), "Incorrect output weight");
            Assert.AreEqual(1f, graph.GetOutput(2).GetWeight(), "Incorrect output weight");

            graph.Evaluate(5f);

            Assert.AreEqual(0f, graph.GetOutput(1).GetWeight(), "Incorrect output weight");
            Assert.AreEqual(0f, graph.GetOutput(2).GetWeight(), "Incorrect output weight");
        }

        [UnityTest]
        public IEnumerator ActorEvaluatesAfterChangingShotInTheSameFrame()
        {
            LoadFromPrefab("TakeRecorder/ActorEvaluateSetup");

            var masterTimeline = m_Root.transform.Find("MasterTimeline").GetComponent<PlayableDirector>();
            var actor = m_Root.transform.Find("Virtual Camera Actor");
            var device = m_Root.GetComponentInChildren<VirtualCameraDevice>();
            var client = Substitute.For<IVirtualCameraClient>();

            device.SetClient(client, false);

            Assert.False(TakeRecorder.IsLive, "TakeRecorder should start in playback mode");
            Assert.NotNull(masterTimeline, "Can't find master timeline");

            masterTimeline.RebuildGraph();
            masterTimeline.DeferredEvaluate();

            Assert.AreEqual(0, masterTimeline.time, "Incorrect time");

            yield return null;

            Assert.AreEqual(new Vector3(0f, 0f, -10f), actor.position, "Incorrect position");

            TakeRecorder.IsLive = true;

            Assert.True(TakeRecorder.IsLive, "Incorrect live state");
            Assert.True(device.IsReady(), "Incorrect ready state");

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(new Vector3(2f, 0f, -10f), actor.position, "Incorrect position");

            // Shot 1 ends at time = 5. Setting Shot 2.
            masterTimeline.time = 6d;
            masterTimeline.DeferredEvaluate();

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(new Vector3(2f, 0f, -10f), actor.position, "Actor did not evaluate after take swap");
        }

        [Test]
        public void TrackBindingsInitializedOnDemand()
        {
            LoadFromPrefab("TakeRecorderTrack/RootUninitializedTrackBindings");

            var timeline = m_Director.playableAsset as TimelineAsset;
            var takeRecorderTrack = timeline.GetOutputTracks()
                .OfType<TakeRecorderTrack>()
                .First();
            var shotAsset = takeRecorderTrack.GetClips().First().asset as ShotPlayableAsset;
            var take = shotAsset.Take;
            var takeTimeline = take.Timeline;
            var track = takeTimeline.GetOutputTrack(0);
            var bindingValue = m_Director.GetGenericBinding(track);

            Assert.IsNull(bindingValue, "Binding should be null");

            m_Director.RebuildGraph();
            m_Director.Evaluate();

            bindingValue = m_Director.GetGenericBinding(track);

            Assert.IsNotNull(bindingValue, "Binding should be set");
        }

        [Test]
        public void ActiveShotDoesNotPlaySelectedTakeWhileRecording()
        {
            Unload();

            var take = Resources.Load<Take>("TakeRecorderTrack/Takes/New Take");

            Assert.NotNull(take, "Take not found");

            var director = new GameObject("director", typeof(PlayableDirector)).GetComponent<PlayableDirector>();
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            var track = timeline.CreateTrack<TakeRecorderTrack>();
            var clip = track.CreateClip<ShotPlayableAsset>();
            var shot = clip.asset as ShotPlayableAsset;

            shot.Take = take;
            director.playableAsset = timeline;

            director.RebuildGraph();
            director.Evaluate();

            var graph = director.playableGraph;

            // Before the recording starts, the graph contains an animation output.
            Assert.AreEqual(2, graph.GetOutputCount(), "Incorrect number of outputs");
            Assert.AreEqual(10, graph.GetPlayableCount(), "Incorrect number of Playables");

            Timeline.SetAsMasterDirector(director);
            MasterTimelineContext.Instance.Update();
            TakeRecorderImpl.Instance.SkipDeviceReadyCheck = true;
            TakeRecorderImpl.Instance.SkipProducingAssets = true;
            TakeRecorder.IsLive = true;
            TakeRecorder.StartRecording();

            Assert.AreNotEqual(graph, director.playableGraph, "Graph did not rebuild after start recording");

            graph = director.playableGraph;

            // After the recording starts, the graph contains no animation output.
            Assert.AreEqual(1, graph.GetOutputCount(), "Incorrect number of outputs");
            Assert.AreEqual(3, graph.GetPlayableCount(), "Incorrect number of Playables");

            GameObject.DestroyImmediate(director.gameObject);
            GameObject.DestroyImmediate(timeline);

            TakeRecorderImpl.Instance.SkipDeviceReadyCheck = false;
            TakeRecorderImpl.Instance.SkipProducingAssets = false;
        }

        [Test]
        public void ActiveShotPlaysIterationBaseWhileRecording()
        {
            Unload();

            var take = Resources.Load<Take>("TakeRecorderTrack/Takes/New Take");

            Assert.NotNull(take, "Take not found");

            var director = new GameObject("director", typeof(PlayableDirector)).GetComponent<PlayableDirector>();
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            var track = timeline.CreateTrack<TakeRecorderTrack>();
            var clip = track.CreateClip<ShotPlayableAsset>();
            var shot = clip.asset as ShotPlayableAsset;

            shot.IterationBase = take;
            director.playableAsset = timeline;

            director.RebuildGraph();
            director.Evaluate();

            var graph = director.playableGraph;

            // Before the recording starts, the graph contains no animation output.
            Assert.AreEqual(1, graph.GetOutputCount(), "Incorrect number of outputs");
            Assert.AreEqual(3, graph.GetPlayableCount(), "Incorrect number of Playables");

            Timeline.SetAsMasterDirector(director);
            MasterTimelineContext.Instance.Update();
            TakeRecorderImpl.Instance.SkipDeviceReadyCheck = true;
            TakeRecorderImpl.Instance.SkipProducingAssets = true;
            TakeRecorder.IsLive = true;
            TakeRecorder.StartRecording();

            graph = director.playableGraph;

            // After recording starts, the graph contains an animation output.
            Assert.AreEqual(2, graph.GetOutputCount(), "Incorrect number of outputs");
            Assert.AreEqual(10, graph.GetPlayableCount(), "Incorrect number of Playables");

            GameObject.DestroyImmediate(director.gameObject);
            GameObject.DestroyImmediate(timeline);
            AssetDatabase.DeleteAsset(shot.Directory);

            TakeRecorderImpl.Instance.SkipDeviceReadyCheck = false;
            TakeRecorderImpl.Instance.SkipProducingAssets = false;
        }

        [Test]
        public void TimelineThatContainsNullPlayable_DoesNotThrow()
        {
            LoadFromPrefab("TakeRecorderTrack/RootNullPlayable");

            m_Director.RebuildGraph();

            Assert.DoesNotThrow(() => m_Director.Evaluate());
        }
    }
}