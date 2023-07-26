using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(AspectRatioAttribute))]
    class AspectRatioPropertyDrawer : PropertyDrawer
    {
        static class Contents
        {
            public static GUIContent AspectRatioPreset =
                EditorGUIUtility.TrTextContent("Aspect Ratio Preset", "Select the aspect ratio from default presets or " +
                    $"custom presets defined in {nameof(SensorPresets)} assets in the project.");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            DoPresetSelector(position, property);

            position.y += EditorGUIUtility.singleLineHeight + 2f;

            EditorGUI.PropertyField(position, property, label);
        }

        void DoPresetSelector(Rect position, SerializedProperty property)
        {
            var aspectRatio = property.floatValue;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var aspectRatios = AspectRatioPresetsCache.GetAspectRatios();
                var index = Array.FindIndex(aspectRatios, (a) => a == aspectRatio);
                var options = AspectRatioPresetsCache.GetAspectRatioNameContents();

                if (index == -1)
                {
                    options = AspectRatioPresetsCache.GetAspectRatioNameContentsWithCustom();
                    index = aspectRatios.Length;
                }

                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                index = EditorGUI.Popup(position, Contents.AspectRatioPreset, index, options);

                if (check.changed && index >= 0 && index < aspectRatios.Length)
                {
                    property.floatValue = aspectRatios[index];
                }
            }
        }
    }
}
