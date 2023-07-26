using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(Take), true)]
    [CanEditMultipleObjects]
    class TakeEditor : Editor
    {
        static class Contents
        {
            public static readonly string Unknown = L10n.Tr("Unknown");
            public static readonly string OverrideStr = L10n.Tr("Override");
            public static GUIStyle LockStyle = "IN LockButton";
            public static readonly GUIContent FrameRate = EditorGUIUtility.TrTextContent("Frame Rate", "The frame rate used during the recording.");
            public static readonly GUIContent StartTime = EditorGUIUtility.TrTextContent("Start Time", "The timecode of the first frame of the recording.");
            public static readonly GUIContent SceneNumber = EditorGUIUtility.TrTextContent("Scene Number", "The number associated with the scene where the take was captured.");
            public static readonly GUIContent ShotName = EditorGUIUtility.TrTextContent("Shot Name", "The name of the shot where the take was captured.");
            public static readonly GUIContent TakeNumber = EditorGUIUtility.TrTextContent("Take Number", "The number associated with the take.");
            public static readonly GUIContent Description = EditorGUIUtility.TrTextContent("Description", "The description of the shot where the take was captured.");
            public static readonly GUIContent Rating = EditorGUIUtility.TrTextContent("Rating", "The rating of the take.");
            public static readonly GUIContent Screenshot = EditorGUIUtility.TrTextContent("Screenshot", "The screenshot at the beginning of the take.");
            public static readonly GUIContent Metadata = EditorGUIUtility.TrTextContent("Metadata", "Metadata associated with the recorded contents.");
            public static readonly GUIContent FixNames = EditorGUIUtility.TrTextContentWithIcon(
                "Inconsistent Take name.",
                "console.warnicon");
        }

        Editor m_PreviewEditor;
        Texture2D[] m_Screenshots;
        SerializedProperty m_StartTimecode;
        SerializedProperty m_SceneNumber;
        SerializedProperty m_ShotName;
        SerializedProperty m_TakeNumber;
        SerializedProperty m_Description;
        SerializedProperty m_Rating;
        SerializedProperty m_FrameRate;
        SerializedProperty m_MetadataEntries;
        Dictionary<TrackAsset, string> m_NameCache = new Dictionary<TrackAsset, string>();
        bool m_NeedsRename;

        void OnEnable()
        {
            m_Screenshots = targets
                .Cast<Take>()
                .Select(t => t.Screenshot)
                .Where(s => s != null)
                .ToArray();

            m_StartTimecode = serializedObject.FindProperty("m_StartTimecode");
            m_SceneNumber = serializedObject.FindProperty("m_SceneNumber");
            m_ShotName = serializedObject.FindProperty("m_ShotName");
            m_TakeNumber = serializedObject.FindProperty("m_TakeNumber");
            m_Description = serializedObject.FindProperty("m_Description");
            m_Rating = serializedObject.FindProperty("m_Rating");
            m_FrameRate = serializedObject.FindProperty("m_FrameRate");
            m_MetadataEntries = serializedObject.FindProperty("m_MetadataEntries");

            BuildCache();

            m_NeedsRename = NeedsRename();
        }

        void OnDisable()
        {
            DestroyImmediate(m_PreviewEditor);
        }

        void BuildCache()
        {
            m_NameCache.Clear();

            var bindingEntries = serializedObject.FindProperty("m_Entries");

            for (var i = 0; i < bindingEntries.arraySize; ++i)
            {
                var bindingEntry = bindingEntries.GetArrayElementAtIndex(i);
                var track = bindingEntry.FindPropertyRelative("m_Track");
                var exposedPropertyNameProp = bindingEntry
                    .FindPropertyRelative("m_Binding.m_ExposedReference.exposedName");
                var exposedNameStr = exposedPropertyNameProp.stringValue;

                m_NameCache.Add(
                    track.objectReferenceValue as TrackAsset,
                    exposedNameStr);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Rating, Contents.Rating);
            EditorGUILayout.Space();

            var editable = DoLockField();

            using (new EditorGUI.DisabledScope(!editable))
            {
                EditorGUILayout.PropertyField(m_FrameRate, Contents.FrameRate);
                EditorGUILayout.PropertyField(m_StartTimecode, Contents.StartTime);

                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.PropertyField(m_SceneNumber, Contents.SceneNumber);
                    EditorGUILayout.PropertyField(m_ShotName, Contents.ShotName);
                    EditorGUILayout.PropertyField(m_TakeNumber, Contents.TakeNumber);

                    if (change.changed)
                    {
                        serializedObject.ApplyModifiedProperties();

                        m_NeedsRename = NeedsRename();
                    }
                }
            }

            if (m_NeedsRename)
            {
                LiveCaptureGUI.DrawFixMeBox(Contents.FixNames, () =>
                {
                    if (TryRename())
                    {
                        m_NeedsRename = false;
                    }
                });
            }

            using (new EditorGUI.DisabledScope(!editable))
            {
                EditorGUILayout.PropertyField(m_Description, Contents.Description);

                DrawMetadata(m_MetadataEntries);

                serializedObject.ApplyModifiedProperties();
            }
        }

        bool DoLockField()
        {
            var editable = SessionState.GetBool("live-capture-take-editor-editable", false);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();


                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    editable = !GUILayout.Toggle(!editable, GUIContent.none, Contents.LockStyle);

                    if (change.changed)
                    {
                        SessionState.SetBool("live-capture-take-editor-editable", editable);
                    }
                }
            }

            return editable;
        }

        public override bool HasPreviewGUI()
        {
            return m_Screenshots.Length > 0;
        }

        public override void DrawPreview(Rect previewArea)
        {
            CreatePreviewEditorIfNeeded();

            m_PreviewEditor.DrawPreview(previewArea);
        }

        void CreatePreviewEditorIfNeeded()
        {
            CreateCachedEditor(m_Screenshots, null, ref m_PreviewEditor);
        }

        void DrawMetadata(SerializedProperty metadataEntriesProp)
        {
            if (targets.Length > 1 || metadataEntriesProp.arraySize == 0)
            {
                return;
            }

            metadataEntriesProp.isExpanded = EditorGUILayout.Foldout(metadataEntriesProp.isExpanded, Contents.Metadata, true);

            if (m_MetadataEntries.isExpanded)
            {
                using var _ = new EditorGUI.IndentLevelScope();

                for (var i = 0; i < metadataEntriesProp.arraySize; ++i)
                {
                    var entryProp = metadataEntriesProp.GetArrayElementAtIndex(i);
                    var trackProp = entryProp.FindPropertyRelative("m_Track");
                    var metadataProp = entryProp.FindPropertyRelative("m_Metadata");
                    var track = trackProp.objectReferenceValue as TrackAsset;
                    var isOverride = track is AnimationTrack && track.parent is AnimationTrack;

                    if (!m_NameCache.TryGetValue(track, out var actorName))
                    {
                        actorName = Contents.Unknown;
                    }

                    var overrideStr = isOverride ? $" ({Contents.OverrideStr})" : string.Empty;
                    var label = new GUIContent($"{track.name}{overrideStr} - {actorName}");

                    EditorGUILayout.PropertyField(metadataProp, label, true);
                }
            }
        }

        bool NeedsRename()
        {
            foreach (var target in targets)
            {
                if (NeedsRename(target as Take))
                {
                    return true;
                }
            }

            return false;
        }

        bool TryRename()
        {
            // Renaming is done in two separate passes. It's because asset renaming and asset modification (sub-asset renaming)
            // can't be performed inside the same AssetDatabase.StartAssetEditing() scope.

            // Flush any pending Take modifications
            AssetDatabase.SaveAssets();

            var result = true;

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var target in targets)
                {
                    if (!TryRenameAssets(target as Take))
                    {
                        result = false;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var target in targets)
                {
                    var take = target as Take;
                    var currentName = take.name;
                    var newName = GetFormattedName(take);
                    var takeHasCorrectName = currentName == newName;

                    if (takeHasCorrectName && !TryRenameSubAssets(target as Take))
                    {
                        result = false;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }

            return result;
        }

        static bool NeedsRename(Take take)
        {
            var assetName = GetFormattedName(take);
            var timeline = take.Timeline;

            return (take.name != assetName || timeline.name != assetName);
        }

        static bool TryRenameAssets(Take take)
        {
            Debug.Assert(take != null);

            var newName = GetFormattedName(take);
            var screenshot = take.Screenshot;

            if (!TryRenameAssetAndLogError(take, newName))
            {
                return false;
            }

            // Rename screenshot too.
            TryRenameAssetAndLogError(screenshot, newName);

            return true;
        }

        static bool TryRenameSubAssets(Take take)
        {
            Debug.Assert(take != null);

            var newName = GetFormattedName(take);
            var timeline = take.Timeline;

            if (timeline != null)
            {
                timeline.name = newName;
            }

            return true;
        }

        static bool TryRenameAssetAndLogError(Object obj, string name)
        {
            if (obj == null)
            {
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(obj);
            var error = AssetDatabase.RenameAsset(assetPath, name);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);

                return false;
            }

            return true;
        }

        static string GetFormattedName(Take take)
        {
            var formatter = TakeNameFormatter.Instance;

            formatter.ConfigureTake(take.SceneNumber, take.ShotName, take.TakeNumber);

            return formatter.GetTakeName();
        }
    }
}
