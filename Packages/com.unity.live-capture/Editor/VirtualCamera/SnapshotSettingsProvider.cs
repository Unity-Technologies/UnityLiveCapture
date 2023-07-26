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
    class SnapshotSettingsProvider : SettingsProvider
    {
        const string k_SettingsMenuPath = "Project/Live Capture/Virtual Camera/Snapshots";

        static class Contents
        {
            public static readonly GUIContent SettingMenuIcon = EditorGUIUtility.IconContent("_Popup");
            public static readonly GUIContent ResetLabel = EditorGUIUtility.TrTextContent("Reset", "Reset to default.");
            public static readonly GUIContent ScreenshotDirectoryLabel = EditorGUIUtility.TrTextContent("Screenshot Directory",
                "The output directory of the screenshots generated when taking snapshots.");
        }

        SerializedObject m_SerializedObject;
        SerializedProperty m_ScreenshotDirectory;

        /// <summary>
        /// Open the settings in the Project Settings.
        /// </summary>
        public static void Open()
        {
            SettingsService.OpenProjectSettings(k_SettingsMenuPath);
        }

        public SnapshotSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
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
            if (EditorGUILayout.DropdownButton(Contents.SettingMenuIcon, FocusType.Passive, EditorStyles.label))
            {
                var menu = new GenericMenu();
                menu.AddItem(Contents.ResetLabel, false, reset =>
                {
                    SnapshotSettings.Instance.Reset();
                    SnapshotSettings.Save();
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
                EditorGUILayout.PropertyField(m_ScreenshotDirectory, Contents.ScreenshotDirectoryLabel);

                if (change.changed)
                {
                    m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                    SnapshotSettings.Save();
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
            return new SnapshotSettingsProvider(
                k_SettingsMenuPath,
                SettingsScope.Project,
                GetSearchKeywordsFromSerializedObject(new SerializedObject(SnapshotSettings.Instance))
            );
        }

        /// <summary>
        /// Grab the <see cref="SnapshotSettings"/> instance and set it up for editing.
        /// </summary>
        void InitializeWithCurrentSettings()
        {
            m_SerializedObject = new SerializedObject(SnapshotSettings.Instance);
            m_ScreenshotDirectory = m_SerializedObject.FindProperty("m_ScreenshotDirectory");
        }
    }
}
