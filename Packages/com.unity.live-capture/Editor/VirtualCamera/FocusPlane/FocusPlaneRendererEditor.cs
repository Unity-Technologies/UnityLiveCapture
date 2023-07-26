using System;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomEditor(typeof(FocusPlaneRenderer))]
    [CanEditMultipleObjects]
    class FocusPlaneRendererEditor : UnityEditor.Editor
    {
        SerializedProperty m_CameraProp;
        SerializedProperty m_SettingsProp;

        void OnEnable()
        {
            m_CameraProp = serializedObject.FindProperty("m_Camera");
            m_SettingsProp = serializedObject.FindProperty("m_Settings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_CameraProp);
            EditorGUILayout.PropertyField(m_SettingsProp);

            serializedObject.ApplyModifiedProperties();

#if URP_14_0_OR_NEWER
            RenderFeatureEditor<FocusPlaneRenderer, VirtualCameraScriptableRenderFeature>.OnInspectorGUI();
#endif
        }
    }
}
