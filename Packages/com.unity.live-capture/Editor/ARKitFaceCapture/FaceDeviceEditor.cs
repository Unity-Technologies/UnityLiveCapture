using UnityEngine;
using UnityEditor;
using Unity.LiveCapture.CompanionApp.Editor;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.ARKitFaceCapture.Editor
{
    [CustomEditor(typeof(FaceDevice))]
    class FaceDeviceEditor : CompanionAppDeviceEditor<IFaceClient>
    {
        static class Contents
        {
            public static GUIContent Recorder = EditorGUIUtility.TrTextContent("Keyframe Reduction", "Parameters to reduce redundant keyframes in the recorded animations. Higher values reduce the file size but might affect the curve accuracy.");
            public static string MissingActorText = L10n.Tr("The device requires a Face Actor target.");
            public static string MissingClientText = L10n.Tr("The device requires a connected Client.");
            public static string ReadMoreText = L10n.Tr("read more");
            public static string ConnectClientURL = Documentation.baseURL + "setup-connecting" + Documentation.endURL;
            public static string SetupActorURL = Documentation.baseURL + "face-capture-getting-started" + Documentation.endURL;
        }

        static readonly string[] s_ExcludeProperties = { "m_Script", "m_Actor", "m_Channels", "m_Recorder" };

        FaceDevice m_Device;
        SerializedProperty m_Actor;
        SerializedProperty m_Channels;
        SerializedProperty m_Recorder;

        void OnEnable()
        {
            m_Device = target as FaceDevice;

            m_Actor = serializedObject.FindProperty("m_Actor");
            m_Channels = serializedObject.FindProperty("m_Channels");
            m_Recorder = serializedObject.FindProperty("m_Recorder");
        }

        protected override void OnDeviceGUI()
        {
            if (m_Device.GetClient() == null)
            {
                LiveCaptureGUI.HelpBoxWithURL(Contents.MissingClientText, Contents.ReadMoreText,
                    Contents.ConnectClientURL, MessageType.Warning);
            }

            serializedObject.Update();

            DoActorGUI(m_Actor);

            if (m_Actor.objectReferenceValue == null)
            {
                LiveCaptureGUI.HelpBoxWithURL(Contents.MissingActorText, Contents.ReadMoreText,
                    Contents.SetupActorURL, MessageType.Warning);
            }

            DoChannelsGUI(m_Channels);

            DrawPropertiesExcluding(serializedObject, s_ExcludeProperties);

            EditorGUILayout.PropertyField(m_Recorder, Contents.Recorder);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
