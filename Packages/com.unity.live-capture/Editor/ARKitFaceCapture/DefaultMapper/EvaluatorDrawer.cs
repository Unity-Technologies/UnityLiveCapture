using UnityEditor;
using UnityEngine;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper.Editor
{
    [CustomPropertyDrawer(typeof(SimpleEvaluator.Impl))]
    [CustomPropertyDrawer(typeof(CurveEvaluator.Impl))]
    class EvaluatorDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var values = property.GetValues<IEvaluator>();

            return values.Length > 1 ? EditorGUIUtility.singleLineHeight : values[0].GetHeight();
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var values = property.GetValues<IEvaluator>();

            if (values.Length > 1)
            {
                EditorGUI.HelpBox(position, "Multi-object editing not supported", MessageType.Info);
                return;
            }

            var value = values[0];

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                value.OnGUI(position);

                if (change.changed)
                {
                    property.SetValue(value);
                }
            }
        }
    }
}
