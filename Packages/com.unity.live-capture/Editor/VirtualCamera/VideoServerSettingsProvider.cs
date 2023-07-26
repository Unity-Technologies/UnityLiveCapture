using System;
using System.Collections.Generic;
using System.Linq;
using Unity.LiveCapture.Editor;
using Unity.LiveCapture.VideoStreaming.Server;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    class VideoServerSettingsProvider : SettingsProvider
    {
        const string k_SettingsMenuPath = "Preferences/Live Capture/Virtual Camera/Video Server";

        static class Contents
        {
            public static readonly GUIContent HelpMenuIcon = EditorGUIUtility.IconContent("_Help");
            public static readonly GUIContent SettingMenuIcon = EditorGUIUtility.IconContent("_Popup");
            public static readonly GUIContent ResetLabel = EditorGUIUtility.TrTextContent("Reset", "Reset to default.");
            public static readonly GUIContent EncoderLabel = EditorGUIUtility.TrTextContent("Encoder", "The preferred encoder to use for video streaming.");
        }

        SerializedObject m_SerializedObject;
        SerializedProperty m_Encoder;
        SerializedProperty m_ResolutionScale;
        SerializedProperty m_FrameRateProp;
        SerializedProperty m_QualityProp;
        SerializedProperty m_PrioritizeLatencyProp;

        VideoEncoder[] m_EncodersSupportedOnPlatform;
        GUIContent[] m_EncoderOptions;
        EncoderSupport m_EncoderSupport;

        /// <summary>
        /// Open the settings in the User Preferences.
        /// </summary>
        public static void Open()
        {
            SettingsService.OpenUserPreferences(k_SettingsMenuPath);
        }

        public VideoServerSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            InitializeWithCurrentSettings();
        }

        public override void OnDeactivate()
        {
        }

        public override void OnTitleBarGUI()
        {
            if (Help.HasHelpForObject(VideoServerSettings.Instance))
            {
                if (EditorGUILayout.DropdownButton(Contents.HelpMenuIcon, FocusType.Passive, EditorStyles.label))
                {
                    Help.ShowHelpForObject(VideoServerSettings.Instance);
                }
            }

            if (EditorGUILayout.DropdownButton(Contents.SettingMenuIcon, FocusType.Passive, EditorStyles.label))
            {
                var menu = new GenericMenu();
                menu.AddItem(Contents.ResetLabel, false, reset =>
                {
                    VideoServerSettings.Instance.Reset();
                    VideoServerSettings.Save();
                }, null);
                menu.ShowAsContext();
            }
        }

        public override void OnGUI(string searchContext)
        {
            if (m_SerializedObject == null)
            {
                InitializeWithCurrentSettings();
            }
            else
            {
                m_SerializedObject.Update();
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            using (new SettingsWindowGUIScope())
            {
                DoEncoderGUI();
                EditorGUILayout.PropertyField(m_ResolutionScale);
                EditorGUILayout.PropertyField(m_FrameRateProp);
                EditorGUILayout.PropertyField(m_QualityProp);
                EditorGUILayout.PropertyField(m_PrioritizeLatencyProp);

                if (change.changed)
                {
                    m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                    VideoServerSettings.Save();
                }
            }
        }

        public override void OnFooterBarGUI()
        {
        }

        public override void OnInspectorUpdate()
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new VideoServerSettingsProvider(
                k_SettingsMenuPath,
                SettingsScope.User,
                GetSearchKeywordsFromSerializedObject(new SerializedObject(VideoServerSettings.Instance))
            );
        }

        /// <summary>
        /// Grab the <see cref="VideoServerSettings"/> instance and set it up for editing.
        /// </summary>
        void InitializeWithCurrentSettings()
        {
            m_SerializedObject = new SerializedObject(VideoServerSettings.Instance);
            m_Encoder = m_SerializedObject.FindProperty("m_Encoder");
            m_ResolutionScale = m_SerializedObject.FindProperty("m_ResolutionScale");
            m_FrameRateProp = m_SerializedObject.FindProperty("m_FrameRate");
            m_QualityProp = m_SerializedObject.FindProperty("m_Quality");
            m_PrioritizeLatencyProp = m_SerializedObject.FindProperty("m_PrioritizeLatency");

            m_EncodersSupportedOnPlatform = Enum.GetValues(typeof(VideoEncoder))
                .Cast<VideoEncoder>()
                .Where(e => EncoderUtilities.IsSupported(e) != EncoderSupport.NotSupportedOnPlatform)
                .ToArray();

            m_EncoderOptions = m_EncodersSupportedOnPlatform
                .Select(e => new GUIContent(e.GetDisplayName()))
                .ToArray();

            m_EncoderSupport = EncoderUtilities.IsSupported((VideoEncoder)m_Encoder.intValue);
        }

        void DoEncoderGUI()
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var currentIndex = Array.IndexOf(m_EncodersSupportedOnPlatform, (VideoEncoder)m_Encoder.intValue);

                if (currentIndex == -1)
                {
                    currentIndex = Array.IndexOf(m_EncodersSupportedOnPlatform, EncoderUtilities.DefaultSupportedEncoder());
                }

                var newIndex = EditorGUILayout.Popup(Contents.EncoderLabel, currentIndex, m_EncoderOptions);

                if (change.changed)
                {
                    var encoder = m_EncodersSupportedOnPlatform[newIndex];

                    m_Encoder.intValue = (int)encoder;
                    m_EncoderSupport = EncoderUtilities.IsSupported(encoder);
                }

                switch (m_EncoderSupport)
                {
                    case EncoderSupport.Supported:
                        break;
                    case EncoderSupport.NoDriver:
                        EditorGUILayout.HelpBox("The driver required to use the selected encoder could not be found. Install the appropriate driver to resolve this issue.", MessageType.Error);
                        break;
                    case EncoderSupport.DriverVersionNotSupported:
                        EditorGUILayout.HelpBox("The driver required by the selected encoder is too old. Update the driver to resolve this issue.", MessageType.Error);
                        break;
                    default:
                        EditorGUILayout.HelpBox("The selected encoder is not available.", MessageType.Error);
                        break;
                }
            }
        }
    }
}
