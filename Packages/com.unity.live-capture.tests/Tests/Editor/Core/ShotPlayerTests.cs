using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.LiveCapture.Tests.Editor
{
    public class ShotPlayerTests
    {
        ShotPlayer m_ShotPlayer;
        ShotLibrary m_ShotLibrary;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("ShotPlayer", typeof(ShotPlayer));

            m_ShotPlayer = go.GetComponent<ShotPlayer>();
            m_ShotLibrary = Resources.Load<ShotLibrary>("ShotPlayer/ShotLibrary");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_ShotPlayer != null)
            {
                GameObject.DestroyImmediate(m_ShotPlayer.gameObject);
            }
        }

        [Test]
        public void RegistersToStaticInstancesList()
        {
            Assert.AreEqual(1, ShotPlayer.Instances.Count, "Incorrect count");
            Assert.True(ShotPlayer.Instances.Contains(m_ShotPlayer), "Failed to register");

            m_ShotPlayer.enabled = false;

            Assert.AreEqual(0, ShotPlayer.Instances.Count, "Incorrect count");
            Assert.False(ShotPlayer.Instances.Contains(m_ShotPlayer), "Failed to deregister");
        }

        [Test]
        public void StaticVersionChanges()
        {
            var version = ShotPlayer.Version;

            m_ShotPlayer.enabled = false;

            Assert.AreNotEqual(version, ShotPlayer.Version, "Incorrect version");
        }

        [UnityTest]
        public IEnumerator PreparesTakeOnUpdate()
        {
            var take = m_ShotLibrary.GetShot(0).Take;
            var timeline = take.Timeline;

            m_ShotPlayer.ShotLibrary = m_ShotLibrary;

            Assert.Null(m_ShotPlayer.Director.playableAsset, "Not null asset");

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.Null(m_ShotPlayer.Director.playableAsset, "Not null asset");

            m_ShotPlayer.Selection = 0;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            Assert.AreEqual(timeline, m_ShotPlayer.Director.playableAsset, "Incorrect asset");
        }

        [UnityTest]
        public IEnumerator TrackBindingsAreSet()
        {
            var actor = new GameObject("Actor", typeof(Animator));
            var animator = actor.GetComponent<Animator>();
            var director = m_ShotPlayer.Director;
            var take = m_ShotLibrary.GetShot(0).Take;
            var timeline = take.Timeline;
            var track = timeline.GetRootTrack(0);

            director.SetReferenceValue(new PropertyName("Virtual Camera Actor"), animator);

            m_ShotPlayer.ShotLibrary = m_ShotLibrary;
            m_ShotPlayer.Selection = 0;

            var binding = director.GetGenericBinding(track);

            Assert.Null(binding, "Incorrect track binding reference");

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            binding = director.GetGenericBinding(track);

            Assert.AreEqual(animator, binding, "Incorrect track binding reference");

            GameObject.DestroyImmediate(actor);
        }

        [UnityTest]
        public IEnumerator TrackBindingsAreCleared()
        {
            var actor = new GameObject("Actor", typeof(Animator));
            var animator = actor.GetComponent<Animator>();
            var director = m_ShotPlayer.Director;
            var take = m_ShotLibrary.GetShot(0).Take;
            var timeline = take.Timeline;
            var track = timeline.GetRootTrack(0);

            director.SetReferenceValue(new PropertyName("Virtual Camera Actor"), animator);

            m_ShotPlayer.ShotLibrary = m_ShotLibrary;
            m_ShotPlayer.Selection = 0;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            var binding = director.GetGenericBinding(track);

            Assert.AreEqual(animator, binding, "Incorrect track binding reference");

            m_ShotPlayer.Selection = -1;

            yield return TestUtils.WaitForPlayerLoopUpdates(1);

            binding = director.GetGenericBinding(track);

            Assert.Null(binding, "Incorrect track binding reference");            

            GameObject.DestroyImmediate(actor);
        }
    }
}
