#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    static class GUIUtils
    {
        public static void NextLine(ref Rect rect)
        {
            rect.y = rect.yMax + EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif
