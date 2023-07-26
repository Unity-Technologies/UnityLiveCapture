using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A property drawer for Timecode.
    /// </summary>
    [CustomPropertyDrawer(typeof(Timecode))]
    class TimecodePropertyDrawer : PropertyDrawer
    {
        static class Contents
        {
            public static readonly GUIContent Separator = new GUIContent(":");
            public static readonly GUIContent DropFrame = new GUIContent(";");
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var prop = new EditorGUI.PropertyScope(position, label, property))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                if (label != GUIContent.none)
                {
                    position = EditorGUI.PrefixLabel(position, prop.content);
                }

                var hours = property.FindPropertyRelative("m_Hours");
                var minutes = property.FindPropertyRelative("m_Minutes");
                var seconds = property.FindPropertyRelative("m_Seconds");
                var frames = property.FindPropertyRelative("m_Frames");
                var isDropFrame = property.FindPropertyRelative("m_IsDropFrame");

                var separatorWidth = 6f;
                var elementWidth = (position.width - separatorWidth * 3f) / 4f;
                var rect1 = new Rect(position) { width = elementWidth };
                var rect2 = new Rect(position) { x = rect1.xMax, width = separatorWidth };
                var rect3 = new Rect(rect1) { x = rect2.xMax, width = elementWidth };
                var rect4 = new Rect(position) { x = rect3.xMax, width = separatorWidth };
                var rect5 = new Rect(position) { x = rect4.xMax, width = elementWidth };
                var rect6 = new Rect(position) { x = rect5.xMax, width = separatorWidth };
                var rect7 = new Rect(position) { x = rect6.xMax, width = elementWidth };

                EditorGUI.PropertyField(rect1, hours, GUIContent.none);
                EditorGUI.LabelField(rect2, Contents.Separator);
                EditorGUI.PropertyField(rect3, minutes, GUIContent.none);
                EditorGUI.LabelField(rect4, Contents.Separator);
                EditorGUI.PropertyField(rect5, seconds, GUIContent.none);
                EditorGUI.LabelField(rect6, isDropFrame.boolValue ? Contents.DropFrame : Contents.Separator);
                EditorGUI.PropertyField(rect7, frames, GUIContent.none);

                if (change.changed)
                {
                    if (hours.intValue < 0) hours.intValue = 0;
                    if (minutes.intValue < 0) minutes.intValue = 0;
                    if (seconds.intValue < 0) seconds.intValue = 0;
                    if (frames.intValue < 0) frames.intValue = 0;
                }
            }
        }
    }
}
