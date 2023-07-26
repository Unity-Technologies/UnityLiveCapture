using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ShotPlayableAsset))]
    class ShotPlayableAssetEditor : Editor
    {
        static class Contents
        {
            public static readonly GUIContent AutoClipName = EditorGUIUtility.TrTextContent("Auto Clip Name", "Enable this option to let Unity manage the clip name, or disable it to manually name the clip.");
            public static readonly GUIContent Select = EditorGUIUtility.TrTextContent("Select In Take Recorder", "Select this Clip to keep it active in the Take Recorder.");
            public static readonly GUIStyle ButtonToggleStyle = "Button";

            public static readonly GUILayoutOption[] LargeButtonOptions =
            {
                GUILayout.Height(30f)
            };
        }

        Editor m_Editor;
        ShotPlayableAsset m_Asset;
        ShotEditor m_ShotEditor = new ShotEditor();
        TimelineHierarchyContext m_Hierarchy;
        SerializedProperty m_AutoClipName;
        SerializedProperty m_SceneNumber;
        SerializedProperty m_ShotName;
        SerializedProperty m_TakeNumber;
        SerializedProperty m_Description;
        SerializedProperty m_Directory;
        SerializedProperty m_Take;
        SerializedProperty m_IterationBase;

        void OnEnable()
        {
            m_AutoClipName = serializedObject.FindProperty("m_AutoClipName");
            m_SceneNumber = serializedObject.FindProperty("m_SceneNumber");
            m_ShotName = serializedObject.FindProperty("m_ShotName");
            m_TakeNumber = serializedObject.FindProperty("m_TakeNumber");
            m_Description = serializedObject.FindProperty("m_Description");
            m_Directory = serializedObject.FindProperty("m_Directory");
            m_Take = serializedObject.FindProperty("m_Take");
            m_IterationBase = serializedObject.FindProperty("m_IterationBase");

            m_Asset = target as ShotPlayableAsset;

            var timeline = Timeline.InspectedAsset;

            if (timeline != null && timeline.FindClip(m_Asset, out var clip))
            {
                m_Hierarchy = TimelineHierarchyContextUtility.FromTimelineNavigation();
            }
        }

        void OnDisable()
        {
            DestroyImmediate(m_Editor);
        }

        public override void OnInspectorGUI()
        {
            var index = -1;
            var context = MasterTimelineContext.Instance;

            serializedObject.Update();

            if (m_Hierarchy != null)
            {
                index = context.IndexOf(m_Hierarchy, m_Asset);

                DoSelectButton(index);
            }

            EditorGUILayout.PropertyField(m_AutoClipName, Contents.AutoClipName);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var shot = new Shot()
                {
                    SceneNumber = m_SceneNumber.intValue,
                    Name = m_ShotName.stringValue,
                    TakeNumber = m_TakeNumber.intValue,
                    Description = m_Description.stringValue,
                    Directory = m_Directory.stringValue,
                    Take = m_Take.objectReferenceValue as Take,
                    IterationBase = m_IterationBase.objectReferenceValue as Take
                };

                var newShot = m_ShotEditor.OnGUI(shot, m_Asset.GetInstanceID());

                if (change.changed)
                {
                    if (index == -1)
                    {
                        m_SceneNumber.intValue = newShot.SceneNumber;
                        m_ShotName.stringValue = newShot.Name;
                        m_TakeNumber.intValue = newShot.TakeNumber;
                        m_Description.stringValue = newShot.Description;
                        m_Directory.stringValue = newShot.Directory;
                        m_Take.objectReferenceValue = newShot.Take;
                        m_IterationBase.objectReferenceValue = newShot.IterationBase;
                    }
                    else
                    {
                        var resolver = context.GetResolver(index) as UnityEngine.Object;

                        if (resolver != null)
                        {
                            Undo.RegisterCompleteObjectUndo(resolver, "Inspector");
                        }

                        var storage = context.GetStorage(index);

                        if (storage != null)
                        {
                            Undo.RegisterCompleteObjectUndo(storage, "Inspector");
                            EditorUtility.SetDirty(storage);
                        }

                        context.SetShotAndBindings(index, newShot);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (index != -1)
            {
                TakeRecorderContextEditor.DrawBindingsInspector(context, ref m_Editor);
            }
        }

        void DoSelectButton(int index)
        {
            var context = TakeRecorder.Context;

            using (new EditorGUI.DisabledScope(context != MasterTimelineContext.Instance))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var value = GUILayout.Toggle(IsSelected(index), Contents.Select,
                    Contents.ButtonToggleStyle, Contents.LargeButtonOptions);

                if (change.changed && value)
                {
                    Debug.Assert(context == MasterTimelineContext.Instance);

                    context.Selection = index;

                    if (TakeRecorderWindow.Instance != null)
                    {
                        TakeRecorderWindow.Instance.SelectProvider<TimelineContextProvider>();
                    }

                    TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
                }
            }
        }

        bool IsSelected(int index)
        {
            var context = MasterTimelineContext.Instance;

            return TakeRecorder.Context == context && index == context.Selection;
        }
    }
}
