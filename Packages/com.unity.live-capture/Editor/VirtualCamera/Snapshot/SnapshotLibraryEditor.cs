using System;
using Unity.LiveCapture.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    [CustomEditor(typeof(SnapshotLibrary))]
    class SnapshotLibraryEditor : UnityEditor.Editor
    {
        static readonly float k_SnapshotElementHeight = 4f * (EditorGUIUtility.singleLineHeight + 2f) + 2f;
        static readonly float k_PreviewWidth = k_SnapshotElementHeight * 16f / 9f;

        static class Contents
        {
            public static string Deleted = L10n.Tr("(deleted)");
            public static string PreviewNotAvailable = L10n.Tr("Preview not available.");
            public static string UndoDeleteSnapshot = L10n.Tr("Delete Snapshot");

            static GUIStyle s_CenteredLabel;
            public static GUIStyle CenteredLabel
            {
                get
                {
                    if (s_CenteredLabel == null)
                    {
                        s_CenteredLabel = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            wordWrap = true
                        };
                    }
                    return s_CenteredLabel;
                }
            }
        }

        SnapshotLibrary m_SnapshotLibrary;
        SerializedProperty m_Snapshots;
        CompactList m_List;
        GUIContent m_NoneListItemContent;

        public int Index
        {
            get => m_List.Index;
        }

        public GUIContent NoneListItemContent
        {
            get => m_NoneListItemContent;
            set
            {
                m_NoneListItemContent = value;

                if (m_NoneListItemContent == null)
                    m_List.DrawNoneListItemCallback = null;
                else
                    m_List.DrawNoneListItemCallback = DrawNoneListItem;
            }
        }

        void OnEnable()
        {
            m_SnapshotLibrary = target as SnapshotLibrary;

            m_Snapshots = serializedObject.FindProperty("m_Snapshots");

            CreateList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            m_List.DoGUILayout();

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        void CreateList()
        {
            m_List = new CompactList(m_Snapshots);
            m_List.OnCanAddCallback = () => false;
            m_List.OnCanRemoveCallback = () => true;
            m_List.Reorderable = true;
            m_List.ItemHeight = k_SnapshotElementHeight;
            m_List.ShowSearchBar = false;
            m_List.DrawElementCallback = (r, i) => { };
            m_List.ElementHeightCallback = (i) => 0f;
            m_List.DrawListItemCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_Snapshots.GetArrayElementAtIndex(index);
                var screenshot = element.FindPropertyRelative("m_Screenshot").objectReferenceValue as Texture2D;
                var shotAsset = element.FindPropertyRelative("m_Slate").objectReferenceValue as ShotPlayableAsset;
                var frameRate = new FrameRate(
                    element.FindPropertyRelative("m_FrameRate.m_Numerator").intValue,
                    element.FindPropertyRelative("m_FrameRate.m_Denominator").intValue,
                    element.FindPropertyRelative("m_FrameRate.m_IsDropFrame").boolValue);
                var time = element.FindPropertyRelative("m_Time").doubleValue;
                var focalLength = element.FindPropertyRelative("m_Lens.m_FocalLength").floatValue;
                var aperture = element.FindPropertyRelative("m_Lens.m_Aperture").floatValue;
                var lensAsset = element.FindPropertyRelative("m_LensAsset").objectReferenceValue;
                var sensorSize = element.FindPropertyRelative("m_CameraBody.m_SensorSize").vector2Value;
                var previewRect = new Rect(rect.x, rect.y + 2.5f, k_PreviewWidth, rect.height - 5f);
                var propertiesRect = new Rect(previewRect.xMax + 5f, rect.y + 2f, rect.width - previewRect.width, rect.height);

                if (screenshot != null)
                {
                    var preview = AssetPreview.GetAssetPreview(screenshot);

                    if (preview != null)
                    {
                        var textureRect = BestFit(previewRect, preview);

                        EditorGUI.DrawPreviewTexture(textureRect, preview);
                    }
                }
                else
                {
                    EditorGUI.LabelField(previewRect, Contents.PreviewNotAvailable, Contents.CenteredLabel);
                }

                var rowHeight = EditorGUIUtility.singleLineHeight;
                var yOffset = rowHeight + 2f;
                var rowRect1 = new Rect(propertiesRect.x, propertiesRect.y, propertiesRect.width, rowHeight);
                var rowRect2 = new Rect(propertiesRect.x, propertiesRect.y + yOffset, propertiesRect.width, rowHeight);
                var rowRect3 = new Rect(propertiesRect.x, propertiesRect.y + yOffset * 2f, propertiesRect.width, rowHeight);
                var rowRect4 = new Rect(propertiesRect.x, propertiesRect.y + yOffset * 3f, propertiesRect.width, rowHeight);
                var slateField = string.Empty;
                var timecode = Timecode.FromSeconds(frameRate, time);

                if (shotAsset != null)
                {
                    slateField = $"[{shotAsset.SceneNumber.ToString("D3")}] {shotAsset.ShotName} TC [{timecode}]";
                }
                else
                {
                    slateField = $"TC [{timecode}]";
                }

                var lensAssetField = string.Empty;

                if (lensAsset != null)
                {
                    lensAssetField = $"Lens: {lensAsset.name}";
                }
                else
                {
                    lensAssetField = $"Lens: {Contents.Deleted}";
                }

                var lensField = $"{focalLength.ToString("F1")}mm f/{aperture}";
                var sensorField = GetSensorName(sensorSize);

                EditorGUI.LabelField(rowRect1, slateField);
                EditorGUI.LabelField(rowRect2, lensAssetField);
                EditorGUI.LabelField(rowRect3, lensField);
                EditorGUI.LabelField(rowRect4, sensorField);
            };
            m_List.OnRemoveCallback = () =>
            {
                Undo.RegisterCompleteObjectUndo(m_SnapshotLibrary, Contents.UndoDeleteSnapshot);
                m_SnapshotLibrary.RemoveAt(m_List.Index);
            };
        }

        void DrawNoneListItem(Rect rect)
        {
            GUI.Label(rect, m_NoneListItemContent);
        }

        Rect BestFit(Rect rect, Texture2D texture)
        {
            var rectAspect = rect.width / rect.height;
            var textureAspect = texture.width / texture.height;

            if (textureAspect > rectAspect)
            {
                var height = rect.width / textureAspect;

                rect.y += (rect.height - height) * 0.5f;
                rect.height = height;
            }
            else if (textureAspect < rectAspect)
            {
                var width = rect.height * textureAspect;

                rect.x += (rect.width - width) * 0.5f;
                rect.width = width;
            }

            return rect;
        }

        string GetSensorName(Vector2 sensorSize)
        {
            var sensorSizes = SensorPresetsCache.GetSensorSizes();
            var index = Array.FindIndex(sensorSizes, (s) => s == sensorSize);

            if (index == -1)
            {
                return $"[{sensorSize.x} x {sensorSize.y}]";
            }

            var options = SensorPresetsCache.GetSensorNames();

            return options[index];
        }
    }
}
