using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    class ShotEditor
    {
        const int k_MinSceneNumber = 1;
        const int k_MinTakeNumber = 1;

        static class Contents
        {
            public static readonly GUIContent FolderIcon = EditorGUIUtility.TrIconContent("Project", "Select a directory.");
            public static readonly GUILayoutOption ButtonSmall = GUILayout.Width(30f);
            public static readonly GUILayoutOption ButtonMid = GUILayout.Width(55f);
            public static readonly GUIContent CurrentBaseIcon = EditorGUIUtility.TrIconContent("Animation.Record", "Set as current Take iteration base.");
            public static readonly GUIContent SetBaseIcon = EditorGUIUtility.TrIconContent("Animation.Record", "Set the Take as iteration base.");
            public static readonly GUIContent IterationBaseLabel = EditorGUIUtility.TrTextContent("Iteration Base", "The Take used as iteration base.");
            public static readonly GUIContent ClearBaseLabel = EditorGUIUtility.TrTextContent("Clear", "Remove the Take used as iteration base.");
            public static readonly GUIContent SceneNumberLabel = EditorGUIUtility.TrTextContent("Scene Number", "The number associated with the scene to record.");
            public static readonly GUIContent ShotNameLabel = EditorGUIUtility.TrTextContent("Shot Name", "The name of the shot.");
            public static readonly GUIContent TakeNumberLabel = EditorGUIUtility.TrTextContent("Take Number", "The number associated with the take to record.");
            public static readonly GUIContent DescriptionLabel = EditorGUIUtility.TrTextContent("Description", "The description of the shot.");
            public static readonly GUIContent DirectoryLabel = EditorGUIUtility.TrTextContent("Directory", "The directory where the Takes are stored.");
            public static readonly GUIContent TakesLabel = EditorGUIUtility.TrTextContent("Takes", "The available Takes in the selected directory.");
            public static readonly GUIContent NoTakesLabel = EditorGUIUtility.TrTextContent("No takes available in directory", "No Takes available in the selected directory.");
            public static readonly GUIContent Take = EditorGUIUtility.TrTextContent("Take", "The Take to play.");
            public static GUIStyle TextAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
        }

        List<Take> m_Takes;
        CompactList m_TakeList;
        Shot m_Shot;
        bool m_IgnoreDelayedTextFieldChanges;

        public Shot OnGUI(Shot shot, int id = 0)
        {
            if (Event.current.type == EventType.Layout && m_Shot != shot)
            {
                RefreshCache(shot, id);

                // DelayedTextField will flush its data when the user presses the "return" key or
                // focus control id changes. If the user changes the shot selection while editing
                // a DelayedTextField, the new selected shot will receive the updated data from the field.
                // To avoid that, we ignore any changes until the next repaint event when the shot changes
                // externally.
                m_IgnoreDelayedTextFieldChanges = true;
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var sceneNumber = EditorGUILayout.IntField(Contents.SceneNumberLabel, m_Shot.SceneNumber);

                if (change.changed)
                {
                    m_Shot.SceneNumber = Mathf.Max(k_MinSceneNumber, sceneNumber);
                }
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var name = EditorGUILayout.DelayedTextField(Contents.ShotNameLabel, m_Shot.Name);

                if (change.changed && !string.IsNullOrEmpty(name))
                {
                    m_Shot.Name = FileNameFormatter.Instance.Format(name);
                }
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var takeNumber = EditorGUILayout.IntField(Contents.TakeNumberLabel, m_Shot.TakeNumber);

                if (change.changed)
                {
                    m_Shot.TakeNumber = Mathf.Max(k_MinTakeNumber, takeNumber);
                }
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField(Contents.DescriptionLabel, GUIContent.none);

                var description = EditorGUILayout.TextArea(m_Shot.Description,
                    Contents.TextAreaStyle,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f));

                if (change.changed)
                {
                    m_Shot.Description = description;
                }
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var path = DoDirectoryField(m_Shot.Directory);

                if (change.changed)
                {
                    m_Shot.Directory = path;

                    RefreshCache(m_Shot, id);
                }
            }

            if (m_TakeList != null)
            {
                EditorGUILayout.LabelField(Contents.TakesLabel);
                m_TakeList.DoGUILayout();
            }

            DoIterationBaseGUI();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(Contents.Take, m_Shot.Take, typeof(Take), false);
            }

            if (m_IgnoreDelayedTextFieldChanges)
            {
                // Ignore all events until the next repaint.
                if (Event.current.type == EventType.Repaint)
                {
                    m_IgnoreDelayedTextFieldChanges = false;
                }

                // Override the "changed" flag to false
                GUI.changed = false;

                // Return the unmodified input shot.
                return shot;
            }

            return m_Shot;
        }

        void DoIterationBaseGUI()
        {
            var iterationBase = m_Shot.IterationBase;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    iterationBase = EditorGUILayout.ObjectField(Contents.IterationBaseLabel,
                        iterationBase, typeof(Take), false) as Take;

                    if (change.changed)
                    {
                        m_Shot.IterationBase = iterationBase;
                    }
                }

                using (new EditorGUI.DisabledScope(iterationBase == null))
                {
                    if (GUILayout.Button(Contents.ClearBaseLabel, Contents.ButtonMid))
                    {
                        m_Shot.IterationBase = null;
                    }
                }
            }
        }

        string DoDirectoryField(string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                path = EditorGUILayout.DelayedTextField(Contents.DirectoryLabel, path);

                if (GUILayout.Button(Contents.FolderIcon, EditorStyles.miniButton, Contents.ButtonSmall))
                {
                    var newPath = EditorUtility.OpenFolderPanel(Contents.DirectoryLabel.text, path, string.Empty);
                    var assetsPath = Application.dataPath;

                    if (string.Compare(newPath, path) != 0 &&
                        newPath.StartsWith(assetsPath))
                    {
                        var projectPath = assetsPath.Substring(0, assetsPath.Length - "Assets/".Length + 1);

                        path = newPath.Substring(projectPath.Length);
                    }
                }
            }

            return path;
        }

        void RefreshCache(in Shot shot, int id)
        {
            m_Shot = shot;
            m_Takes = AssetDatabaseUtility.GetAssetsAtPath<Take>(shot.Directory);

            CreateTakeList(id);
        }

        void CreateTakeList(int id)
        {
            m_TakeList = new CompactList(m_Takes, $"{id}/takes");
            m_TakeList.OnCanAddCallback = () => false;
            m_TakeList.OnCanRemoveCallback = () => false;
            m_TakeList.Reorderable = false;
            m_TakeList.Searchable = false;
            m_TakeList.Index = m_Takes.IndexOf(m_Shot.Take);
            m_TakeList.DrawListItemCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_Takes[index] as Take;

                if (element == null)
                {
                    return;
                }

                rect.height = EditorGUIUtility.singleLineHeight;

                var selected = m_TakeList.Index == index;
                var isIterationBase = element == m_Shot.IterationBase;
                var buttonWidth = 30f;
                var rect1 = rect;
                var rect2 = rect;

                rect1.width -= buttonWidth - 5f;
                rect2.x = rect2.xMax - buttonWidth;
                rect2.width = buttonWidth;

                EditorGUI.LabelField(rect1, element.name);

                if (isIterationBase)
                {
                    DoSlectedBaseButtonGUI(rect2);
                }
                else if (selected)
                {
                    DoSetBaseButtonGUI(rect2, element);
                }
            };
            m_TakeList.DrawNoneListItemCallback = rect => EditorGUI.LabelField(rect, Contents.NoTakesLabel);

            m_TakeList.OnSelectCallback += OnSelectCallback;
        }

        void DoSetBaseButtonGUI(Rect rect, Take take)
        {
            if (GUI.Button(rect, Contents.SetBaseIcon))
            {
                m_Shot.IterationBase = take;
            }
        }

        void DoSlectedBaseButtonGUI(Rect rect)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                GUI.Button(rect, Contents.CurrentBaseIcon);
            }
        }

        void OnSelectCallback()
        {
            var newTake = m_Takes[m_TakeList.Index];

            if (m_Shot.Take != newTake)
            {
                m_Shot.Take = newTake;

                GUI.changed = true;
            }
        }
    }
}
