using System;
using UnityEditor;
using UnityEngine;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.Ltc.Editor
{
    [CustomEditor(typeof(LtcTimecodeSource))]
    class LtcSourceEditor : UnityEditor.Editor
    {
        static class Contents
        {
            public static readonly GUIContent DefaultDeviceLabel = EditorGUIUtility.TrTextContent("Default");
        }

        SerializedProperty m_FrameRate;
        SerializedProperty m_Device;
        SerializedProperty m_Channel;

        void OnEnable()
        {
            m_FrameRate = serializedObject.FindProperty("m_FrameRate");
            m_Device = serializedObject.FindProperty("m_Device");
            m_Channel = serializedObject.FindProperty("m_Channel");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_FrameRate);
            DoDeviceGUI();
            EditorGUILayout.PropertyField(m_Channel);

            serializedObject.ApplyModifiedProperties();
        }

        void DoDeviceGUI()
        {
            var currentDevice = m_Device.stringValue;

            var rect = EditorGUILayout.GetControlRect();
            var label = EditorGUIUtility.TrTextContent(m_Device.displayName);
            var currentOption = string.IsNullOrEmpty(currentDevice) ? Contents.DefaultDeviceLabel : new GUIContent(currentDevice);

            using (var prop = new EditorGUI.PropertyScope(rect, label, m_Device))
            {
                if (label != GUIContent.none)
                {
                    rect = EditorGUI.PrefixLabel(rect, prop.content);
                }

                EditorGUI.showMixedValue = m_Device.hasMultipleDifferentValues;

                if (GUI.Button(rect, currentOption, EditorStyles.popup))
                {
                    var devices = Microphone.devices;
                    var options = new GUIContent[devices.Length + 1];
                    options[0] = Contents.DefaultDeviceLabel;

                    for (var i = 0; i < devices.Length; i++)
                    {
                        options[i + 1] = new GUIContent(devices[i]);
                    }

                    var source = serializedObject.targetObject as LtcTimecodeSource;

                    OptionSelectWindow.SelectOption(rect, new Vector2(300f, 250f), options, (index, value) =>
                    {
                        source.SetDevice(index > 0 ? devices[index - 1] : string.Empty);
                    });
                }
            }
        }
    }
}
