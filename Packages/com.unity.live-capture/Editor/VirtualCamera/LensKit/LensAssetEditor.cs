using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LensAsset))]
    class LensAssetEditor : Editor
    {
        static class Contents
        {
            public static readonly GUIContent Manufacturer = EditorGUIUtility.TrTextContent("Manufacturer", "The manufacturer of the lens.");
            public static readonly GUIContent Model = EditorGUIUtility.TrTextContent("Model", "The model of lens.");
            public static readonly GUIContent DefaultValues = EditorGUIUtility.TrTextContent("Default Values", "The default values of the lens.");
            public static readonly GUIContent LensIntrinsics = EditorGUIUtility.TrTextContent("Lens Intrinsics", "The intrinsic parameters of the lens.");
        }

        SerializedProperty m_Manufacturer;
        SerializedProperty m_Model;
        SerializedProperty m_DefaultValues;
        SerializedProperty m_Intrinsics;

        void OnEnable()
        {
            m_Manufacturer = serializedObject.FindProperty("m_Manufacturer");
            m_Model = serializedObject.FindProperty("m_Model");
            m_DefaultValues = serializedObject.FindProperty("m_DefaultValues");
            m_Intrinsics = serializedObject.FindProperty("m_Intrinsics");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Manufacturer, Contents.Manufacturer);
            EditorGUILayout.PropertyField(m_Model, Contents.Model);
            LensDrawerUtility.DoLensGUI(Contents.DefaultValues, m_DefaultValues, m_Intrinsics);
            EditorGUILayout.PropertyField(m_Intrinsics, Contents.LensIntrinsics);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
