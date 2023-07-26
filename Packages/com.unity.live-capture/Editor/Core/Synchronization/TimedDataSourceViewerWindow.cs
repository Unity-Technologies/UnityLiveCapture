using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A window to view and edit synchronized data sources.
    /// </summary>
    public class TimedDataSourceViewerWindow : EditorWindow
    {
        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";

            public const string WindowName = "Timed Data Source Details";
            public const string WindowPath = "Window/Live Capture/" + WindowName;
            public static readonly GUIContent WindowTitle = EditorGUIUtility.TrTextContentWithIcon(WindowName, $"{k_IconPath}/LiveCaptureConnectionWindow.png");
            public static readonly Vector2 WindowSize = new Vector2(500f, 100f);

            public static readonly string FieldUndo = "Inspector";
            public static readonly GUIContent NoValueText = EditorGUIUtility.TrTextContent("N/A");
            public static readonly GUIContent LeftArrowIcon = EditorGUIUtility.IconContent("back@2x");
            public static readonly GUIContent RightArrowIcon = EditorGUIUtility.IconContent("forward@2x");

            public static readonly Color GraphWhite = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.1f, 0.1f, 0.1f);
            public static readonly Color GraphGreen = new Color32(0x53, 0xc2, 0x2b, 0xff);
            public static readonly Color GraphRed = new Color32(0xff, 0x43, 0x43, 0xff);

            static GUIStyle s_CenteredStyle;
            public static GUIStyle CenteredStyle
            {
                get
                {
                    if (s_CenteredStyle == null)
                    {
                        s_CenteredStyle = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.UpperCenter,
                        };
                    }
                    return s_CenteredStyle;
                }
            }

            static GUIStyle s_ArrowStyle;
            public static GUIStyle ArrowStyle
            {
                get
                {
                    if (s_CenteredStyle == null)
                    {
                        s_CenteredStyle = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                        };
                        s_CenteredStyle.normal.textColor = GraphWhite;
                    }
                    return s_CenteredStyle;
                }
            }
        }

        static readonly int[] k_TimingGraphAxisIncrements = { 1, 2, 5, 10, 20, 50, 100, 200, 500 };
        const float k_TimingGraphSpaceBetweenNumbers = 40f;
        const float k_TimingGraphNumberToLineSpace = 1.0f;
        const float k_TimingGraphNumberLineHeight = 5.0f;
        const float k_TimingGraphNumberLineWidth = 2.0f;
        const float k_TimingGraphTimeLineWidth = 1.75f;
        const float k_TimingGraphScrollSensitivity = 0.05f;
        const float k_TimingGraphMinPixelsPerFrame = 0.25f;
        const float k_TimingGraphMaxPixelsPerFrame = 100f;

        struct DataSourceState
        {
            public FrameRate FrameRate;
            public FrameTime? OldestSampleTime;
            public FrameTime? NewestSampleTime;
        }

        static float s_TimingGraphPixelsPerFrame = 8f;
        static readonly Dictionary<ISynchronizer, FrameTimeWithRate?> s_SyncTimeCache = new Dictionary<ISynchronizer, FrameTimeWithRate?>();
        static readonly Dictionary<ITimedDataSource, DataSourceState> s_DataSourceStateCache = new Dictionary<ITimedDataSource, DataSourceState>();

        [SerializeField]
        TreeViewState m_TreeViewState;
        [SerializeField]
        MultiColumnHeaderState m_TreeViewHeaderState;

        TimedDataTreeView m_TreeView;
        bool m_TreeDirty;

        /// <summary>
        /// Opens an instance of the timed data source window.
        /// </summary>
        [MenuItem(Contents.WindowPath)]
        public static void ShowWindow()
        {
            GetWindow<TimedDataSourceViewerWindow>();
        }

        void OnEnable()
        {
            minSize = Contents.WindowSize;
            titleContent = Contents.WindowTitle;

            CreateTreeView();
        }

        void Update()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            Repaint();

            m_TreeDirty = true;
            s_SyncTimeCache.Clear();
            s_DataSourceStateCache.Clear();
        }

        void OnGUI()
        {
            if (m_TreeView == null)
            {
                CreateTreeView();
            }

            if (m_TreeDirty)
            {
                m_TreeView.Reload();
                m_TreeDirty = false;
            }

            m_TreeView.OnGUI(new Rect(0, 0, position.width, position.height));
        }

        enum TreeViewColumns
        {
            Source,
            Status,
            Buffer,
            Offset,
            FrameRate,
            OldestSample,
            NewestSample,
            TimingGraph,
        }

        class SynchronizerItem : TreeViewItem
        {
            public readonly SynchronizerComponent Synchronizer;

            public SynchronizerItem(SynchronizerComponent synchronizer)
            {
                Synchronizer = synchronizer;
            }
        }

        class TimedDataSourceItem : TreeViewItem
        {
            public readonly SynchronizerComponent Synchronizer;
            public readonly int Index;

            public TimedDataSourceItem(SynchronizerComponent synchronizer, int index)
            {
                Synchronizer = synchronizer;
                Index = index;
            }
        }

        class TimedDataTreeView : TreeView
        {
            public TimedDataTreeView(TreeViewState treeViewState, MultiColumnHeader header) : base(treeViewState, header)
            {
                showBorder = true;
                useScrollView = true;
                showAlternatingRowBackgrounds = true;
                rowHeight = 22f;

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var id = 0;

                var rootItem = new TreeViewItem
                {
                    id = id++,
                    depth = -1,
                    displayName = "Root",
                };

                for (var i = 0; i < SynchronizerComponent.Synchronizers.Count; i++)
                {
                    var synchronizer = SynchronizerComponent.Synchronizers[i];

                    if (synchronizer == null)
                    {
                        continue;
                    }

                    var synchronizerItem = new SynchronizerItem(synchronizer)
                    {
                        id = id++,
                        depth = 0,
                    };

                    for (var j = 0; j < synchronizer.Impl.DataSourceCount; j++)
                    {
                        var dataSource = synchronizer.Impl.GetDataSource(j);

                        if (dataSource == null)
                        {
                            continue;
                        }

                        var dataSourceItem = new TimedDataSourceItem(synchronizer, j)
                        {
                            id = id++,
                            depth = 1,
                        };

                        synchronizerItem.AddChild(dataSourceItem);
                    }

                    rootItem.AddChild(synchronizerItem);
                }

                if (rootItem.children == null)
                {
                    rootItem.children = new List<TreeViewItem>();
                }

                return rootItem;
            }

            /// <inheritdoc />
            protected override float GetCustomRowHeight(int row, TreeViewItem item)
            {
                if (item.depth == 0 && IsExpanded(item.id))
                {
                    return (2f * EditorGUIUtility.singleLineHeight) + 6f;
                }

                return base.GetCustomRowHeight(row, item);
            }

            /// <inheritdoc />
            protected override void RowGUI(RowGUIArgs args)
            {
                switch (args.item)
                {
                    case SynchronizerItem synchronizerItem:
                    {
                        var synchronizer = synchronizerItem.Synchronizer;

                        if (synchronizer == null)
                        {
                            return;
                        }

                        for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                        {
                            var column = (TreeViewColumns)args.GetColumn(i);
                            var rect = args.GetCellRect(i);

                            switch (column)
                            {
                                case TreeViewColumns.Source:
                                {
                                    // draw the synchronizer name
                                    rect.xMin = GetContentIndent(synchronizerItem);
                                    rect.y += 2;
                                    rect.height = EditorGUIUtility.singleLineHeight;
                                    EditorGUI.LabelField(rect, synchronizer.name);

                                    // toggle the foldout when the label is clicked
                                    var e = Event.current;

                                    if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                                    {
                                        SetExpanded(synchronizerItem.id, !IsExpanded(synchronizerItem.id));
                                    }

                                    // draw the timecode source name when expanded
                                    var timecodeSource = synchronizer.Impl.TimecodeSource;

                                    if (IsExpanded(synchronizerItem.id) && timecodeSource != null)
                                    {
                                        rect.xMin += 12f;
                                        rect.y += EditorGUIUtility.singleLineHeight;
                                        EditorGUI.LabelField(rect, timecodeSource.FriendlyName);
                                    }
                                    break;
                                }
                                case TreeViewColumns.Offset:
                                {
                                    if (IsExpanded(synchronizerItem.id))
                                    {
                                        using (var change = new EditorGUI.ChangeCheckScope())
                                        {
                                            CenterRectUsingSingleLineHeight(ref rect);
                                            rect.y = rect.center.y;
                                            rect.height = EditorGUIUtility.singleLineHeight;

                                            var delay = FrameNumberDrawer.DoField(rect, GUIContent.none, synchronizer.Impl.Delay);

                                            if (change.changed)
                                            {
                                                Undo.RegisterCompleteObjectUndo(synchronizer, Contents.FieldUndo);
                                                synchronizer.Impl.Delay = delay;
                                                EditorUtility.SetDirty(synchronizer);
                                            }
                                        }
                                    }

                                    EditorGUI.LabelField(rect, new GUIContent(string.Empty, SynchronizerEditor.Contents.DelayLabel.tooltip));
                                    break;
                                }
                                case TreeViewColumns.FrameRate:
                                {
                                    if (IsExpanded(synchronizerItem.id))
                                    {
                                        CenterRectUsingSingleLineHeight(ref rect);
                                        rect.y = rect.center.y;
                                        rect.height = EditorGUIUtility.singleLineHeight;

                                        var frameRate = synchronizer.Impl.TimecodeSource?.FrameRate;

                                        if (frameRate != null && frameRate.Value.IsValid)
                                        {
                                            EditorGUI.LabelField(rect, frameRate.ToString());
                                        }
                                    }

                                    break;
                                }
                                case TreeViewColumns.TimingGraph:
                                {
                                    if (IsExpanded(synchronizerItem.id))
                                    {
                                        DoTimingGraphTimecodeGUI(rect, synchronizer.Impl);
                                        DoTimingGraphAxisGUI(rect);
                                    }
                                    break;
                                }
                            }
                        }

                        break;
                    }
                    case TimedDataSourceItem dataSourceItem:
                    {
                        var synchronizerComponent = dataSourceItem.Synchronizer;

                        if (synchronizerComponent == null)
                        {
                            return;
                        }

                        var synchronizer = synchronizerComponent.Impl;
                        var sourceAndStatus = synchronizer.GetSourceAndStatus(dataSourceItem.Index);
                        var source = sourceAndStatus.Source;

                        for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                        {
                            var column = (TreeViewColumns)args.GetColumn(i);
                            var rect = args.GetCellRect(i);

                            switch (column)
                            {
                                case TreeViewColumns.Source:
                                {
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    rect.xMin = GetContentIndent(dataSourceItem);
                                    SourceAndStatusBundlePropertyDrawer.DoIsSynchronizedGUI(rect, synchronizerComponent, sourceAndStatus);
                                    rect.xMin += 18;
                                    SourceAndStatusBundlePropertyDrawer.DoSourceNameGUI(rect, source);
                                    break;
                                }
                                case TreeViewColumns.Status:
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    SourceAndStatusBundlePropertyDrawer.DoStatusGUI(rect, sourceAndStatus);
                                    break;
                                case TreeViewColumns.Buffer:
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    SourceAndStatusBundlePropertyDrawer.DoBufferSizeGUI(rect, source);
                                    break;
                                case TreeViewColumns.Offset:
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    SourceAndStatusBundlePropertyDrawer.DoOffsetGUI(rect, source);
                                    break;
                                case TreeViewColumns.FrameRate:
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    SourceAndStatusBundlePropertyDrawer.DoFrameRateGUI(rect, source);
                                    break;
                                case TreeViewColumns.OldestSample:
                                {
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    var frameTime = synchronizer.PresentTime;

                                    if (frameTime.HasValue && frameTime.Value.Rate.IsValid &&
                                        TryGetSourceState(source, out var state) && state.OldestSampleTime.HasValue)
                                    {
                                        var timecode = Timecode.FromFrameTime(frameTime.Value.Rate, state.OldestSampleTime.Value);
                                        EditorGUI.LabelField(rect, timecode.ToString());
                                    }
                                    else
                                    {
                                        EditorGUI.LabelField(rect, Contents.NoValueText);
                                    }
                                    break;
                                }
                                case TreeViewColumns.NewestSample:
                                {
                                    CenterRectUsingSingleLineHeight(ref rect);
                                    var frameTime = synchronizer.PresentTime;

                                    if (frameTime.HasValue && frameTime.Value.Rate.IsValid &&
                                        TryGetSourceState(source, out var state) && state.NewestSampleTime.HasValue)
                                    {
                                        var timecode = Timecode.FromFrameTime(frameTime.Value.Rate, state.NewestSampleTime.Value);
                                        EditorGUI.LabelField(rect, timecode.ToString());
                                    }
                                    else
                                    {
                                        EditorGUI.LabelField(rect, Contents.NoValueText);
                                    }
                                    break;
                                }
                                case TreeViewColumns.TimingGraph:
                                    DoTimingGraphGUI(rect, synchronizer, source);
                                    break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        static void DoTimingGraphTimecodeGUI(Rect rect, ISynchronizer synchronizer)
        {
            if (synchronizer == null || !TryGetSyncTime(synchronizer, out var syncTime) || syncTime == null)
            {
                return;
            }

            const float width = 72f;
            var LabelRect = new Rect(rect)
            {
                xMin = rect.center.x - (0.5f * width),
                xMax = rect.center.x + (0.5f * width),
                yMin = rect.yMax - (k_TimingGraphNumberLineHeight + k_TimingGraphNumberToLineSpace + (2f * EditorGUIUtility.singleLineHeight)),
                height = EditorGUIUtility.singleLineHeight,
            };

            EditorGUI.LabelField(LabelRect, syncTime.Value.ToTimecode().ToString());
        }

        static void DoTimingGraphAxisGUI(Rect rect)
        {
            DoTimingGraphZoom(rect);

            // We only execute on repaint events for performance, so we must not use any
            // methods that create a GUI control ID (GUI.Label, etc).
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var lineRect = rect;
            lineRect.yMin = rect.yMax - k_TimingGraphNumberLineHeight;

            var numberRect = rect;
            numberRect.yMax = lineRect.yMin + k_TimingGraphNumberToLineSpace;
            numberRect.yMin = numberRect.yMax - EditorGUIUtility.singleLineHeight;

            // choose a scale for the axis numbering that is suitable for the current zoom level
            var approxAxisIncrement = k_TimingGraphSpaceBetweenNumbers / s_TimingGraphPixelsPerFrame;
            float axisIncrement = k_TimingGraphAxisIncrements[0];

            if (approxAxisIncrement >= axisIncrement)
            {
                for (var i = 0; i < k_TimingGraphAxisIncrements.Length; i++)
                {
                    var lowerIncrement = k_TimingGraphAxisIncrements[i];

                    if (i == k_TimingGraphAxisIncrements.Length - 1)
                    {
                        axisIncrement = lowerIncrement;
                        break;
                    }

                    var upperIncrement = k_TimingGraphAxisIncrements[i + 1];

                    if (lowerIncrement <= approxAxisIncrement && approxAxisIncrement <= upperIncrement)
                    {
                        var lowerDelta = approxAxisIncrement - lowerIncrement;
                        var upperDelta = upperIncrement - approxAxisIncrement;
                        axisIncrement = upperDelta < lowerDelta ? upperIncrement : lowerIncrement;
                        break;
                    }
                }
            }

            // determine how many axis numbers we can fit on the graph
            const float numberLabelWidth = 40f;
            var pixelsPerNumber = axisIncrement * s_TimingGraphPixelsPerFrame;
            var maxNumberExtent = 0.5f * (rect.width - (0.5f * numberLabelWidth));
            var numberCount = 2 * Mathf.Floor(maxNumberExtent / pixelsPerNumber) + 1;

            // draw the axis numbering
            for (var i = 0; i < numberCount; i++)
            {
                var frameNumber = (i - ((int)numberCount / 2)) * axisIncrement;
                var center = rect.center.x + (frameNumber * s_TimingGraphPixelsPerFrame);

                lineRect.xMin = center - (0.5f * k_TimingGraphNumberLineWidth);
                lineRect.xMax = center + (0.5f * k_TimingGraphNumberLineWidth);

                numberRect.xMin = center - (0.5f * numberLabelWidth);
                numberRect.xMax = center + (0.5f * numberLabelWidth);

                Contents.CenteredStyle.Draw(numberRect, frameNumber.ToString(), false, false, false, false);
                EditorGUI.DrawRect(lineRect, Contents.GraphWhite);
            }
        }

        static void DoTimingGraphGUI(Rect rect, ISynchronizer synchronizer, ITimedDataSource dataSource)
        {
            DoTimingGraphZoom(rect);

            // We only execute on repaint events for performance, so we must not use any
            // methods that create a GUI control ID (GUI.Label, etc).
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var center = rect.center.x;
            var globalTimeLineRect = rect;
            globalTimeLineRect.xMin = center - (0.5f * k_TimingGraphTimeLineWidth);
            globalTimeLineRect.xMax = center + (0.5f * k_TimingGraphTimeLineWidth);

            EditorGUI.DrawRect(globalTimeLineRect, Contents.GraphRed);

            if (synchronizer == null || !TryGetSyncTime(synchronizer, out var syncTime) || syncTime == null)
            {
                return;
            }
            if (dataSource == null || !TryGetSourceState(dataSource, out var state) || state.OldestSampleTime == null || state.NewestSampleTime == null)
            {
                return;
            }

            // Check if the buffer rect is visible in the graph. If not, show an arrow indicating which direction it is in.
            var bufferMin = center + s_TimingGraphPixelsPerFrame * (float)(state.OldestSampleTime - syncTime.Value.Time);
            var bufferMax = center + s_TimingGraphPixelsPerFrame * (float)(state.NewestSampleTime - syncTime.Value.Time);

            const float margin = 10f;

            if (bufferMax < rect.xMin - margin)
            {
                var arrowRect = rect;
                arrowRect.xMax = arrowRect.xMin + arrowRect.height;
                EditorStyles.label.Draw(arrowRect, Contents.LeftArrowIcon, false, false, false, false);
            }
            else if (bufferMin > rect.xMax + margin)
            {
                var arrowRect = rect;
                arrowRect.xMin = arrowRect.xMax - arrowRect.height;
                EditorStyles.label.Draw(arrowRect, Contents.RightArrowIcon, false, false, false, false);
            }
            else
            {
                var BufferRect = rect;
                BufferRect.xMin = Mathf.Clamp(bufferMin, rect.xMin, rect.xMax);
                BufferRect.xMax = Mathf.Clamp(bufferMax, rect.xMin, rect.xMax);
                BufferRect.yMin += 4f;
                BufferRect.yMax -= 4f;

                EditorGUI.DrawRect(BufferRect, Contents.GraphWhite);

                if (state.OldestSampleTime <= syncTime.Value.Time && syncTime.Value.Time <= state.NewestSampleTime)
                {
                    EditorGUI.DrawRect(globalTimeLineRect, Contents.GraphGreen);
                }
            }
        }

        static void DoTimingGraphZoom(Rect rect)
        {
            var e = Event.current;

            if (e.type == EventType.ScrollWheel && rect.Contains(e.mousePosition))
            {
                var pixelsPerFrame = s_TimingGraphPixelsPerFrame * Mathf.Exp(-k_TimingGraphScrollSensitivity * e.delta.y);
                s_TimingGraphPixelsPerFrame = Mathf.Clamp(pixelsPerFrame, k_TimingGraphMinPixelsPerFrame, k_TimingGraphMaxPixelsPerFrame);
            }
        }

        static bool TryGetSyncTime(ISynchronizer synchronizer, out FrameTimeWithRate? syncTime)
        {
            if (synchronizer == null)
            {
                syncTime = default;
                return false;
            }

            if (!s_SyncTimeCache.TryGetValue(synchronizer, out syncTime))
            {
                syncTime = synchronizer.PresentTime;
                s_SyncTimeCache.Add(synchronizer, syncTime);
            }

            return true;
        }

        static bool TryGetSourceState(ITimedDataSource source, out DataSourceState state)
        {
            if (source == null)
            {
                state = default;
                return false;
            }

            if (!s_DataSourceStateCache.TryGetValue(source, out state))
            {
                state = new DataSourceState
                {
                    FrameRate = source.FrameRate,
                };

                var syncFrameRate = source.Synchronizer?.PresentTime?.Rate;

                if (syncFrameRate != null && syncFrameRate.Value.IsValid && source.TryGetBufferRange(out var oldestSample, out var newestSample))
                {
                    state.OldestSampleTime = FrameTime.Remap(oldestSample, source.FrameRate, syncFrameRate.Value);
                    state.NewestSampleTime = FrameTime.Remap(newestSample, source.FrameRate, syncFrameRate.Value);
                }

                s_DataSourceStateCache.Add(source, state);
            }

            return true;
        }

        void CreateTreeView()
        {
            if (m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }

            if (m_TreeViewHeaderState == null)
            {
                m_TreeViewHeaderState = new MultiColumnHeaderState(new[]
                {
                    new MultiColumnHeaderState.Column
                    {
                        width = 200f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 60f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 100f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 50f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 80f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 100f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 100f,
                    },
                    new MultiColumnHeaderState.Column
                    {
                        width = 250f,
                    },
                })
                {
                    // choose the columns that are added to the view by default
                    visibleColumns = new[]
                    {
                        TreeViewColumns.Source,
                        TreeViewColumns.Status,
                        TreeViewColumns.Buffer,
                        TreeViewColumns.Offset,
                        TreeViewColumns.FrameRate,
                        TreeViewColumns.OldestSample,
                        TreeViewColumns.NewestSample,
                        TreeViewColumns.TimingGraph,
                    }.Select(e => (int)e).ToArray(),
                };
            }

            for (var i = 0; i < m_TreeViewHeaderState.columns.Length; i++)
            {
                var column = m_TreeViewHeaderState.columns[i];

                switch ((TreeViewColumns)i)
                {
                    case TreeViewColumns.Source:
                    {
                        column.headerContent = new GUIContent("Source");
                        column.minWidth = 120f;
                        break;
                    }
                    case TreeViewColumns.Status:
                    {
                        column.headerContent = new GUIContent("Status");
                        column.minWidth = 60f;
                        break;
                    }
                    case TreeViewColumns.Buffer:
                    {
                        column.headerContent = new GUIContent("Buffer");
                        column.minWidth = 50f;
                        column.maxWidth = 250f;
                        break;
                    }
                    case TreeViewColumns.Offset:
                    {
                        column.headerContent = new GUIContent("Offset");
                        column.minWidth = 50f;
                        column.maxWidth = 50f;
                        column.width = 50f;
                        break;
                    }
                    case TreeViewColumns.FrameRate:
                    {
                        column.headerContent = new GUIContent("Frame Rate");
                        column.minWidth = 80f;
                        column.maxWidth = 80f;
                        column.width = 80f;
                        break;
                    }
                    case TreeViewColumns.OldestSample:
                    {
                        column.headerContent = new GUIContent("Oldest Sample");
                        column.minWidth = 100f;
                        column.maxWidth = 100f;
                        column.width = 100f;
                        break;
                    }
                    case TreeViewColumns.NewestSample:
                    {
                        column.headerContent = new GUIContent("Newest Sample");
                        column.minWidth = 100f;
                        column.maxWidth = 100f;
                        column.width = 100f;
                        break;
                    }
                    case TreeViewColumns.TimingGraph:
                    {
                        column.headerContent = new GUIContent("Timing Graph");
                        column.minWidth = 250f;
                        break;
                    }
                }
            }

            var header = new MultiColumnHeader(m_TreeViewHeaderState)
            {
                canSort = false,
                height = 24f,
            };

            m_TreeView = new TimedDataTreeView(m_TreeViewState, header);
        }
    }
}
