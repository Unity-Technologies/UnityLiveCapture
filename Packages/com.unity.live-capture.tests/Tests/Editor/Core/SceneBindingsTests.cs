using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    public class SceneBindingsTests
    {
        const string k_TmpPath = "Assets/Tmp";

        PlayableDirector m_Director;
        Take m_Take1;
        Take m_Take2;
        
        Animator m_Actor1;
        Animator m_Actor2;
        Animator m_Actor3;

        Animator CreateActor(string name)
        {
            var actor = new GameObject(name, typeof(Animator));

            return actor.GetComponent<Animator>();
        }

        Take CreateTake(Animator[] actors)
        {
            using var takeBuilder = new TakeBuilder(0d, 10d, 0d, 10d, 1, "shot", 1, string.Empty, k_TmpPath,
                System.DateTime.Now, null, StandardFrameRate.FPS_30_00, null, m_Director);

            using (TimelineDisableUndoScope.Create())
            {
                foreach (var actor in actors)
                {
                    takeBuilder.CreateAnimationTrack(actor.name, actor, new AnimationClip());
                }
            }

            return takeBuilder.Take;
        }

        [SetUp]
        public void Setup()
        {
            AssetDatabase.DeleteAsset(k_TmpPath);
            AssetDatabase.CreateFolder("Assets", "Tmp");

            // Create the Director
            var go = new GameObject("director", typeof(PlayableDirector));
            m_Director = go.GetComponent<PlayableDirector>();

            // Create the Actors
            m_Actor1 = CreateActor("actor1");
            m_Actor2 = CreateActor("actor2");
            m_Actor3 = CreateActor("actor3");

            // Create the Take
            m_Take1 = CreateTake(new[] { m_Actor1, m_Actor2 });
            m_Take2 = CreateTake(new[]{ m_Actor3 });
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Director.gameObject);
            GameObject.DestroyImmediate(m_Actor1.gameObject);
            GameObject.DestroyImmediate(m_Actor2.gameObject);
            GameObject.DestroyImmediate(m_Actor3.gameObject);
            AssetDatabase.DeleteAsset(k_TmpPath);
        }

        [Test]
        public void ClearAndSetSceneBindingsWhenTwoClipsShareTheSameTake()
        {
            // Create the main Timeline asset
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            var track = timeline.CreateTrack<TakeRecorderTrack>();
            track.CreateClip<ShotPlayableAsset>();
            track.CreateClip<ShotPlayableAsset>();
            var entries1 = m_Take1.BindingEntries.ToArray();
            var entries2 = m_Take2.BindingEntries.ToArray();

            m_Director.playableAsset = timeline;

            var context = new DirectorContext(new DirectorProvider(m_Director));

            context.SelectIndex(0);
            context.SetTake(m_Take1);

            context.SelectIndex(1);
            context.SetTake(m_Take1);

            // Setting a take in the context will prepare the track bindings.
            Assert.AreEqual(2, entries1.Length, "Incorrect number of binding entries");
            Assert.AreEqual(m_Actor1, m_Director.GetGenericBinding(entries1[0].Track), "Incorrect reference value");
            Assert.AreEqual(m_Actor2, m_Director.GetGenericBinding(entries1[1].Track), "Incorrect reference value");

            context.SelectIndex(0);
            context.SetTake(null);

            // context2 still references take1.
            Assert.AreEqual(m_Actor1, m_Director.GetGenericBinding(entries1[0].Track), "Incorrect reference value");
            Assert.AreEqual(m_Actor2, m_Director.GetGenericBinding(entries1[1].Track), "Incorrect reference value");

            context.SelectIndex(1);
            context.SetTake(null);

            // no context references m_Take1, so tracks do not have a generic binding.
            Assert.IsNull(m_Director.GetGenericBinding(entries1[0].Track), "Incorrect reference value");
            Assert.IsNull(m_Director.GetGenericBinding(entries1[1].Track), "Incorrect reference value");

            context.SetTake(m_Take2);

            // context2 sets new references from m_Take2,
            Assert.AreEqual(1, entries2.Length, "Incorrect number of binding entries");
            Assert.AreEqual(m_Actor3, m_Director.GetGenericBinding(entries2[0].Track), "Incorrect reference value");
            
            context.SelectIndex(0);
            context.SetTake(m_Take1);

            // m_Take1 bindings are restored through context1.
            Assert.AreEqual(m_Actor1, m_Director.GetGenericBinding(entries1[0].Track), "Incorrect reference value");
            Assert.AreEqual(m_Actor2, m_Director.GetGenericBinding(entries1[1].Track), "Incorrect reference value");

            using (TimelineDisableUndoScope.Create())
            {
                timeline.DeleteTrack(track);
            }

            ScriptableObject.DestroyImmediate(timeline);
        }
    }
}
