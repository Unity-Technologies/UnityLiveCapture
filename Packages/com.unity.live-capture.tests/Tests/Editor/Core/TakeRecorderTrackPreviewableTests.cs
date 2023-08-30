using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TakeRecorderTrackPreviewableTests
    {
        [RequireComponent(typeof(Animator))]
        class PreviewableActor : MonoBehaviour, IPreviewable
        {
            public void Register(IPropertyPreviewer previewer)
            {
                previewer.Register(this, "m_PreviewablePropertyName");
                previewer.Register(gameObject, "m_GameObjectPropertyName");
            }
        }

        const string k_TmpPath = "Assets/Tmp";

        PlayableDirector m_Director;
        Take m_Take;
        Take m_IterationBase;
        PreviewableActor m_Actor1;
        PreviewableActor m_Actor2;
        PreviewableActor m_Actor3;

        PreviewableActor CreateActor(string name, out Animator animator)
        {
            var actor = new GameObject(name, typeof(PreviewableActor));
            animator = actor.GetComponent<Animator>();

            return actor.GetComponent<PreviewableActor>();
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
            m_Actor1 = CreateActor("actor1", out var animator1);
            m_Actor2 = CreateActor("actor2", out var animator2);
            m_Actor3 = CreateActor("actor3", out var animator3);

            // Create the Take
            m_Take = CreateTake(new[] { animator1, animator2 });
            m_IterationBase = CreateTake(new[]{ animator3 });
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
        public void GatherPropertiesCallsIPreviewableRegister()
        {
            // Create the main Timeline asset
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            var track = timeline.CreateTrack<TakeRecorderTrack>();
            var clip = track.CreateClip<ShotPlayableAsset>();
            var shot = clip.asset as ShotPlayableAsset;

            shot.Take = m_Take;
            shot.IterationBase = m_IterationBase;
            m_Director.SetSceneBindings(m_Take.BindingEntries);
            m_Director.SetSceneBindings(m_IterationBase.BindingEntries);

            var driver = Substitute.For<IPropertyCollector>();

            timeline.GatherProperties(m_Director, driver);

            driver.Received().AddFromName(Arg.Is(m_Actor1), Arg.Is("m_PreviewablePropertyName"));
            driver.Received().AddFromName(Arg.Is(m_Actor1.gameObject), Arg.Is("m_GameObjectPropertyName"));

            driver.Received().AddFromName(Arg.Is(m_Actor2), Arg.Is("m_PreviewablePropertyName"));
            driver.Received().AddFromName(Arg.Is(m_Actor2.gameObject), Arg.Is("m_GameObjectPropertyName"));

            driver.Received().AddFromName(Arg.Is(m_Actor3), Arg.Is("m_PreviewablePropertyName"));
            driver.Received().AddFromName(Arg.Is(m_Actor3.gameObject), Arg.Is("m_GameObjectPropertyName"));

            // Cleanup
            ScriptableObject.DestroyImmediate(shot);
            ScriptableObject.DestroyImmediate(track);
            ScriptableObject.DestroyImmediate(timeline);
        }
    }
}
