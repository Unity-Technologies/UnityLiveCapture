using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    /// <summary>
    /// A window to view and edit synchronizers.
    /// </summary>
    public class SynchronizationWindow : EditorWindow
    {
        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";

            public const string WindowName = "Synchronization";
            public const string WindowPath = "Window/Live Capture/" + WindowName;
            public static readonly GUIContent WindowTitle = EditorGUIUtility.TrTextContentWithIcon(WindowName, $"{k_IconPath}/LiveCaptureConnectionWindow.png");
            public static readonly Vector2 WindowSize = new Vector2(300f, 100f);
            public static readonly float IndentSize = 16f;
            public static readonly GUIContent GenlockLabel = EditorGUIUtility.TrTextContent("Genlock Status");
            public static readonly GUIContent GenlockNameLabel = EditorGUIUtility.TrTextContent("Source Name");
            public static readonly GUIContent GenlockStatusLabel = EditorGUIUtility.TrTextContent("Sync Status");
            public static readonly GUIContent GenlockSyncRateLabel = EditorGUIUtility.TrTextContent("Sync Rate", "The pulse rate of the synchronization signal.");
            public static readonly GUIContent DroppedFramesLabel = EditorGUIUtility.TrTextContent("Dropped Frames", "The number of synchronization signal pulses that have been skipped since activating genlock.");
            public static readonly GUIContent NoSyncProviderLabel = EditorGUIUtility.TrTextContent("No genlock provider active.");
            public static readonly GUIContent SynchronizerLabel = EditorGUIUtility.TrTextContent("Synchronizers");
            public static readonly GUIContent SynchronizerStatusLabel = EditorGUIUtility.TrTextContent("Synchronizer Status");
            public static readonly GUIContent TimedDataSourceDetailsLabel = EditorGUIUtility.TrTextContent(
                "Open Timed Data Source Details",
                "Open a dedicated window to control and monitor timecode synchronization of all data sources in detail.");

            public static readonly Color HeaderColor = EditorGUIUtility.isProSkin ? new Color32(0x2c, 0x2c, 0x2c, 0xff) : new Color32(0xb5, 0xb5, 0xb5, 0xff);
            public static readonly Color SeparatorColor = new Color32(0x18, 0x18, 0x18, 0xff);

            static GUIStyle s_HeaderStyle;
            public static GUIStyle HeaderStyle
            {
                get
                {
                    if (s_HeaderStyle == null)
                    {
                        s_HeaderStyle = new GUIStyle(EditorStyles.foldout)
                        {
                            fontStyle = FontStyle.Bold,
                        };
                    }
                    return s_HeaderStyle;
                }
            }
        }

        [SerializeField]
        Vector2 m_Scroll;
        [SerializeField]
        bool m_ShowGenlockStatus = true;
        [SerializeField]
        bool m_ShowSynchronizerStatus = true;
        [SerializeField]
        SynchronizerComponent m_Synchronizer;

        Editor m_Editor;

        ReorderableList m_SynchronizerList;

        /// <summary>
        /// Opens an instance of the synchronization window.
        /// </summary>
        [MenuItem(Contents.WindowPath)]
        public static void ShowWindow()
        {
            GetWindow<SynchronizationWindow>();
        }

        void OnEnable()
        {
            minSize = Contents.WindowSize;
            titleContent = Contents.WindowTitle;

            SynchronizerComponent.SynchronizersChanged += OnSynchronizersChanged;

            CreateSynchronizerList();
        }

        void OnDisable()
        {
            if (m_Editor != null)
                DestroyImmediate(m_Editor);

            SynchronizerComponent.SynchronizersChanged -= OnSynchronizersChanged;
        }

        void OnSynchronizersChanged()
        {
            Repaint();
        }

        void Update()
        {
            if (m_Synchronizer == null && m_SynchronizerList.list.Count > 0)
            {
                var index = Mathf.Clamp(m_SynchronizerList.index, 0, m_SynchronizerList.list.Count - 1);
                m_Synchronizer = m_SynchronizerList.list[index] as SynchronizerComponent;
            }

            if (m_Synchronizer != null && m_ShowSynchronizerStatus)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                Repaint();
            }

            m_SynchronizerList.index = m_SynchronizerList.list.IndexOf(m_Synchronizer);
        }

        void CreateSynchronizerList()
        {
            if (m_SynchronizerList != null)
                return;

            m_SynchronizerList = new ReorderableList(
                (IList)SynchronizerComponent.Synchronizers,
                typeof(SynchronizerComponent),
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, Contents.SynchronizerLabel);
                },
                onSelectCallback = (list) =>
                {
                    m_Synchronizer = list.list[list.index] as SynchronizerComponent;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = m_SynchronizerList.list[index] as SynchronizerComponent;

                    rect.y -= 0.5f * (EditorGUIUtility.singleLineHeight - rect.height);
                    rect.height = EditorGUIUtility.singleLineHeight;

                    var labelRect = rect;
                    var fieldRect = rect;

                    labelRect.width = 0.5f * rect.width;
                    fieldRect.xMin = labelRect.xMax;

                    EditorGUI.LabelField(labelRect, element.name);

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.ObjectField(fieldRect, GUIContent.none, element, typeof(SynchronizerComponent), true);
                    }
                },
                onAddDropdownCallback = (rect, list) =>
                {
                    Selection.activeObject = null;
                    var go = ObjectCreatorUtilities.CreateSynchronizer();
                    EditorGUIUtility.PingObject(go);
                    m_Synchronizer = go.GetComponent<SynchronizerComponent>();
                },
                onRemoveCallback = list =>
                {
                    var element = list.list[list.index] as SynchronizerComponent;
                    var go = element.gameObject;

                    // delete the entire object if it only has a transform and synchronizer component
                    if (go.GetComponents(typeof(Component)).Length == 2)
                    {
                        Undo.DestroyObjectImmediate(go);
                    }
                    else
                    {
                        Undo.DestroyObjectImmediate(element);
                    }
                }
            };
        }

        void OnGUI()
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(m_Scroll);
            m_Scroll = scrollView.scrollPosition;

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(Contents.IndentSize);

                using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(float.MaxValue)))
                {
                    m_SynchronizerList.DoLayoutList();

                    EditorGUILayout.Space();

                    if (GUILayout.Button(Contents.TimedDataSourceDetailsLabel))
                    {
                        TimedDataSourceViewerWindow.ShowWindow();
                    }
                }
            }

            EditorGUILayout.Space();

            DoSynchronizerStatusGUI();
            DoGenlockStatusGUI();
        }

        void DoGenlockStatusGUI()
        {
            DoHeaderLayout(ref m_ShowGenlockStatus, Contents.GenlockLabel);

            if (!m_ShowGenlockStatus)
            {
                return;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(Contents.IndentSize);

                using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(float.MaxValue)))
                {
                    var syncProvider = SyncManager.Instance.ActiveSyncProvider;

                    if (syncProvider != null)
                    {
                        var nameRect = EditorGUILayout.GetControlRect();
                        nameRect = EditorGUI.PrefixLabel(nameRect, Contents.GenlockNameLabel);
                        EditorGUI.LabelField(nameRect, syncProvider.Name);

                        var statusRect = EditorGUILayout.GetControlRect();
                        statusRect = EditorGUI.PrefixLabel(statusRect, Contents.GenlockStatusLabel);
                        EditorGUI.LabelField(statusRect, syncProvider.Status.ToString());

                        var syncRateRect = EditorGUILayout.GetControlRect();
                        syncRateRect = EditorGUI.PrefixLabel(syncRateRect, Contents.GenlockSyncRateLabel);
                        EditorGUI.LabelField(syncRateRect, syncProvider.SyncRate.ToString());

                        var droppedFramesRect = EditorGUILayout.GetControlRect();
                        droppedFramesRect = EditorGUI.PrefixLabel(droppedFramesRect, Contents.DroppedFramesLabel);
                        EditorGUI.LabelField(droppedFramesRect, syncProvider.DroppedFrameCount.ToString());
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(Contents.NoSyncProviderLabel.text, MessageType.Info);
                    }
                }
            }
        }

        void DoSynchronizerStatusGUI()
        {
            if (m_Synchronizer == null)
            {
                return;
            }

            DoHeaderLayout(ref m_ShowSynchronizerStatus, Contents.SynchronizerStatusLabel);

            if (!m_ShowSynchronizerStatus)
            {
                return;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(Contents.IndentSize);

                using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(float.MaxValue)))
                {
                    if (m_Editor != null && m_Editor.target == null)
                    {
                        DestroyImmediate(m_Editor);
                    }

                    Editor.CreateCachedEditor(m_Synchronizer, null, ref m_Editor);

                    if (m_Editor is SynchronizerEditor synchronizerEditor)
                    {
                        synchronizerEditor.DoTimecodeSourceGUI();
                        synchronizerEditor.DoDelayGUI();
                        synchronizerEditor.DoPresentTimecodeGUI();

                        EditorGUILayout.Space();

                        synchronizerEditor.DoSourcesGUI();
                        synchronizerEditor.DoCalibrationGUI();
                    }
                }
            }
        }

        void DoHeaderLayout(ref bool isExpanded, GUIContent title)
        {
            var height = 22f;

            var headerRect = EditorGUILayout.GetControlRect(GUILayout.Height(height - 2f));
            headerRect.x = 0f;
            headerRect.height = 22f;
            headerRect.width = position.width;

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(headerRect, Contents.HeaderColor);

                var separatorRect = new Rect(headerRect)
                {
                    height = 1f,
                };

                EditorGUI.DrawRect(separatorRect, Contents.SeparatorColor);
            }

            var foldoutRect = new Rect(headerRect)
            {
                xMin = 5f,
            };

            isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, title, true, Contents.HeaderStyle);

            if (Event.current.type == EventType.Repaint)
            {
                var separatorRect = new Rect(headerRect)
                {
                    y = headerRect.yMax,
                    height = 1f,
                };

                EditorGUI.DrawRect(separatorRect, Contents.SeparatorColor);
            }
        }
    }
}
