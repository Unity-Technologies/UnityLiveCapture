using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.LiveCapture.Editor;
using Unity.LiveCapture.CompanionApp.Editor;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomEditor(typeof(VirtualCameraDevice))]
    class VirtualCameraDeviceEditor : CompanionAppDeviceEditor<IVirtualCameraClient>
    {
        static readonly float k_SnapshotElementHeight = 4f * (EditorGUIUtility.singleLineHeight + 2f) + 2f;
        static readonly float k_PreviewWidth = k_SnapshotElementHeight * 16f / 9f;

        public static class Contents
        {
            public static GUIContent None = EditorGUIUtility.TrTextContent("None");
            public static GUIContent CreateVirtualCameraActor = EditorGUIUtility.TrTextContent("Virtual Camera Actor");
            public static GUIContent CreateCinemachineCameraActor = EditorGUIUtility.TrTextContent("Cinemachine Camera Actor");
            public static GUIContent LensAssetLabel = EditorGUIUtility.TrTextContent("Lens Asset", "The asset that provides the lens intrinsics.");
            public static GUIContent CameraBody = EditorGUIUtility.TrTextContent("Camera Body", "The parameters of the camera's body.");
            public static GUIContent Settings = EditorGUIUtility.TrTextContent("Settings", "The settings of the device.");
            public static GUIContent AnchorSettings = EditorGUIUtility.TrTextContent("Anchor Settings", "The anchor settings of the device.");
            public static GUIContent Recorder = EditorGUIUtility.TrTextContent("Keyframe Reduction", "Parameters to reduce redundant keyframes in the recorded animations. Higher values reduce the file size but might affect the curve accuracy.");
            public static GUIContent VideoSettingsButton = EditorGUIUtility.TrTextContent("Open Video Settings", "Open the settings of the video server.");
            public static string Deleted = L10n.Tr("(deleted)");
            public static GUIContent Snapshots = EditorGUIUtility.TrTextContent("Snapshots", "The snapshots taken using this device.");
            public static GUIContent Library = EditorGUIUtility.TrTextContent("Library", "The asset where the snapshots are stored.");
            public static GUIContent NewLibrary = EditorGUIUtility.TrTextContent("New", "Create and assign a new Snapshot Library asset.");
            public static GUIContent NoLibrarySelected = EditorGUIUtility.TrTextContent("No Snapshot Library asset selected", "Select or create a Snapshot Library asset to inspect its contents.");
            public static GUIContent TakeSnapshot = EditorGUIUtility.TrTextContent("Take Snapshot", "Save the current position, lens and camera body while generating a screenshot.");
            public static GUIContent GotoLabel = EditorGUIUtility.TrTextContent("Go To", "Move the camera to the saved position.");
            public static GUIContent Load = EditorGUIUtility.TrTextContent("Load", "Move the camera to the saved position and restore the saved lens and the camera body.");
            public static GUIContent MigrateSnapshotsMessage = EditorGUIUtility.TrTextContent("Detected snapshots in obsolete storage. Would you like to transfer them to a Snapshot Library, or simply delete them?");
            public static GUIContent MigrateSnapshotsSaveMessage = EditorGUIUtility.TrTextContent("To finalize the actions below, you must save the current Scene.");
            public static GUIContent MigrateSnapshotsMigrateButton = EditorGUIUtility.TrTextContent("Migrate Snapshots to Library", "Creates a new Snapshot Library for this device and transfers its snapshots to it.");
            public static GUIContent MigrateSnapshotsDeleteButton = EditorGUIUtility.TrTextContent("Delete Snapshots", "Deletes all snapshots from this device's obsolete storage.");
            public static string PreviewNotAvailable = L10n.Tr("Preview not available.");
            public static readonly GUIContent ActorLabel = EditorGUIUtility.TrTextContent("Actor", "The actor currently assigned to this device.");
            public static GUIContent ActorCreateNew = EditorGUIUtility.TrTextContent("Create", "Create a new Virtual Camera Actor and assign it to this device.");
            public static string MissingActorText = L10n.Tr("The device requires a Virtual Camera Actor target.");
            public static string MissingClientText = L10n.Tr("The device requires a connected Client.");
            public static string ReadMoreText = L10n.Tr("read more");
            public static string ConnectClientURL = Documentation.baseURL + "setup-connecting" + Documentation.endURL;
            public static string SetupActorURL = Documentation.baseURL + "virtual-camera-workflow" + Documentation.endURL;

            static GUIStyle s_CenteredLabel;
            public static GUIStyle CenteredLabel
            {
                get
                {
                    if (s_CenteredLabel == null)
                    {
                        s_CenteredLabel = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            wordWrap = true
                        };
                    }
                    return s_CenteredLabel;
                }
            }
        }

        static readonly (GUIContent label, Func<UnityEngine.Object> createActorFunc)[] k_ActorCreateMenuItems =
        {
            (Contents.CreateVirtualCameraActor, CreateVirtualCameraActor),
#if CINEMACHINE_2_4_OR_NEWER
            (Contents.CreateCinemachineCameraActor, CreateCinemachineCameraActor)
#endif
        };

        static VirtualCameraActor CreateVirtualCameraActor()
        {
            return GetActorComponent(VirtualCameraCreatorUtilities.CreateVirtualCameraActorInternal());
        }

