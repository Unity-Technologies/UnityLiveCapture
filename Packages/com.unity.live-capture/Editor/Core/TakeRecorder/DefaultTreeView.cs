using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    class DefaultTreeView : TreeView
    {
        const int k_RootId = -1;

        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";

            public static readonly Texture2D ShotAssetIcon = EditorGUIUtility.IconContent($"{k_IconPath}/d_TakeRecorder@64.png").image as Texture2D;
        }

        int m_Version;

        public ITakeRecorderContext Context { get; private set; }

        public DefaultTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            useScrollView = true;
            this.DeselectOnUnhandledMouseDown(true);
        }

        public int GetSelected()
        {
            var selection = GetSelection();

            return selection.Count > 0 ? selection[0] : -1;
        }

        public void Select(int id)
        {
            SetSelection(new List<int>() { id });
        }

        public void ExpandTo(int id)
        {
            var item = FindItem(id, rootItem);

            while (item != null && item.parent != null)
            {
                SetExpanded(item.parent.id, true);

                item = item.parent;
            }
        }

        public void Update(ITakeRecorderContext context)
        {
            var version = Context != null ? Context.Version : 0;
            var changed = Context != context || m_Version != version;

            if (changed)
            {
                Context = context;

                Reload();

                if (Context != null)
                {
                    ExpandTo(Context.Selection);
                }
            }

            if (Context != null)
            {
                var selection = Context.Selection;
                var current = GetSelected();

                if (current != selection)
                {
                    Select(selection);
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(k_RootId, -1, "Root");
            var items = new List<TreeViewItem>();

            m_Version = 0;

            if (Context != null)
            {
                var shots = Context.Shots;

                for (var i = 0; i < shots.Length; ++i)
                {
                    var shot = shots[i];

                    items.Add(new TreeViewItem(i, 0, shot.Slate.ShotName)
                    {
                        icon = Contents.ShotAssetIcon
                    });
                }

                m_Version = Context.Version;
            }

            SetupParentsAndChildrenFromDepths(root, items);

            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var item = default(TreeViewItem);

            if (selectedIds.Count > 0)
            {
                item = this.FindItem(selectedIds[0], rootItem);
            }

            if (Context != null)
            {
                Context.ClearSelection();

                if (item != null)
                {
                    Context.Selection = item.id;
                }
            }
        }
    }
}
