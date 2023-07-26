using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    static class LensDrawerUtility
    {
        internal static class Contents
        {
            public static GUIContent LensLabel = EditorGUIUtility.TrTextContent("Lens", "The configuration values of the lens.");
            public static GUIContent FocalLength = EditorGUIUtility.TrTextContent("Focal Length", "Focal distance in millimeters.");
            public static GUIContent FocusDistance = EditorGUIUtility.TrTextContent("Focus Distance", "Focus distance in world units.");
            public static GUIContent Aperture = EditorGUIUtility.TrTextContent("Aperture", "Aperture of the lens in f-number.");
        }

        public static void DoLensGUI(SerializedProperty lensProperty, SerializedProperty intrinsicsProperty)
        {
            DoLensGUI(Contents.LensLabel, lensProperty, intrinsicsProperty);
        }

        public static void DoLensGUI(GUIContent label, SerializedProperty lensProperty, SerializedProperty intrinsicsProperty)
        {
            var focalLength = lensProperty.FindPropertyRelative("m_FocalLength");
            var focusDistance = lensProperty.FindPropertyRelative("m_FocusDistance");
            var aperture = lensProperty.FindPropertyRelative("m_Aperture");
            var focalLengthRange = intrinsicsProperty.FindPropertyRelative("m_FocalLengthRange").vector2Value;
            var closeFocusDistance = intrinsicsProperty.FindPropertyRelative("m_CloseFocusDistance").floatValue;
            var apertureRange = intrinsicsProperty.FindPropertyRelative("m_ApertureRange").vector2Value;

            lensProperty.isExpanded = EditorGUILayout.Foldout(lensProperty.isExpanded, label, true);

            if (lensProperty.isExpanded)
            {
                ++EditorGUI.indentLevel;

                DoSlider(Contents.FocalLength, focalLength, focalLengthRange);

                EditorGUILayout.PropertyField(focusDistance, Contents.FocusDistance);

                // Since we don't use a slider a validation step is needed.
                focusDistance.floatValue = Mathf.Clamp(
                    focusDistance.floatValue,
                    closeFocusDistance,
                    LensLimits.FocusDistance.y);

                DoSlider(Contents.Aperture, aperture, apertureRange);

                --EditorGUI.indentLevel;
            }
        }

        static void DoSlider(GUIContent label, SerializedProperty property, Vector2 range)
        {
            var min = range.x;
            var max = range.y;

            DoSlider(label, property, min, max);
        }

        static void DoSlider(GUIContent label, SerializedProperty property, float min, float max)
        {
            using (new EditorGUI.DisabledScope(min == max && !property.hasMultipleDifferentValues))
            {
                EditorGUILayout.Slider(property, min, max, label);
            }
        }
    }
}
