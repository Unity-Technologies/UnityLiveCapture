using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    [CustomPropertyDrawer(typeof(TrackBindingEntry), true)]
    class TrackBindingEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            var binding = property.FindPropertyRelative("m_Binding");

            EditorGUI.PropertyField(position, binding);
        }
    }
}
