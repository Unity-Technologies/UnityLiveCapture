using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Unity.LiveCapture.Editor;

namespace Unity.LiveCapture.Mocap.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(MocapGroup), true)]
    class MocapGroupEditor : Editor
    {
        static IEnumerable<(Type, CreateDeviceMenuItemAttribute[])> s_CreateDeviceMenuItems;

        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";
            public static readonly GUIContent Animator = EditorGUIUtility.TrTextContent("Animator", "The Animator component to animate.");
            public static readonly GUIContent TimecodeSource = EditorGUIUtility.TrTextContent("Timecode Source", "The Mocap Device to read timecode from.");
            public static readonly GUIContent EnableToggle = EditorGUIUtility.TrTextContent("", "Enable or disable the source.");
            public static readonly GUIContent DeviceListHeader = EditorGUIUtility.TrTextContent("Mocap Devices");
            public static readonly GUIContent SelectedDevice = EditorGUIUtility.TrTextContent("Selected Device", "The properties of the selected Mocap Device in the list.");
            public static readonly GUIContent RecordingTransforms = EditorGUIUtility.TrTextContent("Recording Transforms", "The list of Transforms recording in the current session.");
            public static readonly GUIContent None = EditorGUIUtility.TrTextContent("None");
            public static readonly GUIContent NoSourceSelected = EditorGUIUtility.TrTextContent("No source selected");
            public static readonly string SourceGameObjectName = L10n.Tr("Mocap Devices");
            public static readonly string UndoCreateDevice = L10n.Tr("Create Mocap Device");
            public static readonly string UndoEnableDevice = L10n.Tr("Set Enabled");
            public static readonly GUIContent SourceConnectedIcon = EditorGUIUtility.TrIconContent($"{k_IconPath}/winbtn_mac_max.png");
            public static readonly GUIContent SourceNotConnectedIcon = EditorGUIUtility.TrIconContent($"{k_IconPath}/winbtn_mac_close.png");
        }

        MocapGroup m_Device;
        SerializedProperty m_Animator;
        SerializedProperty m_TimeSource;
        SerializedProperty m_Devices;
        ReorderableList m_List;
        bool m_DeviceFoldout;
        bool m_TransformsFoldout;
        Editor m_Editor;

        void OnEnable()
        {
            m_Device = (MocapGroup)target;
            m_Animator = serializedObject.FindProperty("m_Animator");
            m_Devices = serializedObject.FindProperty("m_Devices");
            m_TimeSource = serializedObject.FindProperty("m_TimeSource");

            CreateDeviceList();
        }

        void OnDisable()
        {
            if (m_Editor != null)
            {
                DestroyImmediate(m_Editor);
            }
        }

        void CreateDeviceList()
        {
            m_List = new ReorderableList(serializedObject, m_Devices, true, true, true, false);

            m_List.drawHeaderCallback = (Rect r) => EditorGUI.LabelField(r, Contents.DeviceListHeader);

            m_List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_Devices.GetArrayElementAtIndex(index);
                var device = element.objectReferenceValue as LiveCaptureDevice;
                var isLive = device != null ? device.IsLive : false;

                var buttonRect = rect;
                buttonRect.width = 15f;

                var statusRect = rect;
                statusRect.width = 15f;
                statusRect.x = buttonRect.xMax + 5f;

                var nameRect = rect;
                nameRect.x = statusRect.xMax + 5f;
                nameRect.width -= 40f;

                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    isLive = GUI.Toggle(buttonRect, isLive, Contents.EnableToggle);

                    if (change.changed)
                    {
                        Undo.RegisterCompleteObjectUndo(device, Contents.UndoEnableDevice);

                        device.IsLive = isLive;

                        EditorUtility.SetDirty(device);
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                }

                EditorGUI.LabelField(statusRect, device.IsReady() ? Contents.SourceConnectedIcon : Contents.SourceNotConnectedIcon);

                var name = GetDisplayName(device);

                EditorGUI.LabelField(nameRect, name);
            };

            m_List.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                if (s_CreateDeviceMenuItems == null)
                {
                    s_CreateDeviceMenuItems = AttributeUtility.GetAllTypes<CreateDeviceMenuItemAttribute>()
                        .Where(t => typeof(IMocapDevice).IsAssignableFrom(t.type));
                }

                var menu = MenuUtility.CreateMenu(s_CreateDeviceMenuItems, t => true, (type, attribute) => CreateDevice(type));
                menu.ShowAsContext();
            };
        }

        void CreateDevice(Type type)
        {
            Debug.Assert(type != null);

            var go = new GameObject($"New {type.Name}", type);

            go.transform.SetParent(m_Device.transform, false);
            GameObjectUtility.EnsureUniqueNameForSibling(go);

            Undo.RegisterCreatedObjectUndo(go, Contents.UndoCreateDevice);

            EditorGUIUtility.PingObject(go);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Animator, Contents.Animator);

            DoTimeSourceGUI();

            EditorGUILayout.Space();

            DoListGUI();

            EditorGUILayout.Space();

            DoSelectedGUI();

            EditorGUILayout.Space();

            DoTransformsGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void DoTimeSourceGUI()
        {
            var formatter = new UniqueNameFormatter();

            formatter.Format("None");

            var devices = m_Device.Devices;
            var names = devices
                .Select(d => formatter.Format(GetDisplayName(d)))
                .Prepend("None")
                .ToArray();

            var selected = m_TimeSource.objectReferenceValue as LiveCaptureDevice;
            var index = devices.FindIndex((device) => device == selected);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                index = EditorGUILayout.Popup(Contents.TimecodeSource, index + 1, names) - 1;

                if (change.changed)
                {
                    m_TimeSource.objectReferenceValue = index >= 0 ? devices[index] : null;
                }
            }
        }

        void DoListGUI()
        {
            m_List.DoLayoutList();
        }

        void DoSelectedGUI()
        {
            var index = m_List.index;

            if (index < 0 || index >= m_Devices.arraySize)
                return;

            m_DeviceFoldout = EditorGUILayout.Foldout(m_DeviceFoldout, Contents.SelectedDevice, true);

            if (!m_DeviceFoldout)
                return;

            var element = m_Devices.GetArrayElementAtIndex(index);

            using (new EditorGUI.IndentLevelScope())
            {
                var device = element.objectReferenceValue as LiveCaptureDevice;

                if (device != null)
                {
                    EditorGUILayout.LabelField(GetDisplayName(device), EditorStyles.boldLabel);

                    CreateCachedEditor(device, null, ref m_Editor);

                    if (m_Editor != null)
                    {
                        m_Editor.OnInspectorGUI();
                    }
                }
            }
        }

        void DoTransformsGUI()
        {
            m_TransformsFoldout = EditorGUILayout.Foldout(m_TransformsFoldout, Contents.RecordingTransforms, true);

            if (!m_TransformsFoldout)
                return;

            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(true))
            {
                var recorder = m_Device.GetRecorder();
                var transforms = recorder.GetTransforms();

                Debug.Assert(recorder != null);
                Debug.Assert(transforms != null);

                if (transforms.Any())
                {
                    foreach (var transform in transforms)
                    {
                        if (transform == null)
                            continue;

                        EditorGUILayout.ObjectField(transform, typeof(Transform), true);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(Contents.None);
                }
            }
        }

        string GetDisplayName(LiveCaptureDevice device)
        {
            if (device is ITimedDataSource source
                && !string.IsNullOrEmpty(source.FriendlyName))
            {
                return source.FriendlyName;
            }

            return device.gameObject.name;
        }
    }
}
