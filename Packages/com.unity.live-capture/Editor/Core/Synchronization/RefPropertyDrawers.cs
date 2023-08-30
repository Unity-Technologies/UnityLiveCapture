using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    [CustomPropertyDrawer(typeof(RegisteredRef<ITimecodeSource>))]
    sealed class RegisteredRefTimecodeSourcePropertyDrawer : RegisteredRefPropertyDrawer<ITimecodeSource>
    {
    }

    [CustomPropertyDrawer(typeof(RegisteredRef<ITimedDataSource>))]
    sealed class RegisteredRefTimedDataSourcePropertyDrawer : RegisteredRefPropertyDrawer<ITimedDataSource>
    {
    }

    [CustomPropertyDrawer(typeof(TimecodeSourceRef))]
    sealed class TimecodeSourceRefPropertyDrawer : RegisteredRefPropertyDrawer<ITimecodeSource>
    {
        protected override Registry<ITimecodeSource> DefaultRegistry => TimecodeSourceRef.Manager.Registry;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property.FindPropertyRelative("m_Reference"), label);
        }
    }

    [CustomPropertyDrawer(typeof(TimedDataSourceRef))]
    sealed class TimedDataSourceRefPropertyDrawer : RegisteredRefPropertyDrawer<ITimedDataSource>
    {
        protected override Registry<ITimedDataSource> DefaultRegistry => TimedDataSourceRef.Manager.Registry;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property.FindPropertyRelative("m_Reference"), label);
        }
    }
}
