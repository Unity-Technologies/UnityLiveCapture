using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Tests.Editor
{
    public class TimelineUtilityTests
    {
        [TearDown]
        public void TearDown()
        {
            var window = TimelineEditor.GetWindow();

            if (window != null)
                window.Close();
        }

        void ActivatePreviewMode(TimelineEditorWindow window)
        {
            // This should call TimelineWindow.RebuildGraphIfNecessary();
            window.SendEvent(new Event() { type = EventType.Repaint });
        }

        [Test]
        public void DirectorIsValidAfterLoad()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/Timeline01");

            var director = prefabScope.Root.GetComponent<PlayableDirector>();

            Assert.NotNull(director, "Invalid director");
        }

        [Test]
        public void TimelineImplReturnsTimelineEditorData()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/Timeline01");

            var director = prefabScope.Root.GetComponent<PlayableDirector>();
            var window = TimelineEditor.GetOrCreateWindow();

            Assert.Null(Timeline.MasterDirector, "Incorrect master director");

            window.SetTimeline(director);

            Assert.AreEqual(director, Timeline.MasterDirector, "Incorrect master director");
            Assert.AreEqual(director, Timeline.InspectedDirector, "Incorrect master director");
            Assert.AreEqual(director.playableAsset, Timeline.InspectedAsset, "Incorrect inspected asset");
            Assert.AreEqual(director.playableAsset, Timeline.MasterAsset, "Incorrect inspected asset");

            ActivatePreviewMode(window);

            Assert.True(Timeline.IsActive(), "Incorrect preview state");
        }

        PlayableDirector GetParentDirector(PlayableDirector director)
        {
            TimelineHierarchy.TryGetParentContext(director, out var parent, out var clip);

            return parent;
        }

        TimelineClip GetParentClip(PlayableDirector director)
        {
            TimelineHierarchy.TryGetParentContext(director, out var parent, out var clip);

            return clip;
        }

        [Test]
        public void TimelineHierarchyGetParentDirectorAndClip()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/TimelineHierarchy01");

            var rootTransform = prefabScope.Root.transform;
            var director0 = rootTransform.GetComponent<PlayableDirector>();
            var director1 = rootTransform.Find("Timeline01").GetComponent<PlayableDirector>();
            var director2 = rootTransform.Find("Timeline02").GetComponent<PlayableDirector>();
            var director3 = rootTransform.Find("Timeline03").GetComponent<PlayableDirector>();
            var director4 = rootTransform.Find("Timeline04").GetComponent<PlayableDirector>();
            var director5 = rootTransform.Find("Timeline05").GetComponent<PlayableDirector>();
            var timeline0 = director0.playableAsset as TimelineAsset;
            var timeline1 = director1.playableAsset as TimelineAsset;
            var timeline2 = director2.playableAsset as TimelineAsset;
            var timeline3 = director3.playableAsset as TimelineAsset;
            var timeline4 = director4.playableAsset as TimelineAsset;
            var timeline5 = director5.playableAsset as TimelineAsset;
            var clips0 = timeline0.GetOutputTrack(0).GetClips().ToArray();
            var clips2 = timeline2.GetOutputTrack(0).GetClips().ToArray();
            var clips4 = timeline4.GetOutputTrack(0).GetClips().ToArray();

            Assert.AreEqual(0d, director0.time, "Incorrect director time");

            director0.Evaluate();

            Assert.Null(GetParentDirector(director0), "Invalid parent");
            Assert.AreEqual(director0, GetParentDirector(director1), "Invalid parent");
            Assert.Null(GetParentDirector(director2), "Invalid parent");
            Assert.Null(GetParentDirector(director3), "Invalid parent");
            Assert.Null(GetParentDirector(director4), "Invalid parent");
            Assert.Null(GetParentDirector(director5), "Invalid parent");

            // Timeline1 is selected
            Assert.AreEqual(clips0[0], GetParentClip(director1), "Incorrect parent clip");

            // Timeline2 is not selected
            Assert.Null(GetParentClip(director2), "Incorrect parent clip");
            
            // Timeline3 is not selected, as it depends on Timeline2
            Assert.Null(GetParentClip(director3), "Incorrect parent clip");

            // Select Timeline2
            director0.time = clips0[1].start;
            director0.Evaluate();

            // Timeline2 is selected
            Assert.AreEqual(clips0[1], GetParentClip(director2), "Incorrect parent clip");

            // Select Timeline4
            director0.time = clips0[2].start;
            director0.Evaluate();

            // Timeline4 is selected
            Assert.AreEqual(clips0[2], GetParentClip(director4), "Incorrect parent clip");

            // Timeline5 is not selected
            Assert.Null(GetParentClip(director5), "Incorrect parent clip");
            
            //Select Timeline5
            director0.time = clips0[2].start + clips4[0].start;
            director0.Evaluate();

            // Timeline5 is selected
            Assert.AreEqual(clips4[0], GetParentClip(director5), "Incorrect parent clip");
                
        }

        [Test]
        public void TimelineHierarchyContextSetLocalTime()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/TimelineHierarchy01");

            var rootTransform = prefabScope.Root.transform;
            var director0 = rootTransform.GetComponent<PlayableDirector>();
            var director1 = rootTransform.Find("Timeline01").GetComponent<PlayableDirector>();
            var director2 = rootTransform.Find("Timeline02").GetComponent<PlayableDirector>();
            var director3 = rootTransform.Find("Timeline03").GetComponent<PlayableDirector>();
            var timeline0 = director0.playableAsset as TimelineAsset;
            var timeline2 = director2.playableAsset as TimelineAsset;
            var clips0 = timeline0.GetOutputTrack(0).GetClips().ToArray();
            var clips2 = timeline2.GetOutputTrack(0).GetClips().ToArray();

            var context = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                    new TimelineContext(director2, clips2[0]),
                    new TimelineContext(director0, clips0[1]),
                }
            );

            Assert.True(context.IsValid(), "Incorrect valid state");

            var rootDirector = context.GetRootDirector();

            Assert.False(rootDirector.playableGraph.IsValid(), "Incorrect graph valid state");
            Assert.AreEqual(director0, rootDirector, "Incorrect root director");
            Assert.AreEqual(0d, director0.time, "Incorrect director time");

            context.SetLocalTime(0d);

            Assert.True(rootDirector.playableGraph.IsValid(), "Incorrect graph valid state");
            Assert.AreEqual(5d, director0.time, "Incorrect director time");

            context.SetLocalTime(5d);

            Assert.True(rootDirector.playableGraph.IsValid(), "Incorrect graph valid state");
            Assert.AreEqual(10d, director0.time, "Incorrect director time");
            
            director0.RebuildGraph();
        }

        [Test]
        public void TimelineHierarchyContextCreate()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/TimelineHierarchy01");

            var rootTransform = prefabScope.Root.transform;
            var director0 = rootTransform.GetComponent<PlayableDirector>();
            var director1 = rootTransform.Find("Timeline01").GetComponent<PlayableDirector>();
            var director2 = rootTransform.Find("Timeline02").GetComponent<PlayableDirector>();
            var director3 = rootTransform.Find("Timeline03").GetComponent<PlayableDirector>();
            var timeline0 = director0.playableAsset as TimelineAsset;
            var timeline2 = director2.playableAsset as TimelineAsset;
            var clips0 = timeline0.GetOutputTrack(0).GetClips().ToArray();
            var clips2 = timeline2.GetOutputTrack(0).GetClips().ToArray();

            var context = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                    new TimelineContext(director2, clips2[0]),
                    new TimelineContext(director0, clips0[1]),
                }
            );

            // Activate Timeline3
            context.SetLocalTime(0d);
            context.GetRootDirector().Evaluate();

            var activeContext = TimelineHierarchyContextUtility.FromContext(new TimelineContext(director3));

            Assert.AreEqual(context, activeContext, "Incorrect hierarchy context");
        }

        [UnityTest]
        public IEnumerator TimelineHierarchyContextUsesDeferredEvaluate()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/TimelineHierarchy01");

            var rootTransform = prefabScope.Root.transform;
            var director0 = rootTransform.GetComponent<PlayableDirector>();
            var director1 = rootTransform.Find("Timeline01").GetComponent<PlayableDirector>();
            var director2 = rootTransform.Find("Timeline02").GetComponent<PlayableDirector>();
            var director3 = rootTransform.Find("Timeline03").GetComponent<PlayableDirector>();
            var timeline0 = director0.playableAsset as TimelineAsset;
            var timeline2 = director2.playableAsset as TimelineAsset;
            var clips0 = timeline0.GetOutputTrack(0).GetClips().ToArray();
            var clips2 = timeline2.GetOutputTrack(0).GetClips().ToArray();

            var context = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                    new TimelineContext(director2, clips2[0]),
                    new TimelineContext(director0, clips0[1]),
                }
            );

            var contextBeforeEvaluate = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                }
            );

            // Activate Timeline3
            context.SetLocalTime(0d);

            var activeContext = TimelineHierarchyContextUtility.FromContext(new TimelineContext(director3));

            // Director root evaluation did not happen yet, so we can't obtain the full hierarchy.
            Assert.AreEqual(contextBeforeEvaluate, activeContext, "Incorrect hierarchy context");

            yield return null;

            activeContext = TimelineHierarchyContextUtility.FromContext(new TimelineContext(director3));

            // Director root evaluation did happen, so we can obtain the full hierarchy.
            Assert.AreEqual(context, activeContext, "Incorrect hierarchy context");
        }

        [Test]
        public void TimelineHierarchyContextCreateFromTimelineNavigation()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/TimelineHierarchy01");

            var rootTransform = prefabScope.Root.transform;
            var director0 = rootTransform.GetComponent<PlayableDirector>();
            var director1 = rootTransform.Find("Timeline01").GetComponent<PlayableDirector>();
            var director2 = rootTransform.Find("Timeline02").GetComponent<PlayableDirector>();
            var director3 = rootTransform.Find("Timeline03").GetComponent<PlayableDirector>();
            var timeline0 = director0.playableAsset as TimelineAsset;
            var timeline2 = director2.playableAsset as TimelineAsset;
            var clips0 = timeline0.GetOutputTrack(0).GetClips().ToArray();
            var clips2 = timeline2.GetOutputTrack(0).GetClips().ToArray();

            var context = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                    new TimelineContext(director2, clips2[0]),
                    new TimelineContext(director0, clips0[1]),
                }
            );

            var window = TimelineEditor.GetOrCreateWindow();

            window.SetTimeline(director0);

            ActivatePreviewMode(window);

            var children0 = window.navigator.GetChildContexts().ToArray();

            window.navigator.NavigateTo(children0[1]);

            var children1 = window.navigator.GetChildContexts().ToArray();
            
            window.navigator.NavigateTo(children1[0]);

            var activeContext = Unity.LiveCapture.Editor
                .TimelineHierarchyContextUtility.FromTimelineNavigation();

            Assert.AreEqual(context, activeContext, "Incorrect hierarchy context");
        }

        [Test]
        public void SameNestedTimelineInDifferentParentClips()
        {
            using var prefabScope = new PrefabScope("TimelineUtility/TimelineHierarchy02");

            var rootTransform = prefabScope.Root.transform;
            var director0 = rootTransform.GetComponent<PlayableDirector>();
            var director1 = rootTransform.Find("Timeline01").GetComponent<PlayableDirector>();
            var director2 = rootTransform.Find("Timeline02").GetComponent<PlayableDirector>();
            var director3 = rootTransform.Find("Timeline03").GetComponent<PlayableDirector>();
            var timeline0 = director0.playableAsset as TimelineAsset;
            var timeline1 = director1.playableAsset as TimelineAsset;
            var timeline2 = director2.playableAsset as TimelineAsset;
            var timeline3 = director3.playableAsset as TimelineAsset;
            var clips0 = timeline0.GetOutputTrack(0).GetClips().ToArray();
            var clips1 = timeline1.GetOutputTrack(0).GetClips().ToArray();
            var clips2 = timeline2.GetOutputTrack(0).GetClips().ToArray();

            director0.RebuildGraph();
            director0.Evaluate();

            Assert.Null(GetParentDirector(director0), "Invalid parent");
            Assert.AreEqual(director0, GetParentDirector(director1), "Invalid parent");
            Assert.Null(GetParentDirector(director2), "Invalid parent");
            Assert.AreEqual(director1, GetParentDirector(director3), "Invalid parent");

            var context1 = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                    new TimelineContext(director1, clips1[0]),
                    new TimelineContext(director0, clips0[0]),
                }
            );
            var context2 = new TimelineHierarchyContext(new[] {
                    new TimelineContext(director3),
                    new TimelineContext(director2, clips2[0]),
                    new TimelineContext(director0, clips0[1]),
                }
            );

            Assert.True(context1.IsValid(), "Incorrect context valid state");
            Assert.True(context2.IsValid(), "Incorrect context valid state");

            var activeContext = TimelineHierarchyContextUtility.FromContext(new TimelineContext(director3));

            Assert.AreEqual(context1, activeContext, "Incorrect hierarchy context");

            director0.time = clips0[1].start;
            director0.Evaluate();

            activeContext = TimelineHierarchyContextUtility.FromContext(new TimelineContext(director3));

            Assert.AreEqual(context2, activeContext, "Incorrect hierarchy context");
        }
    } 
}
