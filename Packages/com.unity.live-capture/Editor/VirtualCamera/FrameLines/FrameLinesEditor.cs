using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomEditor(typeof(FrameLines))]
    [CanEditMultipleObjects]
    class FrameLinesEditor : UnityEditor.Editor
    {
        static class Contents
        {
            public static readonly GUIContent GateFitLabel = EditorGUIUtility.TrTextContent("Gate Fit", "The gate fit used by the camera.");
            public static readonly GUIContent GateMaskLabel = EditorGUIUtility.TrTextContent("Gate Mask", "Whether or not to show the sensor gate mask.");
            public static readonly GUIContent GateMaskOpacityLabel = EditorGUIUtility.TrTextContent("Opacity", "The opacity of the sensor gate mask.");
            public static readonly GUIContent AspectRatioLabel = EditorGUIUtility.TrTextContent("Aspect Ratio", "The aspect ratio of the crop.");
            public static readonly GUIContent ShowAspectRatioLabel = EditorGUIUtility.TrTextContent("Aspect Ratio Lines", "Whether or not to show the crop aspect ratio.");
            public static readonly GUIContent AspectModeLabel = EditorGUIUtility.TrTextContent("Type", "The render mode of the crop lines.");
            public static readonly GUIContent AspectLineColorLabel = EditorGUIUtility.TrTextContent("Color", "The color of the crop lines.");
            public static readonly GUIContent AspectLineWidthLabel = EditorGUIUtility.TrTextContent("Width", "The width of the crop lines.");
            public static readonly GUIContent AspectFillOpacityLabel = EditorGUIUtility.TrTextContent("Fill Opacity", "The opacity of the crop mask.");
            public static readonly GUIContent ShowCenterMarkerLabel = EditorGUIUtility.TrTextContent("Center Marker", "Whether or not to show the center marker.");
            public static readonly GUIContent CenterMarkerLabel = EditorGUIUtility.TrTextContent("Type", "The type of the center marker.");
        }

        SerializedProperty m_SettingsProp;
        SerializedProperty m_ShowGateMaskProp;
        SerializedProperty m_GateFitProp;
        SerializedProperty m_ShowAspectRatioLinesProp;
        SerializedProperty m_ShowCenterMarkerProp;
        SerializedProperty m_GateMaskOpacityProp;
        SerializedProperty m_AspectRatioProp;
        SerializedProperty m_AspectModeProp;
        SerializedProperty m_AspectLineColorProp;
        SerializedProperty m_AspectLineWidthProp;
        SerializedProperty m_AspectFillOpacityProp;
        SerializedProperty m_CenterMarkerProp;

        void OnEnable()
        {
            m_SettingsProp = serializedObject.FindProperty("m_Settings");
            m_GateFitProp = m_SettingsProp.FindPropertyRelative("GateFit");
            m_ShowGateMaskProp = m_SettingsProp.FindPropertyRelative("GateMaskEnabled");
            m_ShowAspectRatioLinesProp = m_SettingsProp.FindPropertyRelative("AspectRatioLinesEnabled");
            m_ShowCenterMarkerProp = m_SettingsProp.FindPropertyRelative("CenterMarkerEnabled");
            m_GateMaskOpacityProp = m_SettingsProp.FindPropertyRelative("GateMaskOpacity");
            m_AspectRatioProp = m_SettingsProp.FindPropertyRelative("AspectRatio");
            m_AspectModeProp = m_SettingsProp.FindPropertyRelative("AspectLineType");
            m_AspectLineColorProp = m_SettingsProp.FindPropertyRelative("AspectLineColor");
            m_AspectLineWidthProp = m_SettingsProp.FindPropertyRelative("AspectLineWidth");
            m_AspectFillOpacityProp = m_SettingsProp.FindPropertyRelative("AspectFillOpacity");
            m_CenterMarkerProp = m_SettingsProp.FindPropertyRelative("CenterMarkerType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_GateFitProp, Contents.GateFitLabel);
                EditorGUILayout.PropertyField(m_ShowGateMaskProp, Contents.GateMaskLabel);

                var showGateMask = m_ShowGateMaskProp.boolValue;
                using (new EditorGUI.DisabledScope(!showGateMask))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(m_GateMaskOpacityProp, Contents.GateMaskOpacityLabel);
                    }
                }

                EditorGUILayout.PropertyField(m_ShowAspectRatioLinesProp, Contents.ShowAspectRatioLabel);

                var showAspectRatio = m_ShowAspectRatioLinesProp.boolValue;
                using (new EditorGUI.DisabledScope(!showAspectRatio))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(m_AspectRatioProp, Contents.AspectRatioLabel);
                        EditorGUILayout.PropertyField(m_AspectModeProp, Contents.AspectModeLabel);
                        EditorGUILayout.PropertyField(m_AspectLineColorProp, Contents.AspectLineColorLabel);
                        EditorGUILayout.PropertyField(m_AspectLineWidthProp, Contents.AspectLineWidthLabel);

                        EditorGUILayout.PropertyField(m_AspectFillOpacityProp, Contents.AspectFillOpacityLabel);
                    }
                }

                EditorGUILayout.PropertyField(m_ShowCenterMarkerProp, Contents.ShowCenterMarkerLabel);

                var showCenterMarker = m_ShowCenterMarkerProp.boolValue;
                using (new EditorGUI.DisabledScope(!showCenterMarker))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(m_CenterMarkerProp, Contents.CenterMarkerLabel);
                    }
                }

                if (change.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

#if URP_14_0_OR_NEWER
            RenderFeatureEditor<FrameLines, VirtualCameraScriptableRenderFeature>.OnInspectorGUI();
#endif
        }
    }
}
