using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A property drawer for enums. Does not block the main thread.
    /// </summary>
    [CustomPropertyDrawer(typeof(OnlyStandardFrameRatesAttribute))]
    class StandardFrameRatesPropertyDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var numeratorProp = property.FindPropertyRelative("m_Numerator");
            var denominatorProp = property.FindPropertyRelative("m_Denominator");
            var isDropFrameProp = property.FindPropertyRelative("m_IsDropFrame");

            if (numeratorProp == null || denominatorProp == null || isDropFrameProp == null)
            {
                base.OnGUI(position, property, label);
                return;
            }

            using (var prop = new EditorGUI.PropertyScope(position, label, property))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                if (label != GUIContent.none)
                {
                    position = EditorGUI.PrefixLabel(position, prop.content);
                }

                var frameRate = new FrameRate(numeratorProp.intValue, denominatorProp.intValue, isDropFrameProp.boolValue);

                if (!frameRate.TryGetStandardRate(out var standardRate))
                {
                    standardRate = StandardFrameRate.FPS_24_00;
                    GUI.changed = true;
                }

                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

                var newRate = (StandardFrameRate)EditorGUI.EnumPopup(position, standardRate);

                if (change.changed)
                {
                    frameRate = newRate.ToValue();
                    numeratorProp.intValue = frameRate.Numerator;
                    denominatorProp.intValue = frameRate.Denominator;
                    isDropFrameProp.boolValue = frameRate.IsDropFrame;
                }
            }
        }
    }

    /// <summary>
    /// A property drawer for enums. Does not block the main thread.
    /// </summary>
    [CustomPropertyDrawer(typeof(FrameRate))]
    class FrameRatePropertyDrawer : PropertyDrawer
    {
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

                var numeratorProp = property.FindPropertyRelative("m_Numerator");
                var denominatorProp = property.FindPropertyRelative("m_Denominator");
                var isDropFrameProp = property.FindPropertyRelative("m_IsDropFrame");

                const float dividerWidth = 12f;

                var dividerRect = new Rect(position)
                {
                    xMin = position.xMin + (position.width / 2) - (dividerWidth / 2),
                    xMax = position.xMin + (position.width / 2) + (dividerWidth / 2),
                };
                var numRect = new Rect(position)
                {
                    xMax = dividerRect.xMin,
                };
                var denRect = new Rect(position)
                {
                    xMin = dividerRect.xMax,
                };

                EditorGUI.showMixedValue = numeratorProp.hasMultipleDifferentValues;
                var newNumerator = Mathf.Max(EditorGUI.IntField(numRect, numeratorProp.intValue), 0);

                if (numeratorProp.intValue != newNumerator)
                {
                    numeratorProp.intValue = newNumerator;
                }

                EditorGUI.LabelField(dividerRect, " / ");

                EditorGUI.showMixedValue = denominatorProp.hasMultipleDifferentValues;
                var newDenominator = Mathf.Max(EditorGUI.IntField(denRect, denominatorProp.intValue), 1);

                if (denominatorProp.intValue != newDenominator)
                {
                    denominatorProp.intValue = newDenominator;
                }

                isDropFrameProp.boolValue = false;
            }
        }
    }
}
