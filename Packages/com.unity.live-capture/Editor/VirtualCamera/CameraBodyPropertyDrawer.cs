using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(CameraBody))]
    class CameraBodyPropertyDrawer : PropertyDrawer
    {
        internal static class Contents
        {
            public static GUIContent Iso = EditorGUIUtility.TrTextContent("ISO", "Set the sensibility of the real-world " +
                "camera sensor. Higher values increase the Camera's sensitivity to light and result in faster exposure times.");
            public static GUIContent ShutterSpeed = EditorGUIUtility.TrTextContent("Shutter Speed", "Sets the exposure time " +
                "in seconds for the camera. Lower values result in less exposed pictures.");
        }

        SerializedProperty m_SensorSizeProp;
        SerializedProperty m_IsoProp;
        SerializedProperty m_ShutterSpeedProp;

        void GetProperties(SerializedProperty property)
        {
            m_SensorSizeProp = property.FindPropertyRelative("m_SensorSize");
            m_IsoProp = property.FindPropertyRelative("m_Iso");
            m_ShutterSpeedProp = property.FindPropertyRelative("m_ShutterSpeed");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return EditorGUI.GetPropertyHeight(property);
            }
            else
            {
                return EditorGUIUtility.singleLineHeight + 2f;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                position.y += position.height + 2f;

                DoGUI(position, property);
            }
        }

        void DoGUI(Rect position, SerializedProperty property)
        {
            GetProperties(property);

            using (new EditorGUI.IndentLevelScope())
            {
                DoBodyGUI(position, property);
            }
        }

        void DoBodyGUI(Rect position, SerializedProperty property)
        {
            position.height = EditorGUI.GetPropertyHeight(m_SensorSizeProp);

            EditorGUI.PropertyField(position, m_SensorSizeProp);

            position.y += position.height + 2f;
            position.height = EditorGUI.GetPropertyHeight(m_IsoProp);

            EditorGUI.PropertyField(position, m_IsoProp, Contents.Iso);

            position.y += position.height + 2f;
            position.height = EditorGUI.GetPropertyHeight(m_ShutterSpeedProp);

            EditorGUI.PropertyField(position, m_ShutterSpeedProp, Contents.ShutterSpeed);
        }
    }
}
