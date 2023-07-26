using UnityEditor;
using Unity.LiveCapture.Cameras;

namespace Unity.LiveCapture.Cameras.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(TimecodeComponent))]
    class TimecodeComponentEditor : Editor
    {
        SerializedProperty m_Timecode;
        SerializedProperty m_FrameRate;

        void OnEnable()
        {
            m_Timecode = serializedObject.FindProperty("m_Timecode");
            m_FrameRate = serializedObject.FindProperty("m_FrameRate");
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Timecode);
            EditorGUILayout.PropertyField(m_FrameRate);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
