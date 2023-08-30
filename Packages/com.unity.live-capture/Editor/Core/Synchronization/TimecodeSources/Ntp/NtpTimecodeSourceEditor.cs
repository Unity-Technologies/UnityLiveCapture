using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Ntp.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NtpTimecodeSource))]
    class NtpTimecodeSourceEditor : UnityEditor.Editor
    {
        static class Contents
        {
            public static readonly GUIContent ResynchronizeLabel = EditorGUIUtility.TrTextContent("Resynchronize", "Query the NTP server to resynchronize the time with the server clock.");
        }

        SerializedProperty m_FrameRate;
        SerializedProperty m_ServerAddress;

        void OnEnable()
        {
            m_FrameRate = serializedObject.FindProperty("m_FrameRate");
            m_ServerAddress = serializedObject.FindProperty("m_ServerAddress");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_FrameRate);
            EditorGUILayout.PropertyField(m_ServerAddress);

            EditorGUILayout.Space();

            if (GUILayout.Button(Contents.ResynchronizeLabel))
            {
                foreach (var target in targets.OfType<NtpTimecodeSource>())
                {
                    target.ForceUpdate();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
