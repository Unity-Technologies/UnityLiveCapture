using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    public class ShotPlayableAssetTests
    {
        [Test]
        public void MigrationUpdatesShotNameAndIncreasesVersion()
        {
            var shotAsset = ScriptableObject.CreateInstance<ShotPlayableAsset>();
            var so = new SerializedObject(shotAsset);
            var version = so.FindProperty("m_Version");

            version.intValue = 0;

            so.ApplyModifiedProperties();

            var beforeMigrationName = "BeforeMigrationName";
            var afterMigrationName = "AfterMigrationName";

            shotAsset.ShotName = beforeMigrationName;

            Assert.AreEqual(beforeMigrationName, shotAsset.ShotName, "Incorrect shot name");

            shotAsset.Migrate(afterMigrationName);

            Assert.AreEqual(afterMigrationName, shotAsset.ShotName, "Incorrect shot name");

            so.Update();

            Assert.AreEqual(1, version.intValue, "Incorrect version after migration");

            shotAsset = ScriptableObject.CreateInstance<ShotPlayableAsset>();
        }


        [TestCase(true, "New Shot", "New Clip", ExpectedResult = "New Shot")]
        [TestCase(false, "New Shot", "New Clip", ExpectedResult = "New Clip")]
        public string AutoClipName(bool autoClipName, string shotName, string clipName)
        {
            var go = new GameObject("director", typeof(PlayableDirector));
            var director = go.GetComponent<PlayableDirector>();
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            var track = timeline.CreateTrack<TakeRecorderTrack>();
            var clip = track.CreateClip<ShotPlayableAsset>();
            var slateAsset = clip.asset as ShotPlayableAsset;

            try
            {
                slateAsset.AutoClipName = autoClipName;
                slateAsset.ShotName = shotName;
                clip.displayName = clipName;
                director.playableAsset = timeline;
                director.Evaluate();

                return clip.displayName;
            }
            finally
            {
                GameObject.DestroyImmediate(go);
                ScriptableObject.DestroyImmediate(slateAsset);
                ScriptableObject.DestroyImmediate(track);
                ScriptableObject.DestroyImmediate(timeline);
            }
        }
    }
}
