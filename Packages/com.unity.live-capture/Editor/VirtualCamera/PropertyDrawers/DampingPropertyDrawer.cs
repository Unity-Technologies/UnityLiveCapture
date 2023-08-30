using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(Damping))]
    class DampingPropertyDrawer : PropertyDrawer
    {
        SerializedProperty m_Enabled;
        SerializedProperty m_Body;
        SerializedProperty m_Aim;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BeginLine(ref position);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                NextLine(ref position);
                DoGUI(position, property);
            }
        }

        void DoGUI(Rect position, SerializedProperty property)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                m_Enabled = property.FindPropertyRelative("Enabled");
                m_Body = property.FindPropertyRelative("Body");
                m_Aim = property.FindPropertyRelative("Aim");

                EditorGUI.PropertyField(position, m_Enabled);
                NextLine(ref position);

                using (new EditorGUI.DisabledGroupScope(!m_Enabled.boolValue))
                {
                    EditorGUI.PropertyField(position, m_Body);
                    position.height = EditorGUI.GetPropertyHeight(m_Body);
                    NextLine(ref position);

                    EditorGUI.PropertyField(position, m_Aim);
                    NextLine(ref position);
                }
            }
        }

        float GetLineHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        void BeginLine(ref Rect position)
        {
            position.height = GetLineHeight();
        }

        void NextLine(ref Rect position)
        {
            position.y += position.height + 2f;
        }
    }
}
