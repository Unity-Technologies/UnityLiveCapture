using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    class DirectoryTreeView
    {
        [SerializeField]
        TreeViewState m_TreeViewState = new TreeViewState();

        DirectoryTreeViewImpl m_Impl;

        public Take[] SelectedTakes => m_Impl.SelectedTakes;

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
            m_Impl.ReloadSelection();
        }

        void InitializeIfNeeded()
        {
            if (m_Impl == null)
            {
                m_Impl = new DirectoryTreeViewImpl(m_TreeViewState);
            }
        }
    }

    class DirectoryTreeViewImpl : TreeView
    {

        const int k_RootId = 0;

        static class Contents
        {
            public static readonly GUIContent FolderEmptyIcon = EditorGUIUtility.TrIconContent("FolderEmpty Icon");
        }

        HashSet<int> m_AncestorIds;
        HashSet<int> m_TakeIds;
        HashSet<string> m_DirectoriesWithAssets;
        HashSet<string> m_DirectoryLeafs;

        public Take[] SelectedTakes { get; private set; }

        public DirectoryTreeViewImpl(TreeViewState treeViewState) : base(treeViewState)
        {
            useScrollView = true;
            this.DeselectOnUnhandledMouseDown(true);
            Reload();
            ReloadSelection();
        }

        protected override TreeViewItem BuildRoot()
        {
            var paths = AssetDatabase.FindAssets($"t:{typeof(Take).Name}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            var instanceIds = paths
                .Select(p => AssetDatabase.LoadMainAssetAtPath(p).GetInstanceID())
                .ToArray();

            m_DirectoriesWithAssets = new HashSet<string>(
                paths.Select(p => Path.GetDirectoryName(p)));
            m_DirectoryLeafs = new HashSet<string>(EnumerateLeafs(m_DirectoriesWithAssets));
            m_AncestorIds = new HashSet<int>(
                new HierarchyProperty(HierarchyType.Assets).FindAllAncestors(instanceIds));
            m_TakeIds = new HashSet<int>(instanceIds);

            var root = new TreeViewItem(k_RootId, -1, "Root");

            return root;
        }

        IEnumerable<string> EnumerateLeafs(IEnumerable<string> directories)
        {
            var leaf = string.Empty;

            foreach (var dir in directories.OrderByDescending(d => d))
            {
                if (!leaf.StartsWith(dir))
                {
                    leaf = dir;

                    yield return dir;
                }
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var expandIDs = state.expandedIDs.ToArray();
            var items = new List<TreeViewItem>(m_AncestorIds.Count);
            var property = new HierarchyProperty(HierarchyType.Assets);

            while (property.Next(expandIDs))
            {
                if (property.isFolder)
                {
                    if (!m_AncestorIds.Contains(property.instanceID))
                    {
                        continue;
                    }

                    var item = new TreeViewItem(property.instanceID, property.depth, property.name);

                    items.Add(item);

                    var path = AssetDatabase.GetAssetPath(property.instanceID);

                    if (m_DirectoriesWithAssets.Contains(path))
                    {
                        item.icon = property.icon;
                    }
                    else
                    {
                        item.icon = Contents.FolderEmptyIcon.image as Texture2D;
                    }

                    if (property.hasChildren && !m_DirectoryLeafs.Contains(path))
                    {
                        // add a dummy child in children list to ensure we show the collapse arrow (because we do not fetch data for collapsed items)
                        item.AddChild(null);
                    }
                }
            }

            SetupParentsAndChildrenFromDepths(root, items);

            return items;
        }

        public void ReloadSelection()
        {
            SelectionChanged(GetSelection());
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            SelectedTakes = null;

            if (selectedIds.Count > 0)
            {
                var items = FindRows(SortItemIDsInRowOrder(selectedIds));

                SelectedTakes = items
                    .Select(i => AssetDatabase.GetAssetPath(i.id))
                    .SelectMany(p => AssetDatabaseUtility.GetAssetsAtPath<Take>(p, false))
                    .ToArray();
            }
        }
    }
}
