using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    [CustomPropertyDrawer(typeof(FrameTime))]
    class FrameNumberDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var prop = new EditorGUI.PropertyScope(position, label, property))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var frameProp = property.FindPropertyRelative("m_FrameNumber");
                var subframeProp = property.FindPropertyRelative("m_Subframe.m_Subframe");
                var subframeResolutionProp = property.FindPropertyRelative("m_Subframe.m_Resolution");

                var frameTime = new FrameTime(frameProp.intValue, new Subframe(subframeProp.intValue, subframeResolutionProp.intValue));
                var newFrameTime = DoField(position, prop.content, frameTime);

                if (change.changed)
                {
                    frameProp.intValue = newFrameTime.FrameNumber;
                    subframeProp.intValue = newFrameTime.Subframe.Value;
                    subframeResolutionProp.intValue = newFrameTime.Subframe.Resolution;
                }
            }
        }

        public static FrameTime DoField(Rect position, GUIContent label, FrameTime frameTime)
        {
            var newValue = EditorGUI.DoubleField(position, label, Math.Round((double)frameTime, 3));

            // exclude the subframe component if the value is very close to a integer number
            return Math.Abs(newValue % 1.0) <= 1e-10 ? FrameTime.FromFrameTime(newValue, 0) : FrameTime.FromFrameTime(newValue);
        }
    }
}
