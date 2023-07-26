using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ShotLibrary))]
    class ShotLibraryEditor : Editor
    {
        static class Contents
        {
            public static readonly GUIContent ShotsLabel = EditorGUIUtility.TrTextContent("Shots", "The list of available shots in the library.");
            public static readonly string CreateLens = EditorGUIUtility.TrTextContent("Create Lens").text;
        }

        CompactList m_List;
        SerializedProperty m_Shots;
        ShotLibrary m_ShotLibrary;
        ShotEditor m_ShotEditor = new ShotEditor();

        public int Index
        {
            get => m_List.Index;
            set => m_List.Index = value;
        }

        void OnEnable()
        {
            m_ShotLibrary = target as ShotLibrary;
            m_Shots = serializedObject.FindProperty("m_Shots");

            CreateList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(Contents.ShotsLabel);
            m_List.DoGUILayout();

            DoElementEditor();

            serializedObject.ApplyModifiedProperties();
        }

        void DoElementEditor()
        {
            var index = m_List.Index;

            if (index < 0 || index >= m_Shots.arraySize)
            {
                return;
            }

            var element = m_Shots.GetArrayElementAtIndex(index);
            var shot = From(m_Shots.GetArrayElementAtIndex(index));

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                shot = m_ShotEditor.OnGUI(shot, m_ShotLibrary.GetInstanceID());

                if (change.changed)
                {
                    To(shot, element);
                }
            }
        }

        Shot From(SerializedProperty property)
        {
            return new Shot()
            {
                TimeOffset = property.FindPropertyRelative("m_TimeOffset").doubleValue,
                Duration = property.FindPropertyRelative("m_Duration").doubleValue,
                Directory = property.FindPropertyRelative("m_Directory").stringValue,
                SceneNumber = property.FindPropertyRelative("m_SceneNumber").intValue,
                Name = property.FindPropertyRelative("m_Name").stringValue,
                TakeNumber = property.FindPropertyRelative("m_TakeNumber").intValue,
                Description = property.FindPropertyRelative("m_Description").stringValue,
                Take = property.FindPropertyRelative("m_Take").objectReferenceValue as Take,
                IterationBase = property.FindPropertyRelative("m_IterationBase").objectReferenceValue as Take,
            };
        }

        void To(Shot shot, SerializedProperty property)
        {
            property.FindPropertyRelative("m_TimeOffset").doubleValue = shot.TimeOffset;
            property.FindPropertyRelative("m_Duration").doubleValue = shot.Duration;
            property.FindPropertyRelative("m_Directory").stringValue = shot.Directory;
            property.FindPropertyRelative("m_SceneNumber").intValue = shot.SceneNumber;
            property.FindPropertyRelative("m_Name").stringValue = shot.Name;
            property.FindPropertyRelative("m_TakeNumber").intValue = shot.TakeNumber;
            property.FindPropertyRelative("m_Description").stringValue = shot.Description;
            property.FindPropertyRelative("m_Take").objectReferenceValue = shot.Take;
            property.FindPropertyRelative("m_IterationBase").objectReferenceValue = shot.IterationBase;
        }

        void CreateList()
        {
            m_List = new CompactList(m_Shots);
            m_List.OnCanAddCallback = () => true;
            m_List.OnCanRemoveCallback = () => true;
            m_List.Reorderable = true;
            m_List.ShowSearchBar = false;
            m_List.DrawListItemCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_Shots.GetArrayElementAtIndex(index);
                var name = element.FindPropertyRelative("m_Name");

                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, name.stringValue);
            };
            m_List.ElementHeightCallback = (int index) => 0f;
            m_List.DrawElementCallback = (Rect rect, int index) => { };
        }

        Shot GetSelectedShot()
        {
            var shot = default(Shot);
            var index = m_List.Index;

            if (index >= 0 && index < m_Shots.arraySize)
            {
                shot = m_ShotLibrary.GetShot(index);
            }

            return shot;
        }
    }
}
