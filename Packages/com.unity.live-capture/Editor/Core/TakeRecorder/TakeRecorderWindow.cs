using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    class TakeRecorderWindow : EditorWindow
    {
        static class Contents
        {

            static readonly string k_IconPath = $"Packages/{LiveCaptureInfo.Name}/Editor/Core/Icons";
            public static readonly GUIStyle BottomBarBg = "ProjectBrowserBottomBarBg";
            public static readonly GUIContent NoContext = EditorGUIUtility.TrTextContent("None", "Select a shot provider to use for recording takes.");
            public static readonly GUIContent OpenSettingsIcon = EditorGUIUtility.TrIconContent("_Popup", "Open the Take System project settings.");
            public static GUIContentCompact Live { get; private set; } = GUIContentCompact.None;
            public static readonly GUIContent PlayPreviewLabel = EditorGUIUtility.TrIconContent("PlayButton", "Start previewing the selected Take.");
            public static GUIContent PlayTakeContents { get; private set; }
            public static readonly GUIContent GotoBeginningContent = L10n.IconContent("Animation.FirstKey", "Go to the beginning of the shot");
            public static readonly GUIContent PausePreviewLabel = EditorGUIUtility.TrIconContent("PauseButton", "Pause the ongoing playback.");
            public static readonly GUIContentCompact Record = new GUIContentCompact(EditorStyles.toolbar, "Record", "Start recording a new Take.", "Animation.Record");
            public static readonly GUIContentCompact StopRecording = new GUIContentCompact(EditorStyles.toolbar, "Stop", "Stop the ongoing recording.", $"{k_IconPath}/StopRecording@64.png");
            public static GUIStyle ToolbarButtonBlue { get; private set; }
            public static GUIStyle LiveLabelStyle { get; private set; }
            public static readonly string ReadMoreText = L10n.Tr("read more");
            public static readonly string CreateDeviceURL = Documentation.baseURL + "ref-window-take-recorder" + Documentation.endURL;
            public static readonly string NoDevicesMessage = L10n.Tr("Recording requires at least a capture device.");
            public static readonly string NoDeviceReadyMessage = L10n.Tr("Recording requires an enabled and configured capture device.");
            public static readonly string NoShotSelectedMessage = L10n.Tr("Recording requires a shot to be selected.");
            public static readonly string UndoCreateDevice = L10n.Tr("Create Capture Device");

            static Contents()
            {
                LoadStyles();
            }

            public static void LoadStyles()
            {
                var playrange = EditorStyles.FromUSS("Icon-Playrange");

                PlayTakeContents = new GUIContent(playrange.normal.background)
                {
                    tooltip = L10n.Tr("Toggle play content's range.")
                };

                ToolbarButtonBlue = new GUIStyle(EditorStyles.miniButton)
                {
                    fixedHeight = 0
                };

                LiveLabelStyle = new GUIStyle()
                {
                    alignment = EditorStyles.toolbarButton.alignment,
                    border = EditorStyles.toolbarButton.border,
                    fixedHeight = EditorStyles.toolbarButton.fixedHeight,
                    font = EditorStyles.toolbarButton.font,
                    fontSize = EditorStyles.toolbarButton.fontSize,
                    fontStyle = EditorStyles.toolbarButton.fontStyle,
                    normal = EditorStyles.toolbarButton.normal,
                    padding = EditorStyles.toolbarButton.padding
                };
            }

            public static void PrepareLiveIcon(Texture2D liveIcon)
            {
                Live = new GUIContentCompact(EditorStyles.toolbarButton, "Live", "Set to live mode for previewing and recording takes.", liveIcon);
            }

            public static string GetCanRecordStatusMessage(int deviceCount, bool ShotHasValue)
            {
                if (ShotHasValue)
                {
                    return deviceCount == 0 ? NoDevicesMessage : NoDeviceReadyMessage;
                }
                else
                {
                    return deviceCount == 0 ? NoDevicesMessage : NoShotSelectedMessage;
                }
            }

            public static GUIStyle GetGUIStyle(string s)
            {
                return EditorStyles.FromUSS(s);
            }
        }

        static class IDs
        {
            public const string AddMenu = "create-device";
        }

        public static TakeRecorderWindow Instance { get; private set; }

        enum InspectorType
        {
            Shot,
            Device
        }

        const string k_WindowName = "Take Recorder";
        const string k_WindowPath = "Window/Live Capture/" + k_WindowName;
        const long k_InspectorRepaintIntervalMS = 1000;

        static IEnumerable<(Type, CreateDeviceMenuItemAttribute[])> s_CreateDeviceMenuItems;

        [SerializeField]
        VisualTreeAsset m_WindowUxml;
        [SerializeField]
        StyleSheet m_WindowCommonUss;
        [SerializeField]
        StyleSheet m_WindowDarkUss;
        [SerializeField]
        int m_Selection;
        [SerializeField]
        DeviceTreeView m_DeviceTreeView = new DeviceTreeView();
        [SerializeField]
        InspectorType m_InspectorType;
        [SerializeField]
        Vector2 m_ScrollPosition;
        [SerializeField]
        TimelineContextProvider m_TimelineProvider = new TimelineContextProvider();
        [SerializeField]
        ShotPlayerContextProvider m_ShotPlayerProvider = new ShotPlayerContextProvider();
        [SerializeField]
        ContextEditorCache m_EditorCache = new ContextEditorCache();
        List<TakeRecorderContextProvider> m_Providers = new List<TakeRecorderContextProvider>();
        bool m_DeviceListDirty;
        Editor m_Editor;
        VisualElement m_RedTint;

        [MenuItem(k_WindowPath)]
        static void ShowWindow()
        {
            GetWindow<TakeRecorderWindow>();
        }

        public static void RepaintWindow()
        {
            if (Instance != null)
            {
                Instance.Repaint();
            }
        }

        void OnEnable()
        {
            Instance = this;

            titleContent = new GUIContent(k_WindowName);

            TakeRecorder.LiveStateChanged += OnTakeRecorderStateChanged;
            TakeRecorder.RecordingStateChanged += OnTakeRecorderStateChanged;
            TakeRecorder.PlaybackStateChanged += OnTakeRecorderStateChanged;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            LiveCaptureDeviceManager.DeviceAdded += OnDeviceAdded;
            LiveCaptureDeviceManager.DeviceRemoved += OnDeviceRemoved;

            RebuildProviders();
        }

        void OnDisable()
        {
            Instance = null;

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            TakeRecorder.LiveStateChanged -= OnTakeRecorderStateChanged;
            TakeRecorder.RecordingStateChanged -= OnTakeRecorderStateChanged;
            TakeRecorder.PlaybackStateChanged -= OnTakeRecorderStateChanged;
            LiveCaptureDeviceManager.DeviceAdded -= OnDeviceAdded;
            LiveCaptureDeviceManager.DeviceRemoved -= OnDeviceRemoved;

            Editor.DestroyImmediate(m_Editor);
        }

        void OnDestroy()
        {
            m_EditorCache.Dispose();
        }

        void OnTakeRecorderStateChanged()
        {
            Repaint();
        }

        void UndoRedoPerformed()
        {
            m_DeviceListDirty = true;
        }

        void OnDeviceAdded(LiveCaptureDevice device)
        {
            m_DeviceListDirty = true;
        }

        T GetProvider<T>() where T : TakeRecorderContextProvider
        {
            if (typeof(T) == typeof(TimelineContextProvider))
            {
                return m_TimelineProvider as T;
            }
            else if (typeof(T) == typeof(ShotPlayerContextProvider))
            {
                return m_ShotPlayerProvider as T;
            }
            else
            {
                foreach (var provider in m_Providers)
                {
                    if (provider is T)
                    {
                        return provider as T;
                    }
                }
            }

            return default(T);
        }

        internal void SelectProvider<T>() where T : TakeRecorderContextProvider
        {
            var provider = GetProvider<T>();

            m_Selection = m_Providers.IndexOf(provider);
        }

        void OnDeviceRemoved(LiveCaptureDevice device)
        {
            m_DeviceListDirty = true;
        }

        void RebuildProviders()
        {
            m_Providers.Clear();
            m_Providers.Add(m_TimelineProvider);
            m_Providers.Add(m_ShotPlayerProvider);

            m_Selection = Mathf.Clamp(m_Selection, -1, m_Providers.Count - 1);
        }

        void ReloadDeviceTreeViewIfNeeded()
        {
            if (m_DeviceListDirty)
            {
                m_DeviceListDirty = false;
                m_DeviceTreeView.Reload();
            }
        }

        void CreateGUI()
        {
            m_WindowUxml.CloneTree(rootVisualElement);

            rootVisualElement.styleSheets.Add(m_WindowCommonUss);

            if (EditorGUIUtility.isProSkin)
            {
                rootVisualElement.styleSheets.Add(m_WindowDarkUss);
            }

            rootVisualElement.schedule.Execute(() =>
            {
                Contents.PrepareLiveIcon(rootVisualElement.Q("live-icon")
                    .resolvedStyle.backgroundImage.texture);
            });

            rootVisualElement.RegisterGeometryChangedEventCallbackOnce(() =>
            {
                Contents.PrepareLiveIcon(rootVisualElement.Q("live-icon")
                    .resolvedStyle.backgroundImage.texture);
            });

            m_RedTint = rootVisualElement.Q("red-tint");

            var toolbar = rootVisualElement.Q<IMGUIContainer>("toolbar");

            toolbar.onGUIHandler = () =>
            {
                using var horizontalScope = new EditorGUILayout.HorizontalScope();
                var height = toolbar.contentRect.height;
                var rect = EditorGUILayout.GetControlRect(false, height, EditorStyles.toolbar, GUILayout.MaxWidth(float.MaxValue));
                var width1 = Mathf.Min(Mathf.Max(50f, rect.width * 0.5f), 85f);
                var rect1 = new Rect(rect) { width = width1 };
                var rect2 = new Rect(rect) { x = rect1.xMax, width = rect.width - width1 };

                DoContextDropdown(rect1);

                if (TryGetSelectedProvider(out var provider))
                {
                    TryDoGUI(() => provider.OnToolbarGUI(rect2));
                }
            };

            var playControls = rootVisualElement.Q<IMGUIContainer>("play-controls");

            playControls.onGUIHandler = () =>
            {
                using var horizontalScope = new EditorGUILayout.HorizontalScope();

                var shot = TakeRecorder.Shot;

                using (new EditorGUI.DisabledGroupScope(!shot.HasValue))
                {
                    DoGoToBeginningButton();
                    DoPlayPauseButton();
                    DoPlayTakeContentsToggle();
                }
            };

            var recordControls = rootVisualElement.Q<IMGUIContainer>("record-controls");

            recordControls.onGUIHandler = () =>
            {
                using var horizontalScope = new EditorGUILayout.HorizontalScope();

                var height = recordControls.contentRect.height;
                var rect = EditorGUILayout.GetControlRect(false, height, EditorStyles.toolbar, GUILayout.MaxWidth(float.MaxValue));
                var rect1 = new Rect(rect) { width = rect.width * 0.5f };
                var rect2 = new Rect(rect) { x = rect1.xMax, width = rect.width * 0.5f };

                DoLiveReviewButton(rect1);
                DoRecordButton(rect2);
                DoOpenProjectSettingsButton();
            };

            var footer = rootVisualElement.Q<IMGUIContainer>("footer");

            footer.onGUIHandler = () =>
            {
                GUI.Label(footer.contentRect, GUIContent.none, Contents.BottomBarBg);

                DoRecordingInfoBox();
            };

            var hierarchy = rootVisualElement.Q<IMGUIContainer>("hierarchy");

            hierarchy.onGUIHandler = () =>
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    m_InspectorType = InspectorType.Shot;
                }

                if (TryGetSelectedProvider(out var provider))
                {
                    var context = provider.GetContext();

                    if (context != null)
                    {
                        var editor = CreateCachedEditor(context);

                        TryDoGUI(() => editor.OnShotGUI(hierarchy.contentRect));
                    }
                }
            };

            var deviceList = rootVisualElement.Q<IMGUIContainer>("device-list");

            deviceList.onGUIHandler = () =>
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    m_InspectorType = InspectorType.Device;
                }

                var rect = deviceList.contentRect;

                rect = EditorGUILayout.GetControlRect(false, rect.height);

                TryDoGUI(() => m_DeviceTreeView.OnGUI(rect));
            };

            var inspector = rootVisualElement.Q<IMGUIContainer>("inspector");
            inspector.schedule.Execute(inspector.MarkDirtyRepaint).Every(k_InspectorRepaintIntervalMS);

            inspector.onGUIHandler = () =>
            {
                HandleEditorTargetDeletedExternally();

                using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
                using (new InspectorScope(inspector.contentRect.width))
                {
                    m_ScrollPosition = scrollView.scrollPosition;

                    if (m_InspectorType == InspectorType.Shot)
                    {
                        DrawShotInspector();
                    }
                    else
                    {
                        DrawDeviceInspector();
                    }
                }
            };

            var addMenu = rootVisualElement.Q<ToolbarMenu>(IDs.AddMenu);

            SetupAddMenu(addMenu);
        }

        void SetupAddMenu(ToolbarMenu menu)
        {
            if (s_CreateDeviceMenuItems == null)
            {
                var allTypes = AttributeUtility.GetAllTypes<CreateDeviceMenuItemAttribute>();
                var assemblyName = "Unity.LiveCapture.Mocap";
                var mocapGroupTypeName = "Unity.LiveCapture.Mocap.MocapGroup";
                var mocapDeviceTypeName = "Unity.LiveCapture.Mocap.IMocapDevice";
                var mocapGroupType = Type.GetType($"{mocapGroupTypeName}, {assemblyName}");
                var mocapDeviceType = Type.GetType($"{mocapDeviceTypeName}, {assemblyName}");

                Debug.Assert(mocapGroupType != null);
                Debug.Assert(mocapDeviceType != null);

                var hasMocapDevices = allTypes
                    .Where(t => mocapDeviceType.IsAssignableFrom(t.type))
                    .Any();

                if (hasMocapDevices)
                {
                    s_CreateDeviceMenuItems = allTypes;
                }
                else
                {
                    // Do not show MocapGroup in the create device menu if there is no
                    // MocapDevice<T> implemented.
                    s_CreateDeviceMenuItems = allTypes.Where(t => t.type != mocapGroupType);
                }
            }

            MenuUtility.SetupMenu(menu.menu, s_CreateDeviceMenuItems, (t) => true, (type, attribute) =>
            {
                CreateDevice(type);
            });
        }

        void CreateDevice(Type type)
        {
            Debug.Assert(type != null);

            var lastDevice = LiveCaptureDeviceManager.Instance.Devices.LastOrDefault();
            var go = new GameObject($"New {type.Name}", type);

            GameObjectUtility.EnsureUniqueNameForSibling(go);

            var device = go.GetComponent<LiveCaptureDevice>();

            device.SortingOrder = lastDevice != null ? lastDevice.SortingOrder + 1 : 0;

            Undo.RegisterCreatedObjectUndo(go, Contents.UndoCreateDevice);

            ReloadDeviceTreeViewIfNeeded();

            m_DeviceTreeView.Select(device);

            m_InspectorType = InspectorType.Device;
        }

        void DrawShotInspector()
        {
            if (TryGetSelectedProvider(out var provider))
            {
                var context = provider.GetContext();

                if (context == null)
                {
                    provider.OnNoContextGUI();
                }
                else
                {
                    var editor = CreateCachedEditor(context);

                    TryDoGUI(() =>
                    {
                        editor.OnInspectorGUI();
                        editor.DrawBindingsInspector();
                    });
                }
            }
        }

        void DrawDeviceInspector()
        {
            if (Event.current.type == EventType.Layout)
            {
                var devices = m_DeviceTreeView.SelectedDevices;

                Editor.CreateCachedEditor(devices, null, ref m_Editor);
            }

            if (m_Editor != null)
            {
                m_Editor.OnInspectorGUI();
            }
        }

        void Update()
        {
            UpdateRedTint();
            ReloadDeviceTreeViewIfNeeded();
            UpdateContextFromSelection();

            if (TryGetSelectedProvider(out var provider))
            {
                provider.Update();
            }
        }

        void UpdateRedTint()
        {
            m_RedTint.visible = TakeRecorder.IsRecording();
        }

        void HandleEditorTargetDeletedExternally()
        {
            if (m_Editor != null && m_Editor.target == null)
            {
                DestroyImmediate(m_Editor);
            }
        }

        bool TryGetSelectedProvider(out TakeRecorderContextProvider provider)
        {
            provider = null;

            if (m_Selection >= 0 && m_Selection < m_Providers.Count)
            {
                provider = m_Providers[m_Selection];
            }

            return provider != null;
        }

        TakeRecorderContextEditor CreateCachedEditor(ITakeRecorderContext context)
        {
            return m_EditorCache.CreateCachedEditor(context);
        }

        void DoContextDropdown(Rect rect)
        {
            var content = Contents.NoContext;

            if (TryGetSelectedProvider(out var provider))
            {
                content = new GUIContent(provider.DisplayName);
            }

            if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                var formatter = new UniqueNameFormatter();

                for (var i = 0; i < m_Providers.Count; ++i)
                {
                    var name = formatter.Format(m_Providers[i].DisplayName);

                    menu.AddItem(new GUIContent(name), i == m_Selection, OnSelect, i);
                }

                menu.ShowAsContext();
            }
        }

        void OnSelect(object obj)
        {
            m_Selection = (int)obj;
        }

        void UpdateContextFromSelection()
        {
            if (TryGetSelectedProvider(out var provider))
            {
                TakeRecorder.SetContext(provider.GetContext());
            }
            else
            {
                TakeRecorder.SetContext(null);
            }
        }

        void DoGoToBeginningButton()
        {
            using (new EditorGUI.DisabledScope(TakeRecorder.IsRecording()))
            {
                if (GUILayout.Button(Contents.GotoBeginningContent, EditorStyles.toolbarButton))
                {
                    TakeRecorder.GoToBeginning();
                }
            }
        }

        void DoPlayPauseButton()
        {
            var content = TakeRecorder.IsPreviewPlaying() ? Contents.PausePreviewLabel : Contents.PlayPreviewLabel;

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Toggle(TakeRecorder.IsPreviewPlaying(), content, EditorStyles.toolbarButton);

                if (change.changed)
                {
                    if (TakeRecorder.IsPreviewPlaying())
                    {
                        TakeRecorder.PausePreview();
                    }
                    else
                    {
                        TakeRecorder.PlayPreview();
                    }
                }
            }
        }

        void DoPlayTakeContentsToggle()
        {
            using (new EditorGUI.DisabledScope(TakeRecorder.IsRecording()))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var value = GUILayout.Toggle(
                    TakeRecorderImpl.Instance.PlayTakeContents,
                    Contents.PlayTakeContents,
                    EditorStyles.toolbarButton);

                if (change.changed)
                {
                    TakeRecorderImpl.Instance.PlayTakeContents = value;
                }
            }
        }

        void DoRecordButton(Rect rect)
        {
            var canRecord = TakeRecorderImpl.Instance.CanStartRecording();
            var contents = TakeRecorder.IsRecording()
                ? Contents.StopRecording.Resolve(rect.width)
                : Contents.Record.Resolve(rect.width);

            using (new EditorGUI.DisabledScope(!canRecord))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                GUI.Toggle(rect, TakeRecorder.IsRecording(), contents, EditorStyles.toolbarButton);

                if (change.changed)
                {
                    if (TakeRecorder.IsRecording())
                    {
                        TakeRecorder.StopRecording();
                    }
                    else
                    {
                        TakeRecorder.StartRecording();
                    }
                }
            }
        }

        void DoOpenProjectSettingsButton()
        {
            if (GUILayout.Button(Contents.OpenSettingsIcon, EditorStyles.toolbarButton, GUILayout.Width(26f)))
            {
                LiveCaptureSettingsProvider.Open();
            }
        }

        void DoLiveReviewButton(Rect rect)
        {
            using var disabled = new EditorGUI.DisabledScope(TakeRecorder.IsRecording());
            using var horizontal = new EditorGUILayout.HorizontalScope();

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                if (TakeRecorder.IsLive)
                {
                    var r = new RectOffset(2, 2, 2, 2).Add(rect);

                    GUI.BeginGroup(rect);
                    GUI.Toggle(r, true, GUIContent.none, Contents.ToolbarButtonBlue);
                    GUI.EndGroup();
                }
                else
                {
                    GUI.Toggle(rect, false, GUIContent.none, EditorStyles.toolbarButton);
                }


                if (change.changed)
                {
                    TakeRecorder.IsLive = !TakeRecorder.IsLive;
                }
            }

            GUI.Label(rect, Contents.Live.Resolve(rect.width), Contents.LiveLabelStyle);
        }

        void DoRecordingInfoBox()
        {
            var canRecord = TakeRecorderImpl.Instance.CanStartRecording();

            if (!canRecord)
            {
                var deviceCount = LiveCaptureDeviceManager.Instance.Devices.Count;
                var message = Contents.GetCanRecordStatusMessage(deviceCount, TakeRecorder.Shot.HasValue);

                LiveCaptureGUI.HelpStatusWithURL(message, Contents.ReadMoreText, Contents.CreateDeviceURL, MessageType.Warning);
            }
        }

        void TryDoGUI(Action onGUI)
        {
            try
            {
                onGUI?.Invoke();
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
