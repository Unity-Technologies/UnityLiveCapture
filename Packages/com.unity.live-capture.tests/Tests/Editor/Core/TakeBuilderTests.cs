using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using UnityEditor;
using NUnit.Framework;
using NSubstitute;

namespace Unity.LiveCapture.Tests.Editor
{
    class TestBinding : TakeBinding<AudioSource> {}
    class TestAsset : ScriptableObject {}
    class TestTrackMetadata : ITrackMetadata
    {
        public int data;
    }

    // These tests are failing intermittently (or freezing the editor) on Bokken because of a potential issue in AssetDatabase.
    // Disabling to unblock our ci pipeline but we will reevaluate from time to time.
    #if UNITY_2020_3
    [UnityPlatform(exclude = new[] {RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer})]
    #endif
    public class TakeBuilderTests
    {
        const string tmpDir = "Assets/tmp";

        TakeBuilder m_TakeBuilder;
        IExposedPropertyTable m_Resolver;
        IDisposable m_TimelineDisableUndoScope;

        [SetUp]
        public void Setup()
        {
            m_Resolver = Substitute.For<IExposedPropertyTable>();
            m_TakeBuilder = CreateTakeBuilder(null);
            m_TimelineDisableUndoScope = TimelineDisableUndoScope.Create();
        }

        TakeBuilder CreateTakeBuilder(Take iteration)
        {
            return CreateTakeBuilder(0d, iteration);
        }

        TakeBuilder CreateTakeBuilder(double duration, Take iteration)
        {
            return CreateTakeBuilder(duration, StandardFrameRate.FPS_30_00, iteration);
        }

        TakeBuilder CreateTakeBuilder(double contextDuration, FrameRate frameRate, Take iteration)
        {
            return CreateTakeBuilder(0d, contextDuration, 0d, contextDuration, frameRate, iteration);
        }

        TakeBuilder CreateTakeBuilder(double contextStartTime, double contextDuration, double recordingStartTime, double recordingDuration, FrameRate frameRate, Take iteration)
        {
            return new TakeBuilder(contextStartTime, contextDuration, recordingStartTime, recordingDuration, 1, "test", 1, "", tmpDir, DateTime.Now, iteration, frameRate, null, m_Resolver);
        }

        [TearDown]
        public void TearDown()
        {
            m_TimelineDisableUndoScope.Dispose();
            m_TakeBuilder.Dispose();

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            AssetDatabase.Refresh();
        }

        [Test]
        public void CreateBindingRegistersValueInResolver()
        {
            var name = "actor";
            var binding = m_TakeBuilder.CreateBinding<AnimatorTakeBinding>(name, null);

            m_Resolver.Received().SetReferenceValue(new PropertyName(name), null);
        }

        [Test]
        public void CreateTrackWithName()
        {
            var name = "track";
            var track = m_TakeBuilder.CreateTrack<AnimationTrack>(name);

            Assert.IsNotNull(track, "Track was not created");
            Assert.AreEqual(name, track.name, "Incorrect name");
        }

        [Test]
        public void CreateTrackWithBinding()
        {
            var name1 = "actor";
            var name2 = "track";
            var binding = m_TakeBuilder.CreateBinding<AnimatorTakeBinding>(name1, null);
            var track = m_TakeBuilder.CreateTrack<AnimationTrack>(name2);

            m_Resolver.Received().SetReferenceValue(new PropertyName(name1), null);
            Assert.IsNotNull(track, "Track was not created");
            Assert.AreEqual(name2, track.name, "Incorrect name");
        }

        [Test]
        public void GetTracksOfType()
        {
            var track1 = m_TakeBuilder.CreateTrack<AnimationTrack>("animation");
            var track2 = m_TakeBuilder.CreateTrack<ActivationTrack>("activation");
            var tracks = m_TakeBuilder.GetTracks<ActivationTrack>().ToArray();

            Assert.AreEqual(1, tracks.Length, "Incorrect track count");
            Assert.AreEqual(track2, tracks[0], "Incorrect result");
        }

