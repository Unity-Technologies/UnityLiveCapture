using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Editor
{
    [CustomEditor(typeof(SynchronizerComponent))]
    class SynchronizerEditor : UnityEditor.Editor
    {
        internal static class Contents
        {
            public static readonly string AddDataSource = "Add Data Source";
            public static readonly string RemoveDataSource = "Remove Data Source";
            public static readonly GUIContent TimecodeSourceLabel = EditorGUIUtility.TrTextContent(
                "Timecode Source",
                "The timecode source that corresponds to the primary clock, giving the global time.");
            public static readonly GUIContent AddIcon = EditorGUIUtility.IconContent("Toolbar Plus More@2x");
            public static readonly GUIContent DelayLabel = EditorGUIUtility.TrTextContent(
                "Delay",
                "The delay, in frames, applied to the source timecode. " +
                "Use a positive value to compensate for high-latency sources.");
            public static readonly GUIContent PresentTimecodeLabel = EditorGUIUtility.TrTextContent("Current Timecode");
            public static readonly GUIContent PresentRateLabel = EditorGUIUtility.TrTextContent("Frame Rate");
            public static readonly GUIContent DataSourcesLabel = EditorGUIUtility.TrTextContent("Timed data sources");
            public static readonly GUIContent StatusLabel = EditorGUIUtility.TrTextContent("Status");
            public static readonly GUIContent BufferSizeLabel = EditorGUIUtility.TrTextContent("Buffer", "Buffer size in frames.");
            public static readonly GUIContent LocalOffsetLabel = EditorGUIUtility.TrTextContent("Offset", "Local time offset in frames.");
            public static readonly GUIContent FrameRateLabel = EditorGUIUtility.TrTextContent("Rate", "The frame rate of the source.");
            public static readonly GUIContent CalibrateLabel = EditorGUIUtility.TrTextContent("Calibrate");
            public static readonly GUIContent StopCalibrateLabel = EditorGUIUtility.TrTextContent("Stop Calibration");
            public static readonly GUIContent OpenSyncWindowLabel = EditorGUIUtility.TrTextContent(
                "Open Synchronization",
                "Open a dedicated window to control and monitor timecode synchronization.");
            public static readonly GUIContent OpenTimedDataSourceDetailsLabel = EditorGUIUtility.TrTextContent(
                "Open Timed Data Source Details",
                "Open a dedicated window to control and monitor timecode synchronization of all data sources in detail.");
            public static readonly Vector2 OptionDropdownSize = new Vector2(300f, 250f);
        }

        static IEnumerable<(Type, CreateTimecodeSourceMenuItemAttribute[])> s_CreateTimecodeSourceMenuItems;

        SerializedProperty m_TimecodeSourceProperty;
        SerializedProperty m_DelayProperty;
        SerializedProperty m_DataSourcesProperty;

        SynchronizerComponent m_Synchronizer;
        ReorderableList m_DataSourceList;

        void OnEnable()
        {
            m_Synchronizer = target as SynchronizerComponent;

            m_TimecodeSourceProperty = serializedObject.FindProperty("m_Impl.m_TimecodeSourceRef");
            m_DelayProperty = serializedObject.FindProperty("m_Impl.m_Delay");
            m_DataSourcesProperty = serializedObject.FindProperty("m_Impl.m_SourcesAndStatuses");

            CreateDataSourcesList();
        }

        void CreateDataSourcesList()
        {
            if (m_DataSourceList != null) return;

            m_DataSourceList = new ReorderableList(
                serializedObject,
                m_DataSourcesProperty,
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                drawHeaderCallback = rect =>
                {
                    rect.xMin += SourceAndStatusBundlePropertyDrawer.Contents.HandleSize;

                    var right = rect;
                    right.xMin = right.xMax - SourceAndStatusBundlePropertyDrawer.Contents.RightSectionWidth;

                    var name = rect;
                    name.xMax = right.xMin;

                    var status = right;
                    status.width = SourceAndStatusBundlePropertyDrawer.Contents.StatusColumnWidth;

                    var rate = right;
                    rate.xMin = status.xMax;
                    rate.width = SourceAndStatusBundlePropertyDrawer.Contents.FrameRateWidth;

                    var buffer = right;
                    buffer.xMin = rate.xMax;
                    buffer.width = SourceAndStatusBundlePropertyDrawer.Contents.IntColumnWidth;

                    var offset = right;
                    offset.xMin = buffer.xMax;
                    offset.width = SourceAndStatusBundlePropertyDrawer.Contents.IntColumnWidth;

                    EditorGUI.LabelField(name, Contents.DataSourcesLabel);
                    EditorGUI.LabelField(status, Contents.StatusLabel);
                    EditorGUI.LabelField(rate, Contents.FrameRateLabel);
                    EditorGUI.LabelField(buffer, Contents.BufferSizeLabel);
                    EditorGUI.LabelField(offset, Contents.LocalOffsetLabel);
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = m_DataSourcesProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                },
                onCanAddCallback = list => GetUnusedSources().Any(),
                onAddDropdownCallback = (rect, list) =>
                {
                    ShowAddDataSourceMenu();
                },
                onRemoveCallback = list =>
                {
                    Undo.RegisterCompleteObjectUndo(m_Synchronizer, Contents.RemoveDataSource);
                    m_Synchronizer.Impl.RemoveDataSource(list.index);
                    EditorUtility.SetDirty(m_Synchronizer);
                }
            };
        }

        void ShowAddDataSourceMenu()
        {
            var sources = GetUnusedSources()
                .Where(s => s != null)
                .OrderBy(s => s.FriendlyName)
                .ToArray();

            var names = sources
                .Select(s => new GUIContent(s.FriendlyName))
                .ToArray();

            var pos = new Rect(Event.current.mousePosition, Vector2.zero);

            OptionSelectWindow.SelectOption(pos, Contents.OptionDropdownSize, names, (index, value) =>
            {
                var source = sources[index];

                if (source is Object sourceObject)
                {
                    Undo.RecordObject(sourceObject, Contents.AddDataSource);
                }

                // Find out if the data source is already part of another synchronization group
                if (source.Synchronizer is { } oldSynchronizer)
                {
                    // Track down the SynchronizerComponent that owns the Synchronizer instance.
                    // Undo tracking requires a UnityEngine.Object target.
                    foreach (var oldSyncObject in Resources.FindObjectsOfTypeAll<SynchronizerComponent>())
                    {
                        if (EditorUtility.IsPersistent(oldSyncObject) || oldSyncObject.hideFlags == HideFlags.NotEditable || oldSyncObject.hideFlags == HideFlags.HideAndDontSave)
                            continue;

                        if (oldSyncObject.Impl == oldSynchronizer)
                        {
                            Undo.RecordObject(oldSyncObject, Contents.AddDataSource);
                            break;
                        }
                    }

                    // Remove source from the old synchronizer
                    oldSynchronizer.RemoveDataSource(source);
                }

                // Add the source to this synchronizer
                Undo.RecordObject(m_Synchronizer, Contents.AddDataSource);
                m_Synchronizer.Impl.AddDataSource(source);
                EditorUtility.SetDirty(m_Synchronizer);
            });
        }

        IEnumerable<ITimedDataSource> GetUnusedSources()
        {
            return TimedDataSourceManager.Instance.Where(i => !m_Synchronizer.Impl.ContainsDataSource(i));
        }

        public override void OnInspectorGUI()
        {
            DoTimecodeSourceGUI();
            DoDelayGUI();

            EditorGUILayout.Space();

            DoSourcesGUI();
            DoCalibrationGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button(Contents.OpenSyncWindowLabel))
            {
                SynchronizationWindow.ShowWindow();
            }
            if (GUILayout.Button(Contents.OpenTimedDataSourceDetailsLabel))
            {
                TimedDataSourceViewerWindow.ShowWindow();
            }
        }

        public void DoTimecodeSourceGUI()
        {
            serializedObject.Update();

            var timecodeSourceRect = EditorGUILayout.GetControlRect();
            var addSourceRect = new Rect(timecodeSourceRect)
            {
                xMin = timecodeSourceRect.xMax - 20f,
            };
            timecodeSourceRect.xMax = addSourceRect.xMin;

            EditorGUI.PropertyField(timecodeSourceRect, m_TimecodeSourceProperty, Contents.TimecodeSourceLabel, true);

            if (GUI.Button(addSourceRect, GUIContent.none))
            {
                if (s_CreateTimecodeSourceMenuItems == null)
                {
                    s_CreateTimecodeSourceMenuItems = AttributeUtility.GetAllTypes<CreateTimecodeSourceMenuItemAttribute>()
                        .Where(t => typeof(Component).IsAssignableFrom(t.type));
                }

                var menu = MenuUtility.CreateMenu(s_CreateTimecodeSourceMenuItems, t => true, (type, attribute) =>
                {
                    var timecodeSource = Undo.AddComponent(m_Synchronizer.gameObject, type);
                    m_Synchronizer.Synchronizer.TimecodeSource = timecodeSource as ITimecodeSource;
                });

                menu.ShowAsContext();
            }

            GUI.Label(addSourceRect, Contents.AddIcon);

            serializedObject.ApplyModifiedProperties();
        }

        public void DoDelayGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_DelayProperty, Contents.DelayLabel);

            serializedObject.ApplyModifiedProperties();
        }

        public void DoPresentTimecodeGUI()
        {
            var presentTime = m_Synchronizer.Synchronizer.PresentTime;

            if (presentTime != null)
            {
                var timecodeRect = EditorGUILayout.GetControlRect();
                timecodeRect = EditorGUI.PrefixLabel(timecodeRect, Contents.PresentTimecodeLabel);
                EditorGUI.LabelField(timecodeRect, presentTime.Value.ToTimecode().ToString(), EditorStyles.boldLabel);

                var frameRateRect = EditorGUILayout.GetControlRect();
                frameRateRect = EditorGUI.PrefixLabel(frameRateRect, Contents.PresentRateLabel);
                EditorGUI.LabelField(frameRateRect, presentTime.Value.Rate.ToString());
            }
        }

        public void DoSourcesGUI()
        {
            serializedObject.Update();

            var listRect = EditorGUILayout.GetControlRect(false, m_DataSourceList.GetHeight());
            m_DataSourceList.DoList(listRect);

            serializedObject.ApplyModifiedProperties();
        }

        public void DoCalibrationGUI()
        {
            var calibrationStatus = m_Synchronizer.Impl.CalibrationStatus;

            if (calibrationStatus != CalibrationStatus.InProgress)
            {
                if (GUILayout.Button(Contents.CalibrateLabel))
                {
                    m_Synchronizer.StartCalibration();
                }
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(calibrationStatus.ToString());

                    if (GUILayout.Button(Contents.StopCalibrateLabel))
                    {
                        m_Synchronizer.StopCalibration();
                    }
                }
            }
        }
    }
}
