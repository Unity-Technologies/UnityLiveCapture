using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(AnchorDeviceSettings))]
    class AnchorDeviceSettingsProperyDrawer : PropertyDrawer
    {
        const float kMarginY = 2f;

        SerializedProperty m_Enabled;
        SerializedProperty m_Target;

        SerializedProperty m_PositionOffset;
        SerializedProperty m_PositionLock;
        SerializedProperty m_RotationLock;
        SerializedProperty m_Damping;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ResolveProperties(property);

            if (property.isExpanded)
            {
                return GetLineHeight() +
                       EditorGUI.GetPropertyHeight(m_Enabled) +
                       EditorGUI.GetPropertyHeight(m_Target) +
                       EditorGUI.GetPropertyHeight(m_PositionOffset) +
                       EditorGUI.GetPropertyHeight(m_PositionLock) +
                       EditorGUI.GetPropertyHeight(m_RotationLock) +
                       EditorGUI.GetPropertyHeight(m_Damping) +
                       kMarginY * 6f;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
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
            ResolveProperties(property);

            using (new EditorGUI.IndentLevelScope())
            {
                PropertyField(ref position, m_Enabled, true);
                PropertyField(ref position, m_Target, true);
                PropertyField(ref position, m_PositionOffset, true);
                PropertyField(ref position, m_PositionLock, true);
                PropertyField(ref position, m_RotationLock, true);
                PropertyField(ref position, m_Damping, true);
            }
        }

        void ResolveProperties(SerializedProperty property)
        {
            m_Enabled = property.FindPropertyRelative("Enabled");
            m_Target = property.FindPropertyRelative("Target");

            m_PositionOffset = property.FindPropertyRelative("Settings.PositionOffset");
            m_PositionLock = property.FindPropertyRelative("Settings.PositionLock");
            m_RotationLock = property.FindPropertyRelative("Settings.RotationLock");
            m_Damping = property.FindPropertyRelative("Settings.Damping");
        }

        void PropertyField(ref Rect position, SerializedProperty property, bool getHeight = false)
        {
            if (getHeight)
            {
                position.height = EditorGUI.GetPropertyHeight(property);
            }
            else
            {
                position.height = GetLineHeight();
            }

            EditorGUI.PropertyField(position, property);
            NextLine(ref position);
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
            position.y += position.height + kMarginY;
        }
    }
}