        [Test]
        public void GetTracksWithBinding()
        {
            var binding = m_TakeBuilder.CreateBinding<AnimatorTakeBinding>("actor", null);
            var track1 = m_TakeBuilder.CreateTrack<AnimationTrack>("animation", binding);
            var track2 = m_TakeBuilder.CreateTrack<AnimationTrack>("animation2");
            var tracks = m_TakeBuilder.GetTracks<AnimationTrack>(binding).ToArray();

            Assert.AreEqual(1, tracks.Length, "Incorrect track count");
            Assert.AreEqual(track1, tracks[0], "Incorrect result");
        }

        [Test]
        public void ContentsDirectoryIsValid()
        {
            var assetsDir = Path.GetDirectoryName("Assets/asset.asset");

            Assert.True(m_TakeBuilder.ContentsDirectory.StartsWith(assetsDir), "Incorrect contents directory");
        }

        [Test]
        public void SaveAsAssetAcceptsAnimations()
        {
            var name = "clip";
            var clip = new AnimationClip();

            Assert.DoesNotThrow(() => m_TakeBuilder.SaveAsAsset(clip, name), "SaveAsAsset failed with exception");

            var path = $"{m_TakeBuilder.ContentsDirectory}/clip [Test] [001].anim";
            var loaded = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            Assert.AreEqual(clip, loaded, "Clips are different");
        }

        [Test]
        public void SaveAsAssetAcceptsScriptableObjects()
        {
            var name = "asset";
            var asset = ScriptableObject.CreateInstance<TestAsset>();

            Assert.DoesNotThrow(() => m_TakeBuilder.SaveAsAsset(asset, name), "SaveAsAsset failed with exception");

            var path = $"{m_TakeBuilder.ContentsDirectory}/asset [Test] [001].asset";
            var loaded = AssetDatabase.LoadAssetAtPath<TestAsset>(path);

            Assert.AreEqual(asset, loaded, "Assets are different");
        }

        [Test]
        public void SaveAsAssetWithoutNameThrows()
        {
            var clip = new AnimationClip();

            Assert.Throws<Exception>(() => m_TakeBuilder.SaveAsAsset(clip, null), "Expected exception.");
            Assert.Throws<Exception>(() => m_TakeBuilder.SaveAsAsset(clip, ""), "Expected exception.");

            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void SavingMultipleAssetsDoesNotThrow()
        {
            var clip = new AnimationClip();
            var clip2 = new AnimationClip();

            Assert.DoesNotThrow(() => m_TakeBuilder.SaveAsAsset(clip, "clip"));
            Assert.DoesNotThrow(() => m_TakeBuilder.SaveAsAsset(clip2, "clip2"));

            GameObject.DestroyImmediate(clip, true);
            GameObject.DestroyImmediate(clip2, true);
        }

        [Test]
        public void CreateAnimationTrackWithInvalidArgumentsThrows()
        {
            var animator = new GameObject("animator", typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();

            Assert.Throws<ArgumentException>(() => m_TakeBuilder.CreateAnimationTrack(null, animator, clip), "Expected exception.");
            Assert.Throws<ArgumentException>(() => m_TakeBuilder.CreateAnimationTrack("", animator, clip), "Expected exception.");
            Assert.Throws<ArgumentNullException>(() => m_TakeBuilder.CreateAnimationTrack("track", null, clip), "Expected exception.");
            Assert.Throws<ArgumentNullException>(() => m_TakeBuilder.CreateAnimationTrack("track", animator, null), "Expected exception.");

            GameObject.DestroyImmediate(animator.gameObject);
            GameObject.DestroyImmediate(clip);
        }

        [Test]
        public void CreateAnimationTrackWithoutIterationBaseCreatesTrack()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip);

            var take = m_TakeBuilder.Take;
            var timeline = take.Timeline;
            var entries = take.BindingEntries.ToArray();

            Assert.AreEqual(1, entries.Length, "Incorrect number of entries in take.");

            var entry = entries[0];
            var binding = entry.Binding;
            var track = entry.Track;
            var clips = track.GetClips().ToArray();

            Assert.AreEqual(1, clips.Length, "Incorrect number of clips in track.");

            var clipInTrack = (clips[0].asset as AnimationPlayableAsset).clip;

            Assert.AreEqual(clip, clipInTrack, "Incorrect clip in track.");

            m_Resolver.Received().SetReferenceValue(new PropertyName(name), animator);

            Assert.True(binding is AnimatorTakeBinding, "Incorrect binding");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void CreateAnimationTrackWithIterationBaseCreatesTrackOverride()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();
            var clip2 = new AnimationClip();

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip);

