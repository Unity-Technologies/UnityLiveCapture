using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomPropertyDrawer(typeof(SensorSizeAttribute))]
    class SensorSizePropertyDrawer : PropertyDrawer
    {
        internal static class Contents
        {
            public static GUIContent SensorPreset =
                EditorGUIUtility.TrTextContent("Sensor Preset", $"Select the sensor size from default presets or " +
                    $"custom presets defined in {nameof(SensorPresets)} assets in the project.");
            public static GUIContent SensorSize =
                EditorGUIUtility.TrTextContent("Sensor Size", "The size of the camera sensor in millimeters.");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) +
                EditorGUIUtility.singleLineHeight + 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            DoPresetSelector(position, property);

            position.y += EditorGUIUtility.singleLineHeight + 2f;
            position.height = EditorGUI.GetPropertyHeight(property, label);

            EditorGUI.PropertyField(position, property, Contents.SensorSize);
        }

        void DoPresetSelector(Rect position, SerializedProperty property)
        {
            var sensorSize = property.vector2Value;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var sensorSizes = SensorPresetsCache.GetSensorSizes();
                var index = Array.FindIndex(sensorSizes, (s) => s == sensorSize);
                var options = SensorPresetsCache.GetSensorNameContents();

                if (index == -1)
                {
                    options = SensorPresetsCache.GetSensorNameContentsWithCustom();
                    index = sensorSizes.Length;
                }

                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                index = EditorGUI.Popup(position, Contents.SensorPreset, index, options);

                if (check.changed && index >= 0 && index < sensorSizes.Length)
                {
                    property.vector2Value = sensorSizes[index];
                }
            }
        }
    }
}
