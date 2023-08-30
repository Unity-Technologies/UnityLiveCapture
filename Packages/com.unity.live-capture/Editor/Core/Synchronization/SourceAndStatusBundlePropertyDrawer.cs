using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    [CustomPropertyDrawer(typeof(Synchronizer.SourceAndStatusBundle))]
    class SourceAndStatusBundlePropertyDrawer : PropertyDrawer
    {
        internal static class Contents
        {
            public static readonly string FieldUndo = "Inspector";
            public static readonly GUIContent EnableSyncToggleTooltip = EditorGUIUtility.TrTextContent("", "Turn on to enable device synchronization");
            public static readonly GUIContent BufferSizeTooltip = EditorGUIUtility.TrTextContent("",
                "The number of data frames to buffer for this data source. " +
                "Adjust the value to get a consistent overlap between the synchronized data sources.");
            public static readonly GUIContent OffsetTooltip = EditorGUIUtility.TrTextContent("",
                "The time offset applied to sample timecodes, in frames. " +
                "This value should typically match the time delay between timecode generation and data sampling for a single frame.");
            public static readonly GUIContent SourceUnavailableText = EditorGUIUtility.TrTextContent("Source unavailable", "The data source is disabled or deleted.");

            public const float HandleSize = 18f;
            public const float ToggleSize = 16f;
            public const float ToggleColumnWidth = ToggleSize + 3f;
            public const float SyncMonitorSize = 8f;
            public const float StatusTextWidth = 56f;
            public const float FrameRateWidth = 36f;
            public const float FieldPadding = 2f;
            public const float IntFieldWidth = 36f;
            public const float IntColumnWidth = IntFieldWidth + 4;
            public const float MonitorColumnWidth = SyncMonitorSize + 4;
            public const float StatusColumnWidth = MonitorColumnWidth + StatusTextWidth;
            public const float RightSectionWidth = StatusColumnWidth + FrameRateWidth + (2 * IntColumnWidth);

            public static readonly Dictionary<TimedSampleStatus, GUIContent> StatusNames = new Dictionary<TimedSampleStatus, GUIContent>
            {
                { TimedSampleStatus.Ok, EditorGUIUtility.TrTextContent("synced", "Source data is synchronized") },
                { TimedSampleStatus.DataMissing, EditorGUIUtility.TrTextContent("no data", "Source data is missing") },
                { TimedSampleStatus.Ahead, EditorGUIUtility.TrTextContent("ahead", "Source data is ahead of global time") },
                { TimedSampleStatus.Behind, EditorGUIUtility.TrTextContent("behind", "Source data is behind global time") }
            };
            public static readonly Dictionary<TimedSampleStatus, Color32> StatusColors = new Dictionary<TimedSampleStatus, Color32>
            {
                { TimedSampleStatus.Ok, new Color32(0x53, 0xc2, 0x2b, 0xff) },
                { TimedSampleStatus.DataMissing,  new Color32(0xb2, 0xb2, 0xb2, 0xff) },
                { TimedSampleStatus.Ahead,  new Color32(0xff, 0xc1, 0x07, 0xff) },
                { TimedSampleStatus.Behind,  new Color32(0xff, 0xc1, 0x07, 0xff) }
            };
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sourceAndStatus = property.GetValue<Synchronizer.SourceAndStatusBundle>();
            var source = sourceAndStatus.Source;
            var synchronizerComponent = property.serializedObject.targetObject as SynchronizerComponent;

            if (source == null)
            {
                EditorGUI.LabelField(position, Contents.SourceUnavailableText);
                return;
            }

            // Compute the layout
            var right = position;
            right.width = Contents.RightSectionWidth;
            right.x = position.width - right.width + position.x;

            var left = position;
            left.width -= right.width;

            var toggleRect = left;
            toggleRect.width = Contents.ToggleSize;

            var sourceNameRect = left;
            sourceNameRect.x += Contents.ToggleColumnWidth;
            sourceNameRect.width = left.width - Contents.ToggleColumnWidth;

            var statusRect = right;
            statusRect.width = Contents.StatusColumnWidth;

            var frameRateRect = right;
            frameRateRect.xMin = statusRect.xMax;
            frameRateRect.width = Contents.FrameRateWidth;

            var bufferFieldRect = right;
            bufferFieldRect.x = frameRateRect.xMax;
            bufferFieldRect.width = Contents.IntFieldWidth;
            bufferFieldRect.y += Contents.FieldPadding;
            bufferFieldRect.height -= Contents.FieldPadding * 2;

            var offsetFieldRect = bufferFieldRect;
            offsetFieldRect.x += Contents.IntColumnWidth;

            // Draw property fields
            DoStatusGUI(statusRect, sourceAndStatus);
            DoIsSynchronizedGUI(toggleRect, synchronizerComponent, sourceAndStatus);
            DoSourceNameGUI(sourceNameRect, source);
            DoFrameRateGUI(frameRateRect, source);
            DoBufferSizeGUI(bufferFieldRect, source);
            DoOffsetGUI(offsetFieldRect, source);
        }

        public static void DoStatusGUI(Rect rect, Synchronizer.SourceAndStatusBundle sourceAndStatus)
        {
            var statusImgRect = rect;
            statusImgRect.width = Contents.SyncMonitorSize;
            statusImgRect.height = Contents.SyncMonitorSize;
            statusImgRect.y += (rect.height - statusImgRect.height) / 2f;

            var statusRect = rect;
            statusRect.x += Contents.MonitorColumnWidth;
            statusRect.width = Contents.StatusTextWidth;

            EditorGUI.DrawRect(statusImgRect, Contents.StatusColors[sourceAndStatus.Status]);
            EditorGUI.LabelField(statusRect, Contents.StatusNames[sourceAndStatus.Status]);
        }

        public static void DoIsSynchronizedGUI(Rect rect, SynchronizerComponent synchronizer, Synchronizer.SourceAndStatusBundle sourceAndStatus)
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var isSynced = EditorGUI.Toggle(rect, sourceAndStatus.SynchronizationRequested);

                if (change.changed)
                {
                    if (synchronizer != null)
                    {
                        Undo.RegisterCompleteObjectUndo(synchronizer, Contents.FieldUndo);
                    }

                    sourceAndStatus.SynchronizationRequested = isSynced;

                    if (synchronizer != null)
                    {
                        EditorUtility.SetDirty(synchronizer);
                    }
                }
            }

            EditorGUI.LabelField(rect, Contents.EnableSyncToggleTooltip);
        }

        public static void DoSourceNameGUI(Rect rect, ITimedDataSource source)
        {
            EditorGUI.LabelField(rect, source.FriendlyName);
        }

        public static void DoFrameRateGUI(Rect rect, ITimedDataSource source)
        {
            EditorGUI.LabelField(rect, source.FrameRate.IsValid ? source.FrameRate.ToString() : "N/A");
        }

        public static void DoBufferSizeGUI(Rect rect, ITimedDataSource source)
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                int bufferSize;

                var min = Mathf.Max(source.MinBufferSize ?? 1, 1);
                var max = Mathf.Max(source.MaxBufferSize ?? int.MaxValue, min);

                if (source.MinBufferSize.HasValue && source.MaxBufferSize.HasValue && rect.width > 120f)
                {
                    bufferSize = EditorGUI.IntSlider(rect, source.BufferSize, min, max);
                }
                else
                {
                    bufferSize = EditorGUI.IntField(rect, source.BufferSize);
                }

                if (change.changed)
                {
                    var undoTarget = source.UndoTarget;

                    if (undoTarget != null)
                    {
                        Undo.RegisterCompleteObjectUndo(undoTarget, Contents.FieldUndo);
                    }

                    source.BufferSize = Mathf.Clamp(bufferSize, min, max);

                    if (undoTarget != null)
                    {
                        EditorUtility.SetDirty(undoTarget);
                    }
                }
            }

            EditorGUI.LabelField(rect, Contents.BufferSizeTooltip);
        }

        public static void DoOffsetGUI(Rect rect, ITimedDataSource source)
        {
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var newOffset = FrameNumberDrawer.DoField(rect, GUIContent.none, source.Offset);

                if (change.changed)
                {
                    var undoTarget = source.UndoTarget;

                    if (undoTarget != null)
                    {
                        Undo.RegisterCompleteObjectUndo(undoTarget, Contents.FieldUndo);
                    }

                    source.Offset = newOffset;

                    if (undoTarget != null)
                    {
                        EditorUtility.SetDirty(undoTarget);
                    }
                }
            }

            EditorGUI.LabelField(rect, Contents.OffsetTooltip);
        }
    }
}
