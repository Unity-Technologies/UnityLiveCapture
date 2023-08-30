using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Timeline;

namespace Unity.LiveCapture.Editor
{
    [ContextEditor(typeof(DirectorContext))]
    [Serializable]
    class TimelineContextEditor : TakeRecorderContextEditor
    {
        static class Contents
        {
            public static readonly string CreateShotMessage = L10n.Tr("Create a shot to start recording in the selected Timeline");
            public static readonly GUIContent CreateShotLabel = EditorGUIUtility.TrTextContent("Create Shot", "Create a shot in the selected Timeline.");
            public static readonly GUILayoutOption[] LargeButtonOptions = { GUILayout.Height(30f) };
        }

        [SerializeField]
        TreeViewState m_TreeViewState = new TreeViewState();
        TimelineTreeView m_TreeView;

        public override void OnShotGUI(Rect rect)
        {
            rect = EditorGUILayout.GetControlRect(false, rect.height);

            InitializeIfNeeded();

            m_TreeView.Update();
            m_TreeView.OnGUI(rect);
        }

        public override void OnInspectorGUI()
        {
            InitializeIfNeeded();

            if (RequiresTimelineTrack())
            {
                DoCreateShotButton();
            }
            else
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    DrawDefaultInspector();

                    if (change.changed)
                    {
                        TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
                    }
                }
            }
        }

        void InitializeIfNeeded()
        {
            if (m_TreeView == null)
            {
                m_TreeView = new TimelineTreeView(m_TreeViewState);
                m_TreeView.Reload();
            }
        }

        bool RequiresTimelineTrack()
        {
            if (TryGetTimelineFromSequence(m_TreeView.SelectedSequenceContext, out var clip, out var director, out var timeline))
            {
                return !timeline.GetOutputTracks()
                    .Any(t => !t.muted && t.hasClips && t is TakeRecorderTrack);
            }

            return false;
        }

        bool TryGetTimelineFromSequence(SequenceContext? sequenceContext, out TimelineClip clip, out PlayableDirector director, out TimelineAsset timeline)
        {
            clip = null;
            director = null;
            timeline = null;

            if (!sequenceContext.HasValue)
            {
                return false;
            }

            var sequence = sequenceContext.Value;

            clip = sequence.clip;
            director = sequence.director;

            if (director == null)
            {
                return false;
            }

            timeline = director.playableAsset as TimelineAsset;

            return timeline != null;
        }

        void DoCreateShotButton()
        {
            EditorGUILayout.HelpBox(Contents.CreateShotMessage, MessageType.Info, true);

            if (GUILayout.Button(Contents.CreateShotLabel, Contents.LargeButtonOptions))
            {
                if (TryGetTimelineFromSequence(m_TreeView.SelectedSequenceContext, out var parentClip, out var director, out var timeline))
                {
                    var track = timeline.GetOutputTracks()
                        .FirstOrDefault(t => !t.muted && t is TakeRecorderTrack);

                    if (track == null)
                    {
                        track = timeline.CreateTrack<TakeRecorderTrack>();
                    }

                    var clip = track.CreateClip<ShotPlayableAsset>();

                    clip.displayName = "New Shot";

                    if (parentClip != null)
                    {
                        clip.duration = parentClip.duration;
                    }
                    else if (timeline.duration > 0d)
                    {
                        clip.duration = timeline.duration;
                    }

                    TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

                    var context = Context as DirectorContext;

                    context.Reload();
                    context.Selection = context.IndexOf(null, clip.asset as PlayableAsset);
                }
            }
        }
    }
}