#if CINEMACHINE_2_4_OR_NEWER
        static VirtualCameraActor CreateCinemachineCameraActor()
        {
            return GetActorComponent(VirtualCameraCreatorUtilitiesCM.CreateCinemachineCameraActorInternal());
        }

#endif

        static VirtualCameraActor GetActorComponent(GameObject go)
        {
            var actor = go.GetComponent<VirtualCameraActor>();

            Debug.Assert(actor != null, $"Can't find {nameof(VirtualCameraActor)} component in {go}");

            return actor;
        }

        VirtualCameraDevice m_Device;
        SerializedProperty m_Actor;
        SerializedProperty m_Channels;
        SerializedProperty m_Lens;
        SerializedProperty m_LensAsset;
        SerializedProperty m_LensIntrinsics;
        SerializedProperty m_CameraBody;
        SerializedProperty m_Settings;
        SerializedProperty m_AnchorDeviceSettings;
        SerializedProperty m_Recorder;
        SerializedProperty m_Snapshots;
        SerializedProperty m_SnapshotLibrary;

        SnapshotLibrary m_EmptySnapshotLibrary;
        UnityEditor.Editor m_SnapshotLibraryEditor;

        void OnEnable()
        {
            m_Device = target as VirtualCameraDevice;

            m_Actor = serializedObject.FindProperty("m_Actor");
            m_Channels = serializedObject.FindProperty("m_Channels");
            m_LensAsset = serializedObject.FindProperty("m_LensAsset");
            m_Lens = serializedObject.FindProperty("m_Lens");
            m_LensIntrinsics = serializedObject.FindProperty("m_LensIntrinsics");
            m_CameraBody = serializedObject.FindProperty("m_CameraBody");
            m_Settings = serializedObject.FindProperty("m_Settings");
            m_AnchorDeviceSettings = serializedObject.FindProperty("m_AnchorDeviceSettings");
            m_Recorder = serializedObject.FindProperty("m_Recorder");
            m_Snapshots = serializedObject.FindProperty("m_Snapshots");
            m_SnapshotLibrary = serializedObject.FindProperty("m_SnapshotLibrary");

            m_EmptySnapshotLibrary = CreateInstance<SnapshotLibrary>();
        }

        void OnDisable()
        {
            if (m_SnapshotLibraryEditor != null)
            {
                DestroyImmediate(m_SnapshotLibraryEditor);
            }

            DestroyImmediate(m_EmptySnapshotLibrary);
        }

        protected override void OnDeviceGUI()
        {
            if (m_Device.GetClient() == null)
            {
                LiveCaptureGUI.HelpBoxWithURL(Contents.MissingClientText, Contents.ReadMoreText,
                    Contents.ConnectClientURL, MessageType.Warning);
            }

            serializedObject.Update();

            DoActorCreateGUI();

            if (m_Actor.objectReferenceValue == null)
            {
                LiveCaptureGUI.HelpBoxWithURL(Contents.MissingActorText, Contents.ReadMoreText,
                    Contents.SetupActorURL, MessageType.Warning);

                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                DoChannelsGUI(m_Channels);
                DoLensAssetField();

                if (GUILayout.Button(Contents.VideoSettingsButton))
                {
                    VideoServerSettingsProvider.Open();
                }

                LensDrawerUtility.DoLensGUI(m_Lens, m_LensIntrinsics);

                EditorGUILayout.PropertyField(m_CameraBody, Contents.CameraBody);

                EditorGUILayout.PropertyField(m_Settings, Contents.Settings);

                using (new EditorGUI.DisabledGroupScope(m_Device.IsRecording))
                {
                    EditorGUILayout.PropertyField(m_AnchorDeviceSettings, Contents.AnchorSettings);
                }

                EditorGUILayout.PropertyField(m_Recorder, Contents.Recorder);

                var controlRect = EditorGUILayout.GetControlRect();
                var snapshotsFoldoutLabel = EditorGUI.BeginProperty(controlRect, Contents.Snapshots, m_SnapshotLibrary);
                m_SnapshotLibrary.isExpanded = EditorGUI.Foldout(controlRect, m_SnapshotLibrary.isExpanded, snapshotsFoldoutLabel, true);

                if (m_SnapshotLibrary.isExpanded)
                {
                    if (m_Snapshots.arraySize > 0)
                    {
                        DoMigrateSnapshotsGUI();
                    }
                    else
                    {
                        DoSnapshotsGUI();
                    }
                }

                serializedObject.ApplyModifiedProperties();

#if URP_14_0_OR_NEWER
                if (m_Device.Settings.FocusPlane)
                {
                    RenderFeatureEditor<FocusPlaneRenderer, VirtualCameraScriptableRenderFeature>.OnInspectorGUI();
                }

                if (m_Device.Settings.GateMask ||
                    m_Device.Settings.AspectRatioLines ||
                    m_Device.Settings.CenterMarker)
                {
                    RenderFeatureEditor<FrameLines, VirtualCameraScriptableRenderFeature>.OnInspectorGUI();
                }
#endif
            }
        }

        void DoActorCreateGUI()
        {
            // Create button style inspired from VolumeEditor.OnInspectorGUI
            var lineRect = EditorGUILayout.GetControlRect();
            var fieldWidth = lineRect.width - EditorGUIUtility.labelWidth;
            var buttonWidth = Mathf.Min(60f, fieldWidth);
            var fieldRect = new Rect(lineRect.x, lineRect.y, lineRect.width - buttonWidth, lineRect.height);
            var buttonNewRect = new Rect(fieldRect.xMax, lineRect.y, buttonWidth, lineRect.height);

            EditorGUI.ObjectField(fieldRect, m_Actor);

            if (GUI.Button(buttonNewRect, Contents.ActorCreateNew, EditorStyles.miniButton))
            {
                if (k_ActorCreateMenuItems.Length == 1)
                {
                    AssignAndPingActor(k_ActorCreateMenuItems[0].createActorFunc());
                }
                else
                {
                    var menu = new GenericMenu();

                    foreach (var (label, createActorFunc) in k_ActorCreateMenuItems)
                    {
                        menu.AddItem(label, false, () => AssignAndPingActor(createActorFunc()));
                    }

                    menu.ShowAsContext();
                }
            }
        }

        void AssignAndPingActor(UnityEngine.Object newActor)
        {
            serializedObject.Update();

            m_Actor.objectReferenceValue = newActor;

            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.PingObject(newActor);

            //TakeRecorderEditor.RepaintEditors();
        }

        static SnapshotLibrary CreateSnapshotLibrary(string name)
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create new Snapshot Library",
                name,
                "asset",
                "Create a new Snapshot Library at the selected directory"
            );

            return SnapshotLibraryUtility.CreateSnapshotLibrary(path);
        }

        void DoSnapshotsGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(Contents.Library);
                EditorGUILayout.ObjectField(m_SnapshotLibrary, GUIContent.none);

                if (GUILayout.Button(Contents.NewLibrary, EditorStyles.miniButton))
                {
                    var asset = CreateSnapshotLibrary(
                        SnapshotLibraryUtility.GetSnapshotLibraryDefaultName(m_Device));

                    if (asset != null)
                    {
                        m_SnapshotLibrary.objectReferenceValue = asset;
                    }
                }
            }

            var snapshotLibrary = m_EmptySnapshotLibrary;
            var hasSnapshotLibrary = m_SnapshotLibrary.objectReferenceValue != null;

            if (hasSnapshotLibrary)
            {
                snapshotLibrary = m_SnapshotLibrary.objectReferenceValue as SnapshotLibrary;
            }

            CreateCachedEditor(snapshotLibrary, null, ref m_SnapshotLibraryEditor);

            var snapshotLibraryEditor = m_SnapshotLibraryEditor as SnapshotLibraryEditor;

            snapshotLibraryEditor.NoneListItemContent = hasSnapshotLibrary ? null : Contents.NoLibrarySelected;

            using (new EditorGUI.DisabledScope(!hasSnapshotLibrary))
            {
                m_SnapshotLibraryEditor.OnInspectorGUI();
            }

            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledScope(m_Device.IsRecording))
            {
                using (new EditorGUI.DisabledScope(!m_Device.IsLiveAndReady()))
                {
                    if (GUILayout.Button(Contents.TakeSnapshot))
                    {
                        m_Device.TakeSnapshot();

                        EditorUtility.SetDirty(m_Device);
                    }
                }

                using (new EditorGUI.DisabledScope(snapshotLibrary.Count == 0 || snapshotLibraryEditor.Index == -1))
                {
                    if (GUILayout.Button(Contents.GotoLabel))
                    {
                        m_Device.GoToSnapshot(snapshotLibraryEditor.Index);

                        EditorUtility.SetDirty(m_Device);
                    }

                    if (GUILayout.Button(Contents.Load))
                    {
                        m_Device.LoadSnapshot(snapshotLibraryEditor.Index);

                        EditorUtility.SetDirty(m_Device);
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        void DoMigrateSnapshotsGUI()
        {
            EditorGUILayout.HelpBox(Contents.MigrateSnapshotsMessage.text, MessageType.Info);
            EditorGUILayout.HelpBox(Contents.MigrateSnapshotsSaveMessage.text, MessageType.Info);

            if (GUILayout.Button(Contents.MigrateSnapshotsMigrateButton))
            {
                var asset = CreateSnapshotLibrary(
                    SnapshotLibraryUtility.GetSnapshotLibraryDefaultName(m_Device));

                if (asset != null)
                {
                    m_SnapshotLibrary.objectReferenceValue = asset;

                    serializedObject.ApplyModifiedProperties();

                    SnapshotLibraryUtility.MigrateSnapshotsToSnapshotLibrary(m_Device);
                }
            }

            if (GUILayout.Button(Contents.MigrateSnapshotsDeleteButton))
            {
                m_Snapshots.ClearArray();
            }
        }

        void DoLensAssetField()
        {
            LensKitCache.Get(out var presets, out var contents);

            var asset = m_LensAsset.objectReferenceValue;
            var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            var label = EditorGUI.BeginProperty(rect, Contents.LensAssetLabel, m_LensAsset);
            EditorGUI.LabelField(rect, label);

            rect.x += EditorGUIUtility.labelWidth + 5;
            rect.width -= EditorGUIUtility.labelWidth + 5;

            var name = Contents.None;

            if (asset != null)
            {
                name = new GUIContent(asset.name);
            }

            if (GUI.Button(rect, name, EditorStyles.popup))
            {
                var menu = new GenericMenu();

                for (var i = 0; i < contents.Length; ++i)
                {
                    menu.AddItem(contents[i], presets[i] == asset, (p) => SetLensPreset(p as LensAsset), presets[i]);
                }

                menu.ShowAsContext();
            }

            EditorGUI.EndProperty();
        }

        void SetLensPreset(LensAsset preset)
        {
            serializedObject.Update();

            m_LensAsset.objectReferenceValue = preset;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
