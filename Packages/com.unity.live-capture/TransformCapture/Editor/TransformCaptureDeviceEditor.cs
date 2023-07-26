using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.TransformCapture
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(TransformCaptureDevice))]
    class TransformCaptureDeviceEditor : Editor
    {
        static class Contents
        {
            public static readonly GUIContent Actor = EditorGUIUtility.TrTextContent("Actor", "The actor currently assigned to this device.");
            public static readonly GUIContent AvatarMask = EditorGUIUtility.TrTextContent("Avatar Mask", "The Avatar Mask asset containing the list of active transforms to use.");
            public static readonly GUIContent CreateAvatarMask = EditorGUIUtility.TrTextContent("Create Avatar Mask", "Create an Avatar Mask from the current actor.");
            public static readonly GUIContent Recorder = EditorGUIUtility.TrTextContent("Keyframe Reduction", "Parameters to reduce redundant keyframes in the recorded animations. Higher values reduce the file size but might affect the curve accuracy.");
            public static readonly string MissingActorText = L10n.Tr("The device requires a reference to an Animator component in the Actor field.");
            public static readonly string MissingAvatarMaskText = L10n.Tr("Use an Avatar Mask asset in order to select which transforms to include.");
        }

        SerializedProperty m_Actor;
        SerializedProperty m_AvatarMask;
        SerializedProperty m_Recorder;

        void OnEnable()
        {
            m_Actor = serializedObject.FindProperty("m_Actor");
            m_AvatarMask = serializedObject.FindProperty("m_AvatarMask");
            m_Recorder = serializedObject.FindProperty("m_Recorder");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Actor, Contents.Actor);

            if (m_Actor.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(Contents.MissingActorText, MessageType.Warning);
            }

            EditorGUILayout.PropertyField(m_AvatarMask, Contents.AvatarMask);

            if (m_AvatarMask.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(Contents.MissingAvatarMaskText, MessageType.Warning);

                DoCreateAvatarMaskButton();
            }

            EditorGUILayout.PropertyField(m_Recorder, Contents.Recorder);

            serializedObject.ApplyModifiedProperties();
        }

        void DoCreateAvatarMaskButton()
        {
            var actor = m_Actor.objectReferenceValue as Animator;

            using var disabledScope = new EditorGUI.DisabledScope(actor == null);

            if (GUILayout.Button(Contents.CreateAvatarMask))
            {
                var avatarMask = TransformCaptureUtility.CreateAvatarMask(actor.transform);
                var directory = TransformCaptureUtility.GetActiveProjectPath();
                var name = $"{actor.gameObject.name} Mask";
                var extension = "mask";

                m_AvatarMask.objectReferenceValue =
                    TransformCaptureUtility.CreateAssetUsingFilePanel(avatarMask, name, extension, directory, Contents.CreateAvatarMask.text);
            }
        }
    }
}
