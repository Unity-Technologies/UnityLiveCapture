using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    class TakeTreeView
    {
        const float k_ShortColumnWidth = 15f;
        const float k_SmallColumnWidth = 45f;
        const float k_MidColumnWidth = 150f;

        public enum Columns
        {
            Asset,
            Scene,
            Shot,
            Take,
            Rating,
        }

        [SerializeField]
        TreeViewState m_TreeViewState = new TreeViewState();
        [SerializeField]
        MultiColumnHeaderState m_MultiColumnHeaderState;

        TakeTreeViewImpl m_Impl;

        public Take[] Selection
        {
            get
            {
                InitializeIfNeeded();

                return m_Impl.Selection;
            }
        }

        public void SetTakes(Take[] value)
        {
            InitializeIfNeeded();

            Debug.Assert(m_Impl != null);

            m_Impl.SetTakes(value);
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

        void InitializeIfNeeded()
        {
            if (m_Impl == null)
            {
                Initialize();
            }
        }

        void Initialize()
        {
            if (m_MultiColumnHeaderState == null)
            {
                m_MultiColumnHeaderState = new MultiColumnHeaderState(new[]
                {
                    new MultiColumnHeaderState.Column()
                    {
                        width = 200f,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        width = k_SmallColumnWidth,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        width = k_MidColumnWidth,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        width = k_SmallColumnWidth,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        width = k_SmallColumnWidth,
                    }
                })
                {
                    // choose the columns that are added to the view by default
                    visibleColumns = new[]
                    {
                        Columns.Asset,
                        Columns.Scene,
                        Columns.Shot,
                        Columns.Take,
                        Columns.Rating
                    }.Select(e => (int)e).ToArray(),
                };
            }


            for (var i = 0; i < m_MultiColumnHeaderState.columns.Length; i++)
            {
                var column = m_MultiColumnHeaderState.columns[i];

                switch ((Columns)i)
                {
                    case Columns.Asset:
                    {
                        column.headerContent = new GUIContent("Asset");
                        column.canSort = true;
                        column.autoResize = false;
                        column.minWidth = k_MidColumnWidth;
                        column.maxWidth = float.MaxValue;
                        break;
                    }
                    case Columns.Scene:
                    {
                        column.headerContent = new GUIContent("Scene");
                        column.canSort = true;
                        column.autoResize = true;
                        column.minWidth = k_ShortColumnWidth;
                        column.maxWidth = k_SmallColumnWidth;
                        break;
                    }
                    case Columns.Shot:
                    {
                        column.headerContent = new GUIContent("Shot");
                        column.canSort = true;
                        column.autoResize = true;
                        column.minWidth = k_SmallColumnWidth;
                        column.maxWidth = k_MidColumnWidth;
                        break;
                    }
                    case Columns.Take:
                    {
                        column.headerContent = new GUIContent("Take");
                        column.canSort = true;
                        column.autoResize = true;
                        column.minWidth = k_ShortColumnWidth;
                        column.maxWidth = k_SmallColumnWidth;
                        break;
                    }
                    case Columns.Rating:
                    {
                        column.headerContent = new GUIContent("Rating");
                        column.canSort = true;
                        column.autoResize = false;
                        column.minWidth = k_SmallColumnWidth;
                        column.maxWidth = RatingPropertyDrawer.kIconWidth * 5f + 10f;
                        break;
                    }
                }
            }

            var header = new MultiColumnHeader(m_MultiColumnHeaderState)
            {
                canSort = false,
                height = 24f,
            };

            m_Impl = new TakeTreeViewImpl(m_TreeViewState, header);
        }
    }

    class TakeTreeViewImpl : TreeView
    {
        class ItemComparer : IComparer<TreeViewItem>
        {
            public TakeTreeView.Columns Sorting { get; set; } = TakeTreeView.Columns.Asset;
            public bool SortedAscending { get; set; }

            int Multiplyer => SortedAscending ? 1 : -1;

            public int Compare(TreeViewItem item1, TreeViewItem item2)
            {
                var takeItem1 = item1 as TakeItem;
                var takeItem2 = item2 as TakeItem;

                if (takeItem1 == null || takeItem2 == null)
                {
                    return 0;
                }

                var x = takeItem1.Value;
                var y = takeItem2.Value;

                switch (Sorting)
                {
                    case TakeTreeView.Columns.Asset:
                        return x.name.CompareTo(y.name) * Multiplyer;
                    case TakeTreeView.Columns.Scene:
                        return x.SceneNumber.CompareTo(y.SceneNumber) * Multiplyer;
                    case TakeTreeView.Columns.Shot:
                        return x.ShotName.CompareTo(y.ShotName) * Multiplyer;
                    case TakeTreeView.Columns.Take:
                        return x.TakeNumber.CompareTo(y.TakeNumber) * Multiplyer;
                    case TakeTreeView.Columns.Rating:
                        return x.Rating.CompareTo(y.Rating) * Multiplyer;
                }

                return 0;
            }
        }

        class AssetItem<T> : TreeViewItem where T : UnityEngine.Object
        {
            public T Value { get; private set; }

            public AssetItem(T value, int depth, string displayName) : base(value.GetInstanceID(), depth, displayName)
            {
                Value = value;
            }
        }

        class TakeItem : AssetItem<Take>
        {
            public TakeItem(Take value, int depth, string displayName) : base(value, depth, displayName) { }
        }

        const int k_RootId = 0;

        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";

            public static readonly GUIContent TakeIcon = EditorGUIUtility.TrIconContent($"{k_IconPath}/d_TakeAsset@64.png");
            public static readonly GUIContent FolderIcon = EditorGUIUtility.TrIconContent("Folder Icon");
            public static readonly GUIContent FolderEmptyIcon = EditorGUIUtility.TrIconContent("FolderEmpty Icon");
        }

        MultiColumnHeader m_Header;
        List<Take> m_Takes = new List<Take>();
        ItemComparer m_Comparer = new ItemComparer();

        public Take[] Selection { get; private set; }

        public TakeTreeViewImpl(TreeViewState treeViewState, MultiColumnHeader header) : base(treeViewState, header)
        {
            useScrollView = true;

            m_Header = header;

            header.canSort = true;
            header.sortingChanged += OnSortingChanged;

            var index = header.sortedColumnIndex;

            if (index >= 0)
            {
                m_Comparer.Sorting = (TakeTreeView.Columns)index;
                m_Comparer.SortedAscending = header.GetColumn(index).sortedAscending;
            }

            Reload();
        }

        public void SetTakes(Take[] takes)
        {
            if (m_Takes.Count == 0
                && (takes == null || takes.Length == 0))
            {
                return;
            }

            var update = takes == null
                || !Enumerable.SequenceEqual(m_Takes, takes);

            if (update)
            {
                m_Takes.Clear();

                if (takes != null)
                {
                    m_Takes.AddRange(takes);
                }

                Reload();
                Sort(GetRows());

                if (Selection != null)
                {
                    Selection = m_Takes
                        .Intersect(Selection)
                        .ToArray();
                }
                else
                {
                    Selection = Array.Empty<Take>();
                }

                SetSelection(Selection
                    .Select(t => t.GetInstanceID())
                    .ToList());
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(k_RootId, -1, "Root");

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var icon = Contents.TakeIcon.image as Texture2D;
            var items = new List<TreeViewItem>();

            if (m_Takes.Count == 0)
            {
                return items;
            }

            foreach (var take in m_Takes)
            {
                var item = new TakeItem(take, 0, take.name)
                {
                    icon = icon
                };

                items.Add(item);
            }

            SetupParentsAndChildrenFromDepths(root, items);
            Sort(items);

            return items;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (TakeTreeView.Columns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect rect, TreeViewItem item, TakeTreeView.Columns column, ref RowGUIArgs args)
        {
            var take = (item as TakeItem).Value;

            switch (column)
            {
                case TakeTreeView.Columns.Asset:
                    args.rowRect = rect;
                    base.RowGUI(args);
                    break;
                case TakeTreeView.Columns.Scene:
                    EditorGUI.LabelField(rect, take.SceneNumber.ToString());
                    break;
                case TakeTreeView.Columns.Shot:
                    EditorGUI.LabelField(rect, take.ShotName);
                    break;
                case TakeTreeView.Columns.Take:
                    EditorGUI.LabelField(rect, take.TakeNumber.ToString());
                    break;
                case TakeTreeView.Columns.Rating:
                    DrawRating(rect, take.Rating);
                    break;
            }
        }

        void DrawRating(Rect rect, int value)
        {
            if (rect.width < value * RatingPropertyDrawer.kIconWidth)
            {
                RatingPropertyDrawer.DrawCompactField(rect, value);
            }
            else
            {
                RatingPropertyDrawer.DrawField(rect, value, false, false);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            EditorGUIUtility.PingObject(id);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            Selection = null;

            if (selectedIds.Count > 0)
            {
                Selection = FindRows(SortItemIDsInRowOrder(selectedIds))
                    .OfType<TakeItem>()
                    .Select(i => i.Value)
                    .ToArray();
            }
        }

        void OnSortingChanged(MultiColumnHeader header)
        {
            Sort(GetRows());
        }

        void Sort(IList<TreeViewItem> rows)
        {
            var index = m_Header.sortedColumnIndex;

            if (index >= 0)
            {
                m_Comparer.Sorting = (TakeTreeView.Columns)index;
                m_Comparer.SortedAscending = m_Header.GetColumn(index).sortedAscending;

                var sortedItems = rows
                    .OrderBy(i => i, m_Comparer)
                    .ToList();

                rows.Clear();

                foreach (var item in sortedItems)
                {
                    rows.Add(item);
                }
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();

            var takes = FindRows(args.draggedItemIDs)
                .OfType<TakeItem>()
                .Select(item => item.Value)
                .ToArray();

            DragAndDrop.objectReferences = takes;

            DragAndDrop.StartDrag("Takes");
        }
    }
}