            var take = m_TakeBuilder.Take;
            var entries = take.BindingEntries.ToArray();

            Assert.AreEqual(1, entries.Length, "Incorrect number of entries in take.");

            m_TakeBuilder.Dispose();
            m_TakeBuilder = CreateTakeBuilder(take);

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip2);

            take = m_TakeBuilder.Take;
            var timeline = take.Timeline;
             entries = take.BindingEntries.ToArray();

            Assert.AreEqual(2, entries.Length, "Incorrect number of entries in take.");

            var entry = entries[0];
            var parent = entry.Track;
            var binding = entry.Binding;
            var track = parent.GetChildTracks().First();
            var clips = track.GetClips().ToArray();

            Assert.AreEqual(1, clips.Length, "Incorrect number of clips in track.");

            var clipInTrack = (clips[0].asset as AnimationPlayableAsset).clip;

            Assert.AreEqual(clip2, clipInTrack, "Incorrect clip in track.");

            m_Resolver.Received(2).SetReferenceValue(new PropertyName(name), animator);

            Assert.True(binding is AnimatorTakeBinding, "Incorrect binding");

            var clipAsset1 = parent.GetClips().First();
            var clipAsset2 = track.GetClips().First();

            Assert.AreEqual(clipAsset1.duration, clipAsset2.duration, "Override duration is different");

            Assert.True(entries.All((x) => x.Binding.PropertyName == binding.PropertyName), "Incorrect override binding");
            Assert.True(entries.GroupBy(x => x.Track).Count() == entries.Count(), "Incorrect override track");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void CreateAnimationTrackWithIterationBaseUsesStartTime()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();
            var clip2 = new AnimationClip();

            clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(1f, 0f)
            }));

            clip2.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(10f, 0f)
            }));

            Assert.Greater(clip2.length, clip.length, "Iteration clip should be longer than base clip");

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip);

            var take = m_TakeBuilder.Take;

            m_TakeBuilder.Dispose();

            var frameRate = StandardFrameRate.FPS_48_00.ToValue();
            var startTime = 10f;

            m_TakeBuilder = CreateTakeBuilder(startTime, 0f, 0f, 1f, frameRate, take);
            m_TakeBuilder.CreateAnimationTrack("test", animator, clip2);

            take = m_TakeBuilder.Take;
            var timeline = take.Timeline;
            var entries = take.BindingEntries.ToArray();
            var entry = entries[0];
            var parent = entry.Track;
            var binding = entry.Binding;
            var track = parent.GetChildTracks().First();
            var clips = track.GetClips().ToArray();
            var clipInTrack = (clips[0].asset as AnimationPlayableAsset).clip;
            var clipAsset1 = parent.GetClips().First();
            var clipAsset2 = track.GetClips().First();

            Assert.AreEqual(0f, clipAsset1.start, "Parent start time is incorrect");
            Assert.AreEqual(startTime, clipAsset2.start, "Override start time is incorrect");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void RatingResetsWhenCreatingIteration()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();
            var clip2 = new AnimationClip();

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip);

            var baseTake = m_TakeBuilder.Take;
            var baseRating = ++baseTake.Rating;
            baseTake.Duration = 10f;

            m_TakeBuilder.Dispose();
            m_TakeBuilder = CreateTakeBuilder(baseTake);

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip2);

            var iterationTake = m_TakeBuilder.Take;

            Assert.AreEqual(default(int), iterationTake.Rating, "Rating of iteration take wasn't reset");
            Assert.AreEqual(baseTake.Duration, iterationTake.Duration, "Different durations");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void CreateAnimationTrackWithTrackMetadata()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();
            var metadata = new TestTrackMetadata()
            {
                data = 5
            };

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip, metadata);

            var take = m_TakeBuilder.Take;
            var metadataEntries = take.MetadataEntries.ToArray();

            Assert.AreEqual(1, metadataEntries.Length, "Incorrect number of metadata entries");
            Assert.NotNull(metadataEntries[0].Track, "Invalid track set");
            Assert.AreEqual(metadata, metadataEntries[0].Metadata, "Incorrect metadata set");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void CreateAnimationTrackWithoutTrackMetadata()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var clip = new AnimationClip();

            m_TakeBuilder.CreateAnimationTrack("test", animator, clip);

            var take = m_TakeBuilder.Take;
            var metadataEntries = take.MetadataEntries.ToArray();

            Assert.AreEqual(0, metadataEntries.Length, "Incorrect number of metadata entries");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void CanAlignTimecodedTakes()
        {
            var frameRate = m_TakeBuilder.Take.FrameRate;
            var clipStart1 = Timecode.FromHMSF(frameRate, 1, 2, 4, 1, new Subframe(67, 100));
            var clipStart2 = Timecode.FromHMSF(frameRate, 1, 2, 4, 6, new Subframe(22, 100));

            var metadata = new TestTrackMetadata
            {
                data = 5
            };

            var animator1 = new GameObject("animator1", typeof(Animator)).GetComponent<Animator>();
            var animator2 = new GameObject("animator2", typeof(Animator)).GetComponent<Animator>();
            var animator3 = new GameObject("animator3", typeof(Animator)).GetComponent<Animator>();
            var clip1 = new AnimationClip();
            var clip2 = new AnimationClip();
            var clip3 = new AnimationClip();

            var clipStartSeconds1 = clipStart1.ToSeconds(frameRate);
            var clipStartSeconds2 = clipStart2.ToSeconds(frameRate);

            // Takes w/ timecodes
            m_TakeBuilder.CreateAnimationTrack("clip1", animator1, clip1, alignTime: clipStartSeconds1);
            m_TakeBuilder.CreateAnimationTrack("clip2", animator2, clip2, metadata, clipStartSeconds2);
            // Take w/o timecodes
            m_TakeBuilder.CreateAnimationTrack("clip3", animator3, clip3);
            m_TakeBuilder.AlignTracks(default);

            Assert.That(m_TakeBuilder.Take.StartTimecode, Is.EqualTo(clipStart1));

            var tracks = m_TakeBuilder.GetTracks<AnimationTrack>().ToList();
            Assert.That(tracks.Count, Is.EqualTo(3));

            var track1 = tracks.FirstOrDefault(t => t.name == "clip1");
            Assert.That(track1, Is.Not.Null);
            Assert.That(track1.GetClips().FirstOrDefault()?.start, Is.EqualTo(0));

            var track2 = tracks.FirstOrDefault(t => t.name == "clip2");
            Assert.That(track2, Is.Not.Null);
            Assert.That(track2.GetClips().FirstOrDefault()?.start, Is.EqualTo(clipStartSeconds2 - clipStartSeconds1));

            var track3 = tracks.FirstOrDefault(t => t.name == "clip3");
            Assert.That(track3, Is.Not.Null);
            Assert.That(track3.GetClips().FirstOrDefault()?.start, Is.EqualTo(0));
        }

        [Test]
        public void TimelineDurationIsFixedWhenDurationIsGreaterThanZero()
        {
            m_TakeBuilder.Dispose();
            m_TakeBuilder = CreateTakeBuilder(1d, null);

            var timeline = m_TakeBuilder.Take.Timeline;

            Assert.AreEqual(1d, timeline.duration, "Incorrect timeline duration");
            Assert.AreEqual(TimelineAsset.DurationMode.FixedLength, timeline.durationMode, "Incorrect duration mode");
        }

        [Test]
        public void TimelineDurationIsBasedOnClipsWhenDurationIsZero()
        {
            m_TakeBuilder.Dispose();
            m_TakeBuilder = CreateTakeBuilder(0d, null);

            var timeline = m_TakeBuilder.Take.Timeline;

            Assert.AreEqual(0d, timeline.duration, "Incorrect timeline duration");
            Assert.AreEqual(TimelineAsset.DurationMode.BasedOnClips, timeline.durationMode, "Incorrect duration mode");
        }

        [Test]
        public void TimelineFrameRateIsSet()
        {
            m_TakeBuilder.Dispose();

            var frameRate = StandardFrameRate.FPS_48_00.ToValue();

            m_TakeBuilder = CreateTakeBuilder(0d, frameRate, null);

            var timeline = m_TakeBuilder.Take.Timeline;

            Assert.AreEqual(frameRate.AsFloat(), timeline.editorSettings.frameRate, "Incorrect timeline frameRate");
        }

        [Test]
        public void ClipStartsAtContextStartTime()
        {
            m_TakeBuilder.Dispose();

            var contextStartTime = 10d;
            var contextDuration = 5d;
            var frameRate = StandardFrameRate.FPS_48_00.ToValue();
            var animator = new GameObject("animator", typeof(Animator)).GetComponent<Animator>();
            var animationClip = new AnimationClip();

            m_TakeBuilder = CreateTakeBuilder(contextStartTime, contextDuration, 0d, contextDuration, frameRate, null);
            m_TakeBuilder.CreateAnimationTrack("test", animator, animationClip);

            var take = m_TakeBuilder.Take;
            var timeline = take.Timeline;
            var entries = take.BindingEntries.ToArray();
            var entry = entries[0];
            var binding = entry.Binding;
            var track = entry.Track;
            var clips = track.GetClips().ToArray();
            var clip = clips[0];

            Assert.AreEqual(contextStartTime, clip.start, "Incorrect clip start time");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void ClipStartsAtContextStartTimeWhenIterating()
        {
            var name = "animator";
            var animator = new GameObject(name, typeof(Animator)).GetComponent<Animator>();
            var animationClip1 = new AnimationClip();
            var animationClip2 = new AnimationClip();

            m_TakeBuilder.CreateAnimationTrack("test", animator, animationClip1);

            var take = m_TakeBuilder.Take;
            var entries = take.BindingEntries.ToArray();

            Assert.AreEqual(1, entries.Length, "Incorrect number of entries in take.");

            m_TakeBuilder.Dispose();

            var contextStartTime = 10d;
            var contextDuration = 5d;
            var frameRate = StandardFrameRate.FPS_48_00.ToValue();

            m_TakeBuilder = CreateTakeBuilder(contextStartTime, contextDuration, 0d, contextDuration, frameRate, take);
            m_TakeBuilder.CreateAnimationTrack("test", animator, animationClip2);

            take = m_TakeBuilder.Take;
            var timeline = take.Timeline;
            entries = take.BindingEntries.ToArray();

            Assert.AreEqual(2, entries.Length, "Incorrect number of entries in take.");

            var entry = entries[0];
            var parent = entry.Track;
            var binding = entry.Binding;
            var track = parent.GetChildTracks().First();
            var clips = track.GetClips().ToArray();
            var clip = clips[0];

            Assert.AreEqual(contextStartTime, clip.start, "Incorrect clip start time");

            GameObject.DestroyImmediate(animator.gameObject);
        }

        [Test]
        public void ClipStartsAtRecordingStartTime()
        {
            m_TakeBuilder.Dispose();

            var contextStartTime = 10d;
            var contextDuration = 5d;
            var recordingStartTime = 1d;
            var recordingDuration = 2d;
            var frameRate = StandardFrameRate.FPS_48_00.ToValue();
            var animator = new GameObject("animator", typeof(Animator)).GetComponent<Animator>();
            var animationClip = new AnimationClip();

            m_TakeBuilder = CreateTakeBuilder(contextStartTime, contextDuration, recordingStartTime, recordingDuration, frameRate, null);
            m_TakeBuilder.CreateAnimationTrack("test", animator, animationClip);

            var take = m_TakeBuilder.Take;
            var timeline = take.Timeline;
            var entries = take.BindingEntries.ToArray();
            var entry = entries[0];
            var binding = entry.Binding;
            var track = entry.Track;
            var clips = track.GetClips().ToArray();
            var clip = clips[0];

            Assert.AreEqual(contextStartTime + recordingStartTime, clip.start, "Incorrect clip start time");

            GameObject.DestroyImmediate(animator.gameObject);
        }
    }
}
