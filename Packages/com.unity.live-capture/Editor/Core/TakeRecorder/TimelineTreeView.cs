using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    class TimelineTreeView : TreeView
    {
        const int k_RootId = -1;

        static class Contents
        {
            static readonly string k_IconPath = "Packages/com.unity.live-capture/Editor/Core/Icons";

            public static readonly Texture2D TimelineAssetIcon;
            public static readonly Texture2D ShotAssetIcon = EditorGUIUtility.IconContent($"{k_IconPath}/d_TakeRecorder@64.png").image as Texture2D;

            static Contents()
            {
                TimelineAssetIcon = GetIconFromType(typeof(TimelineAsset));
            }

            static Texture2D GetIconFromType(Type type)
            {
                return EditorGUIUtility.FindTexture($"{type.FullName.Replace('.', '/')} Icon");
            }
        }

        class TimelineTreeViewItem : TreeViewItem
        {
            public SequenceContext Context { get; private set; }

            public TimelineTreeViewItem(SequenceContext ctx, int id, int depth, string displayName) : base(id, depth, displayName)
            {
                Context = ctx;
            }
        }

        class ContextTreeViewItem : TreeViewItem
        {
            public ContextTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
            {
            }
        }

        int m_Version;

        public SequenceContext? SelectedSequenceContext { get; private set; }

        public TimelineTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            useScrollView = true;
            this.DeselectOnUnhandledMouseDown(true);
        }

        public void RefreshSelection()
        {
            SelectionChanged(GetSelection());
        }

        public void Select(int id)
        {
            SetSelection(new List<int>() { id });
        }

        public int GetSelected()
        {
            var selection = GetSelection();

            return selection.Count > 0 ? selection[0] : -1;
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

        public void Update()
        {
            var context = MasterTimelineContext.Instance;

            if (m_Version != context.Version)
            {
                Reload();

                Select(context.Selection);
                ExpandTo(context.Selection);
            }

            if (context.HasSelection() && context.Selection != GetSelected())
            {
                Select(context.Selection);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var context = MasterTimelineContext.Instance;
            var root = new TreeViewItem(k_RootId, -1, "Root");
            var items = new List<TreeViewItem>();

            BuildItems(context.Director, items);

            m_Version = context.Version;

            SetupParentsAndChildrenFromDepths(root, items);

            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        static bool ValidateDirector(PlayableDirector director)
        {
            if (director == null)
                return false;
            else if (director.playableAsset is not TimelineAsset)
                return false;
            else if (director.TryGetComponent<ShotPlayer>(out var _))
                return false;
            else
                return true;
        }

        static void BuildItems(PlayableDirector director, List<TreeViewItem> items)
        {
            if (ValidateDirector(director))
            {
                var hierarchy = new Stack<TimelineContext>();

                BuildItems(director, null, hierarchy, 0, items);

                var shotId = 0;
                var timelineId = k_RootId - 1;

                foreach (var item in items)
                {
                    if (item is TimelineTreeViewItem timelineItem)
                    {
                        timelineItem.id = timelineId--;
                    }
                    else if (item is ContextTreeViewItem shotItem)
                    {
                        shotItem.id = shotId++;
                    }
                }
            }
        }

        static void BuildItems(
            PlayableDirector director,
            TimelineClip parentClip,
            Stack<TimelineContext> hierarchy,
            int depth,
            List<TreeViewItem> items)
        {
            if (!ValidateDirector(director))
            {
                return;
            }

            var timelineAsset = director.playableAsset as TimelineAsset;

            items.Add(new TimelineTreeViewItem(
                    new SequenceContext(director, parentClip), 0, depth, director.gameObject.name)
            {
                icon = Contents.TimelineAssetIcon
            });

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track.muted)
                {
                    continue;
                }

                foreach (var clip in track.GetClips())
                {
                    foreach (var subDirector in TimelineHierarchyContextUtility.GetSubTimelines(clip, director))
                    {
                        hierarchy.Push(new TimelineContext(director, clip));

                        BuildItems(subDirector, clip, hierarchy, depth + 1, items);

                        hierarchy.Pop();
                    }
                }
            }

            hierarchy.Push(new TimelineContext(director));

            var hierarchyContext = default(TimelineHierarchyContext);

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track.muted)
                {
                    continue;
                }

                if (track is TakeRecorderTrack takeRecorderTrack)
                {
                    if (hierarchyContext == null)
                    {
                        hierarchyContext = new TimelineHierarchyContext(hierarchy);
                    }

                    foreach (var clip in takeRecorderTrack.GetClips())
                    {
                        var asset = clip.asset as ShotPlayableAsset;

                        items.Add(new ContextTreeViewItem(0, depth + 1, asset.ShotName)
                        {
                            icon = Contents.ShotAssetIcon
                        });
                    }
                }
            }

            hierarchy.Pop();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var item = default(TreeViewItem);

            if (selectedIds.Count > 0)
            {
                item = this.FindItem(selectedIds[0], rootItem);
            }

            SelectedSequenceContext = null;

            var context = MasterTimelineContext.Instance;

            context.ClearSelection();

            switch (item)
            {
                case ContextTreeViewItem shotItem:
                {
                    context.Selection = shotItem.id;
                    TakeRecorder.SetPreviewTime(0d);

                    break;
                }
                case TimelineTreeViewItem timelineItem:
                {
                    SelectedSequenceContext = timelineItem.Context;

                    break;
                }
            }

            Timeline.Repaint();
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);

            if (item == null)
            {
                return;
            }

            var window = TimelineEditor.GetWindow();

            if (window == null)
            {
                return;
            }

            if (item is ContextTreeViewItem)
            {
                item = item.parent;
            }

            var timelineItem = item as TimelineTreeViewItem;

            Debug.Assert(timelineItem != null);

            var sequence = new Stack<SequenceContext>();

            while (timelineItem != null)
            {
                sequence.Push(timelineItem.Context);

                timelineItem = timelineItem.parent as TimelineTreeViewItem;
            }

            foreach (var ctx in sequence)
            {
                window.navigator.NavigateTo(ctx);
            }
        }
    }
}
