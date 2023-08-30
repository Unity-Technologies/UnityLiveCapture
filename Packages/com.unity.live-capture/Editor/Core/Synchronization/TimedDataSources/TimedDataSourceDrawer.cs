using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor.TimedDataSources
{
    [CustomPropertyDrawer(typeof(TimedDataSource), true)]
    class TimedDataSourceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isSynchronized = property.FindPropertyRelative("m_IsSynchronized");
            var isSynchronizedLabel = new GUIContent(isSynchronized.displayName);

            using (var prop = new EditorGUI.PropertyScope(position, isSynchronizedLabel, isSynchronized))
            {
                EditorGUI.PropertyField(position, isSynchronized, prop.content);
            }
        }
    }
}
