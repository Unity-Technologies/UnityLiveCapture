using System;
using UnityEditor;
using UnityEngine;
using Unity.LiveCapture.Editor.Internal;

namespace Unity.LiveCapture.Editor
{
    class InspectorScope : IDisposable
    {
        bool m_Disposed;
        bool m_HierarchyMode;
        float m_LabelWidth;
        bool m_WideMode;

        public InspectorScope(float width, bool fullWidth = false)
        {
            var style = EditorStyles.inspectorDefaultMargins;

            if (fullWidth)
            {
                style = EditorStyles.inspectorFullWidthMargins;
            }

            m_HierarchyMode = EditorGUIUtility.hierarchyMode;
            m_LabelWidth = EditorGUIUtility.labelWidth;
            m_WideMode = EditorGUIUtility.wideMode;

            // EditorGUIUtility.labelWidth uses this formula then using hierarchyMode,
            // only that IMGUIContainer fails to setup a correct contextWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(width * 0.45f - 40, 120f);
            EditorGUIUtility.hierarchyMode = true;
            // Some custom Editors use EditorGUIUtility.CurrentViewWidth. This property returns the size
            // of the window currently rendering the gui. Editors assume the window is the InspectorWindow
            // and not a region of a window rendering through an IMGUIContainer. We can fix all the Editors
            // that use that property by setting it to the container's width.
            EditorGUIUtilityInternal.CurrentViewWidth = width;
            EditorGUILayout.BeginVertical(style);
            EditorGUIUtility.wideMode = true;
        }

        public void Dispose()
        {
            if (!m_Disposed)
            {
                EditorGUILayout.EndVertical();
                EditorGUIUtility.labelWidth = m_LabelWidth;
                EditorGUIUtility.hierarchyMode = m_HierarchyMode;
                EditorGUIUtility.wideMode = m_WideMode;
                EditorGUIUtilityInternal.CurrentViewWidth = -1f;

                m_Disposed = true;
            }
        }
    }
}
