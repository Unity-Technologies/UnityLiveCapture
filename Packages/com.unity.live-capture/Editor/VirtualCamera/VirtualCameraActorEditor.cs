using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VirtualCameraActor))]
    class VirtualCameraActorEditor : UnityEditor.Editor
    {
        static class Contents
        {
            public static readonly GUIContent LensIntrinsics = EditorGUIUtility.TrTextContent("Lens Intrinsics", "The intrinsic parameters of the lens.");
            public static readonly GUIContent CameraBody = EditorGUIUtility.TrTextContent("Camera Body", "The parameters of the camera's body.");
            public static readonly GUIContent DepthOfField = EditorGUIUtility.TrTextContent("Depth Of Field", "Depth of field enabled state.");
            public static readonly GUIContent CropAspect = EditorGUIUtility.TrTextContent("Crop Aspect", "The aspect ratio of the crop mask.");
        }

        SerializedProperty m_Lens;
        SerializedProperty m_LensIntrinsics;
        SerializedProperty m_CameraBody;
        SerializedProperty m_DepthOfField;
        SerializedProperty m_CropAspect;
        VirtualCameraActor m_Target;

        void OnEnable()
        {
            m_Lens = serializedObject.FindProperty("m_Lens");
            m_LensIntrinsics = serializedObject.FindProperty("m_LensIntrinsics");
            m_CameraBody = serializedObject.FindProperty("m_CameraBody");
            m_DepthOfField = serializedObject.FindProperty("m_DepthOfField");
            m_CropAspect = serializedObject.FindProperty("m_CropAspect");
            m_Target = target as VirtualCameraActor;
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(IsLinked()))
            {
                serializedObject.Update();

                LensDrawerUtility.DoLensGUI(m_Lens, m_LensIntrinsics);
                EditorGUILayout.PropertyField(m_LensIntrinsics, Contents.LensIntrinsics);
                EditorGUILayout.PropertyField(m_CameraBody, Contents.CameraBody);
                EditorGUILayout.PropertyField(m_DepthOfField, Contents.DepthOfField);
                EditorGUILayout.PropertyField(m_CropAspect, Contents.CropAspect);

                serializedObject.ApplyModifiedProperties();
            }
        }

        bool IsLinked()
        {
            foreach (var device in VirtualCameraDevice.instances)
            {
                if (device.IsLiveAndReady() && device.Actor == m_Target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
