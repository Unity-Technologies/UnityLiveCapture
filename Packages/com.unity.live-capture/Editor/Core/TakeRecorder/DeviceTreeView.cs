using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    class DeviceTreeView
    {
        const float k_SmallColumnWidth = 22f;

        public enum Columns
        {
            DragHandle,
            Status,
            Active,
            Devices
        }

        [SerializeField]
        TreeViewState m_TreeViewState = new TreeViewState();

        DeviceTreeViewImpl m_Impl;

        public LiveCaptureDevice[] SelectedDevices
        {
            get
            {
                InitializeIfNeeded();

                return m_Impl.SelectedDevices;
            }
        }

        public void OnGUI(Rect rect)
        {
            InitializeIfNeeded();

            Debug.Assert(m_Impl != null);

            m_Impl.OnGUI(rect);
        }

        public void Reload()
        {
            InitializeIfNeeded();

            Debug.Assert(m_Impl != null);

            m_Impl.Reload();
        }

        public void Select(LiveCaptureDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var id = device.GetInstanceID();

            m_Impl.SetSelection(new List<int>() { id },
                TreeViewSelectionOptions.FireSelectionChanged | TreeViewSelectionOptions.RevealAndFrame);
        }

        void InitializeIfNeeded()
        {
            if (m_Impl == null)
            {
                Initialize();
            }
        }

        void Initialize()
        {
            var multiColumnHeaderState = new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column()
                {
                    width = k_SmallColumnWidth,
                },
                new MultiColumnHeaderState.Column()
                {
                    width = k_SmallColumnWidth,
                },
                new MultiColumnHeaderState.Column()
                {
                    width = k_SmallColumnWidth,
                },
                new MultiColumnHeaderState.Column()
            })
            {
                // choose the columns that are added to the view by default
                visibleColumns = new[]
                {
                    Columns.DragHandle,
                    Columns.Status,
                    Columns.Active,
                    Columns.Devices
                }.Select(e => (int)e).ToArray(),
            };

            for (var i = 0; i < multiColumnHeaderState.columns.Length; i++)
            {
                var column = multiColumnHeaderState.columns[i];

                switch ((Columns)i)
                {
                    case Columns.DragHandle:
                    {
                        column.headerContent = GUIContent.none;
                        column.autoResize = false;
                        column.minWidth = k_SmallColumnWidth;
                        column.maxWidth = k_SmallColumnWidth;
                        break;
                    }
                    case Columns.Status:
                    {
                        column.headerContent = GUIContent.none;
                        column.autoResize = false;
                        column.minWidth = k_SmallColumnWidth;
                        column.maxWidth = k_SmallColumnWidth;
                        break;
                    }
                    case Columns.Active:
                    {
                        column.headerContent = GUIContent.none;
                        column.autoResize = false;
                        column.minWidth = k_SmallColumnWidth;
                        column.maxWidth = k_SmallColumnWidth;
                        break;
                    }
                    case Columns.Devices:
                    {
                        column.headerContent = new GUIContent("Capture Devices");
                        column.minWidth = 100f;
                        column.autoResize = true;
                        break;
                    }
                }
            }

            var header = new MultiColumnHeader(multiColumnHeaderState)
            {
                canSort = false,
                height = 0f,
            };

            header.ResizeToFit();

            m_Impl = new DeviceTreeViewImpl(m_TreeViewState, header);
        }
    }

    class DeviceTreeViewImpl : TreeView
    {
        class DeviceTreeViewItem : TreeViewItem
        {
            public LiveCaptureDevice Device { get; private set; }

            public DeviceTreeViewItem(LiveCaptureDevice device, int id, int depth, string displayName) : base(id, depth, displayName)
            {
                Device = device;
            }
        }

        const int k_RootId = 0;

        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";
            public static readonly GUIContent EnableToggleContent = EditorGUIUtility.TrTextContent("", "Toggle the enabled state of the device.");
            public static readonly GUIContent DeviceReadyMixed = EditorGUIUtility.TrIconContent($"{k_IconPath}/winbtn_mac_min.png");
            public static readonly GUIContent DeviceReadyIcon = EditorGUIUtility.TrIconContent($"{k_IconPath}/winbtn_mac_max.png");
            public static readonly GUIContent DeviceNotReadyIcon = EditorGUIUtility.TrIconContent($"{k_IconPath}/winbtn_mac_close.png");
            public static readonly GUIStyle draggingHandle = "RL DragHandle";
            public static readonly string UndoEnableDevice = L10n.Tr("Set Enabled");
        }

        public LiveCaptureDevice[] SelectedDevices { get; private set; }

        public DeviceTreeViewImpl(TreeViewState treeViewState, MultiColumnHeader header) : base(treeViewState, header)
        {
            useScrollView = true;
            columnIndexForTreeFoldouts = 3;
            rowHeight = EditorGUIUtility.singleLineHeight + 2f;

            this.DeselectOnUnhandledMouseDown(true);
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(k_RootId, -1, "Root");
            var items = new List<TreeViewItem>();
            var devices = LiveCaptureDeviceManager.Instance.Devices
                .OrderBy(d => d.SortingOrder);

            foreach (var device in devices)
            {
                items.Add(new DeviceTreeViewItem(device, device.GetInstanceID(), 0, device.gameObject.name));
            }

            SetupParentsAndChildrenFromDepths(root, items);

            var selection = GetSelection().ToList();
            // Remove any selection that is no longer valid
            selection.RemoveAll(id => FindItem(id, root) == null);
            SetSelection(selection, TreeViewSelectionOptions.FireSelectionChanged);

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (DeviceTreeView.Columns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect rect, TreeViewItem item, DeviceTreeView.Columns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref rect);

            switch (column)
            {
                case DeviceTreeView.Columns.DragHandle:
                    DraggingHandleGUI(rect);
                    break;
                case DeviceTreeView.Columns.Status:
                    StatusGUI(rect, item);
                    break;
                case DeviceTreeView.Columns.Active:
                    ActiveGUI(rect, item);
                    break;
                case DeviceTreeView.Columns.Devices:
                    DeviceGUI(rect, item);
                    break;
            }
        }

        void DraggingHandleGUI(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Contents.draggingHandle.Draw(new Rect(rect.x + 5, rect.y + 8, 10, 6), false, false, false, false);
            }
        }

        void StatusGUI(Rect rect, TreeViewItem item)
        {
            const float k_Width = 18f;

            var xOffset = (rect.width - k_Width) * 0.5f;
            var rect2 = new Rect(rect.x + xOffset, rect.y, k_Width, rect.height);

            switch (item)
            {
                case DeviceTreeViewItem deviceItem:
                    EditorGUI.LabelField(rect2, GetStatusIcon(deviceItem.Device.IsReady()));
                    break;
            }
        }

        void ActiveGUI(Rect rect, TreeViewItem item)
        {
            const float k_ElementWidth = 18f;

            var xOffset = (rect.width - k_ElementWidth) * 0.5f + 2f;
            var rect2 = new Rect(rect.x + xOffset, rect.y, rect.width - xOffset, rect.height);

            switch (item)
            {
                case DeviceTreeViewItem deviceItem:
                    var device = deviceItem.Device;
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var isLive = EditorGUI.Toggle(rect2, Contents.EnableToggleContent, device.IsLive);

                        if (change.changed)
                        {
                            Undo.RegisterCompleteObjectUndo(device, Contents.UndoEnableDevice);

                            device.IsLive = isLive;

                            EditorUtility.SetDirty(device);
                            EditorApplication.QueuePlayerLoopUpdate();
                        }
                    }
                    break;
            }
        }

        void DeviceGUI(Rect rect, TreeViewItem item)
        {
            const float verticalScrollBarWidth = 10f;

            var deviceItem = item as DeviceTreeViewItem;
            var indent = GetContentIndent(item);

            rect = new Rect(rect.x + indent, rect.y, rect.width - indent, rect.height);

            if (!showingVerticalScrollBar)
            {
                rect.width += verticalScrollBarWidth;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.ObjectField(rect, GUIContent.none, deviceItem.Device, typeof(LiveCaptureDevice), true);
            }
        }

        GUIContent GetStatusIcon(bool isReady)
        {
            if (isReady)
            {
                return Contents.DeviceReadyIcon;
            }
            else
            {
                return Contents.DeviceNotReadyIcon;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            SelectedDevices = IdsToDeviceArray(selectedIds);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = IdsToDeviceArray(args.draggedItemIDs);
            DragAndDrop.StartDrag("Reorder");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (args.performDrop)
            {
                var insertAtIndex = args.insertAtIndex;
                var objectReferences = DragAndDrop.objectReferences;
                var objectReferenceCount = objectReferences.Length;
                var items = GetRows();

                for (var index = 0; index < items.Count; ++index)
                {
                    var item = items[index];
                    var deviceItem = item as DeviceTreeViewItem;
                    var device = deviceItem.Device;

                    Undo.RegisterCompleteObjectUndo(device, Undo.GetCurrentGroupName());

                    if (index < insertAtIndex)
                    {
                        device.SortingOrder = index;
                    }
                    else
                    {
                        device.SortingOrder = index + objectReferenceCount;
                    }
                }

                for (var index = 0; index < objectReferenceCount; ++index)
                {
                    var device = objectReferences[index] as LiveCaptureDevice;

                    device.SortingOrder = insertAtIndex + index;
                }

                SetSelection(objectReferences
                    .OfType<LiveCaptureDevice>()
                    .Select(d => d.GetInstanceID())
                    .ToList());

                Reload();
            }

            return DragAndDropVisualMode.Move;
        }

        static LiveCaptureDevice[] IdsToDeviceArray(IList<int> selectedIds)
        {
            return selectedIds
                .Select(id => EditorUtility.InstanceIDToObject(id))
                .OfType<LiveCaptureDevice>()
                .ToArray();
        }
    }
}
