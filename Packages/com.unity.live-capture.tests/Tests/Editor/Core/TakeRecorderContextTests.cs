using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.LiveCapture.Tests.Editor
{
    class DirectorProvider : IDirectorProvider
    {
        public PlayableDirector Director { get; private set; }

        public DirectorProvider(PlayableDirector director)
        {
            Director = director;
        }
    }

    public class TakeRecorderContextTests
    {
        [TearDown]
        public void TearDown()
        {
            TakeRecorder.SetContext(null);
        }

        [Test]
        public void ContextSetTimeChecksBounds()
        {
            using var prefabScope = new PrefabScope("TakeRecorderContext/RootTrack01");

            var comparer = new FloatEqualityComparer(0.001f);
            var director = prefabScope.Root.GetComponent<PlayableDirector>();

            Assert.NotNull(director, "Invalid director");

            var context = new DirectorContext(new DirectorProvider(director));

            TakeRecorder.SetContext(context);

            Assert.AreEqual(context, TakeRecorder.Context, "Unexpected context set in TakeRecorder");

            var duration = context.GetDuration();

            Assert.That((float)duration, Is.EqualTo(5f).Using(comparer), "Incorrect context duration");

            context.SetTime(duration);

            Assert.That((float)context.GetTime(), Is.EqualTo(5f).Using(comparer), "Incorrect context duration");

            context.SetTime(100d);

            Assert.That((float)director.time, Is.EqualTo(5f).Using(comparer), "Incorrect director time");
            Assert.That((float)context.GetTime(), Is.EqualTo(5f).Using(comparer), "Incorrect context context");

            context.SetTime(-100d);

            Assert.That((float)director.time, Is.EqualTo(0f), "Incorrect director time");
            Assert.That((float)context.GetTime(), Is.EqualTo(0f), "Incorrect context time");
        }

        [Test]
        public void ContextRebuildTriggersPlayableGraphRebuild()
        {
            using var prefabScope = new PrefabScope("TakeRecorderContext/RootTrack01");

            var comparer = new FloatEqualityComparer(0.07f);
            var director = prefabScope.Root.GetComponent<PlayableDirector>();
            var context = new DirectorContext(new DirectorProvider(director));

            TakeRecorder.SetContext(context);

            Assert.NotNull(director, "Invalid director");

            director.RebuildGraph();

            var graph = director.playableGraph;

            Assert.True(graph.IsValid(), "Invalid PlayableGraph");

            context.Update();
            context.Rebuild();

            Assert.AreNotEqual(graph, director.playableGraph, "Incorrect playable graph");
        }

        [Test]
        public void NestedShotControlsRootDirector()
        {
            using var prefabScope = new PrefabScope("TakeRecorderContext/RootTrack02");

            var director = prefabScope.Root.GetComponent<PlayableDirector>();
            var context = new DirectorContext(new DirectorProvider(director));

            Assert.NotNull(director, "Invalid director");
            Assert.AreEqual(2, context.Shots.Length, "Incorrect amount of shots");
            Assert.AreNotEqual(director, context.GetResolver(0), "Invalid context resolver");
            Assert.AreNotEqual(context.GetResolver(0), context.GetResolver(1), "Contexts should not be equal");

            context.Selection = 1;
            context.SetTime(0f);

            Assert.AreEqual(5f, director.time, "Incorrect director time");
        }

        [Test]
        public void ContextUsesClipInAsTimeOffset()
        {
            using var prefabScope = new PrefabScope("TakeRecorderContext/ClipIn");

            var director = prefabScope.Root.GetComponent<PlayableDirector>();
            var context = new DirectorContext(new DirectorProvider(director));

            Assert.NotNull(director, "Invalid director");
            Assert.AreEqual(1, context.Shots.Length, "Incorrect shot count");
            Assert.AreEqual(5d, context.Shots[0].TimeOffset, "Incorrect time offset");
            
            var timeline = director.playableAsset as TimelineAsset;
            var track = timeline.GetOutputTrack(0) as TakeRecorderTrack;
            var clip = track.GetClips().ToArray()[0];

            Assert.AreEqual(5d, clip.clipIn, "Incorrect time offset");
        }
    } 
}