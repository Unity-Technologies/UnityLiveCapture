using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    class TimelineContextProvider : TakeRecorderContextProvider
    {
        const string k_DisplayName = "Timeline";

        static class Contents
        {
            public static readonly GUIContent NoTimeline = EditorGUIUtility.TrTextContent("None", $"Select a master Timeline to use for recording takes.");
        }

        public TimelineContextProvider() : base(k_DisplayName)
        {
        }

        public override ITakeRecorderContext GetContext()
        {
            return MasterTimelineContext.Instance;
        }

        public override void OnToolbarGUI(Rect rect)
        {
            DoDropdown(rect);
        }

        void DoDropdown(Rect rect)
        {
            var content = Contents.NoTimeline;
            var masterDirector = Timeline.MasterDirector;

            if (masterDirector != null && !masterDirector.TryGetComponent<ShotPlayer>(out var _))
            {
                content = new GUIContent(masterDirector.gameObject.name);
            }

            if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                var formatter = new UniqueNameFormatter();
                var mainStage = StageUtility.GetMainStage();
                var directors = mainStage.FindComponentsOfType<PlayableDirector>()
                    .Where(d => !d.TryGetComponent<ShotPlayer>(out var _))
                    .OrderBy(d => d.gameObject.name)
                    .ToList();

                for (var i = 0; i < directors.Count; ++i)
                {
                    var director = directors[i];
                    var name = formatter.Format(director.gameObject.name);

                    menu.AddItem(new GUIContent(name), director == masterDirector, OnSelect, director);
                }

                menu.ShowAsContext();
            }

            void OnSelect(object obj)
            {
                Timeline.SetAsMasterDirector(obj as PlayableDirector);
            }
        }
    }
}
