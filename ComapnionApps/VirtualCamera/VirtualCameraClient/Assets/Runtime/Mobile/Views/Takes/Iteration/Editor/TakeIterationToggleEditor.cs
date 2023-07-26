#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

namespace Unity.CompanionApps.VirtualCamera.Editor
{
    // We extend Toggle to keep MainView working with a collection of Toggles,
    // we then need a custom editor so that we can edit added properties...
    [CustomEditor(typeof(TakeIterationToggle), true)]
    class TakeIterationToggleEditor : ToggleEditor
    {
        SerializedProperty m_UnlockedSprite;
        SerializedProperty m_LockedSprite;
        SerializedProperty m_UnlockedColor;
        SerializedProperty m_LockedColor;
        SerializedProperty m_Icon;
        SerializedProperty m_Outline;
        SerializedProperty m_IsLocked;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_UnlockedSprite = serializedObject.FindProperty("m_UnlockedSprite");
            m_LockedSprite = serializedObject.FindProperty("m_LockedSprite");
            m_UnlockedColor = serializedObject.FindProperty("m_UnlockedColor");
            m_LockedColor = serializedObject.FindProperty("m_LockedColor");
            m_Icon = serializedObject.FindProperty("m_Icon");
            m_Outline = serializedObject.FindProperty("m_Outline");
            m_IsLocked = serializedObject.FindProperty("m_IsLocked");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Properties", EditorStyles.boldLabel);

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_UnlockedSprite);
            EditorGUILayout.PropertyField(m_LockedSprite);
            EditorGUILayout.PropertyField(m_UnlockedColor);
            EditorGUILayout.PropertyField(m_LockedColor);
            EditorGUILayout.PropertyField(m_Icon);
            EditorGUILayout.PropertyField(m_Outline);
            EditorGUILayout.PropertyField(m_IsLocked);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
