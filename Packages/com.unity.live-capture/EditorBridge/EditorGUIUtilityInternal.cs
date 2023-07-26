using UnityEditor;

namespace Unity.LiveCapture.Editor.Internal
{
    static class EditorGUIUtilityInternal
    {
        public static float Indent => EditorGUI.indent;

        public static float CurrentViewWidth
        {
            get => EditorGUIUtility.currentViewWidth;
            set => EditorGUIUtility.currentViewWidth = value;
        }
    }
}
