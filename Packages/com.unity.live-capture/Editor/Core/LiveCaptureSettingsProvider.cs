using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture.Editor
{
    class LiveCaptureSettingsProvider : SettingsProvider
    {
        const string k_SettingsMenuPath = "Project/Live Capture";

        static class Contents
        {
            public static readonly GUIContent SettingMenuIcon = EditorGUIUtility.IconContent("_Popup");
            public static readonly GUIContent ResetLabel = EditorGUIUtility.TrTextContent("Reset", "Reset to default.");
            public static readonly GUIContent FrameRate = EditorGUIUtility.TrTextContent("Frame Rate", "The frame rate to use for recording.");
            public static readonly GUIContent TakeNameFormatLabel = EditorGUIUtility.TrTextContent("Take Name Format", "The format of the file name of the output take.");
            public static readonly GUIContent AssetNameFormatLabel = EditorGUIUtility.TrTextContent("Asset Name Format", "The format of the file name of the generated assets.");
            public static readonly GUIContent SyncSectionLabel = EditorGUIUtility.TrTextContent("Genlock");
            public static readonly GUIContent SyncProviderAssignLabel = EditorGUIUtility.TrTextContent("Genlock Source", "The genlock signal source used to control the engine update timing.");
            public static readonly GUIContent SyncProviderNoneLabel = EditorGUIUtility.TrTextContent("None");
        }

        static (Type type, SyncProviderAttribute[] attributes)[] s_SyncProviderTypes;

        SerializedObject m_SerializedObject;
        SerializedProperty m_FrameRate;
        SerializedProperty m_TakeNameFormatProp;
        SerializedProperty m_AssetNameFormatProp;
        SerializedProperty m_SyncProviderProp;

        /// <summary>
        /// Open the settings in the Project Settings.
        /// </summary>
        public static void Open()
        {
            SettingsService.OpenProjectSettings(k_SettingsMenuPath);
        }

        public LiveCaptureSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
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
                    LiveCaptureSettings.Instance.Reset();
                    LiveCaptureSettings.Save();
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
                EditorGUILayout.PropertyField(m_FrameRate, Contents.FrameRate);
                EditorGUILayout.PropertyField(m_TakeNameFormatProp, Contents.TakeNameFormatLabel);
                EditorGUILayout.PropertyField(m_AssetNameFormatProp, Contents.AssetNameFormatLabel);

                DoSyncProviderGUI();

                if (change.changed)
                {
                    m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                    LiveCaptureSettings.Save();
                }
            }
        }

        public override void OnFooterBarGUI()
        {
        }

        public override void OnInspectorUpdate()
        {
        }

        void DoSyncProviderGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Contents.SyncSectionLabel, EditorStyles.boldLabel);

            // use a popup to select the sync provider
            if (s_SyncProviderTypes == null)
            {
                s_SyncProviderTypes = AttributeUtility.GetAllTypes<SyncProviderAttribute>()
                    .Where(t => !t.type.IsAbstract && t.type.GetAttribute<SerializableAttribute>() != null)
                    .ToArray();
            }

            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, Contents.SyncProviderAssignLabel);

            var provider = LiveCaptureSettings.Instance.SyncProvider;
            var providerType = provider?.GetType();
            var currentOption = Contents.SyncProviderNoneLabel;

            if (provider != null)
            {
                foreach (var types in s_SyncProviderTypes)
                {
                    if (types.type == providerType && types.attributes.Length > 0)
                    {
                        var split = types.attributes[0].ItemName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        currentOption = new GUIContent(split[split.Length - 1]);
                        break;
                    }
                }
            }

            if (GUI.Button(rect, currentOption, EditorStyles.popup))
            {
                var menu = new GenericMenu();

                menu.AddItem(Contents.SyncProviderNoneLabel, providerType == null, () =>
                {
                    m_SyncProviderProp.managedReferenceValue = null;
                    m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                    LiveCaptureSettings.Save();
                });

                MenuUtility.CreateMenu(s_SyncProviderTypes, (type, attribute) =>
                {
                    menu.AddItem(new GUIContent(attribute.ItemName), providerType == type, () =>
                    {
                        if (provider == null || provider.GetType() != type)
                        {
                            provider = Activator.CreateInstance(type) as ISyncProvider;
                            m_SyncProviderProp.managedReferenceValue = provider;
                            m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                            LiveCaptureSettings.Save();
                        }
                    });
                }, menu.AddSeparator);

                menu.DropDown(rect);
            }

            // draw the serialized properties of the selected provider type
            if (provider != null)
            {
                var prop = m_SyncProviderProp.Copy();
                var endProp = m_SyncProviderProp.GetEndProperty();
                var isFirst = true;

                while (prop.Next(isFirst) && !SerializedProperty.EqualContents(prop, endProp))
                {
                    EditorGUILayout.PropertyField(prop);
                    isFirst = false;
                }
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new LiveCaptureSettingsProvider(
                k_SettingsMenuPath,
                SettingsScope.Project,
                GetSearchKeywordsFromSerializedObject(new SerializedObject(LiveCaptureSettings.Instance))
            );
        }

        /// <summary>
        /// Grab the <see cref="LiveCaptureSettings"/> instance and set it up for editing.
        /// </summary>
        void InitializeWithCurrentSettings()
        {
            m_SerializedObject = new SerializedObject(LiveCaptureSettings.Instance);
            m_FrameRate = m_SerializedObject.FindProperty("m_FrameRate");
            m_TakeNameFormatProp = m_SerializedObject.FindProperty("m_TakeNameFormat");
            m_AssetNameFormatProp = m_SerializedObject.FindProperty("m_AssetNameFormat");
            m_SyncProviderProp = m_SerializedObject.FindProperty("m_SyncProvider");
        }
    }
}
