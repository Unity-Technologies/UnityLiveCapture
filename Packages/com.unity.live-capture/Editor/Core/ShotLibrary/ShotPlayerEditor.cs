using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ShotPlayer))]
    class ShotPlayerEditor : Editor
    {
        static class Contents
        {
            public static readonly GUIContent ShotLibrary = EditorGUIUtility.TrTextContent("Shot Library", "The Shot Library asset that contains the available shots.");
            public static readonly GUIContent Selection = EditorGUIUtility.TrTextContent("Shot", "The selected shot to play.");
            public static GUIContent LibraryCreateNew = EditorGUIUtility.TrTextContent("Create", "Create a new Shot Library asset and assign it to this player.");
            public const string k_NewShotLibrary = "New shot library";
        }

        ShotPlayer m_ShotPlayer;
        SerializedProperty m_ShotLibrary;
        SerializedProperty m_Selection;
        Editor m_LibraryEditor;
        Editor m_BindingsEditor;

        void OnEnable()
        {
            m_ShotPlayer = target as ShotPlayer;
            m_ShotLibrary = serializedObject.FindProperty("m_ShotLibrary");
            m_Selection = serializedObject.FindProperty("m_Selection");
        }

        void OnDisable()
        {
            DestroyImmediate(m_LibraryEditor);
        }

        public override void OnInspectorGUI()
        {
            using var change = new EditorGUI.ChangeCheckScope();

            serializedObject.Update();

            ShotLibraryField();
            DrawLibraryEditor();

            serializedObject.ApplyModifiedProperties();

            DrawBindingsEditor();

            if (change.changed)
            {
                TakeRecorderWindow.RepaintWindow();
            }
        }

        void DrawLibraryEditor()
        {
            CreateCachedEditor(m_ShotLibrary.objectReferenceValue, null, ref m_LibraryEditor);

            var selection = m_Selection.intValue;

            if (m_LibraryEditor is ShotLibraryEditor editor)
            {
                editor.Index = selection;
                editor.OnInspectorGUI();
                selection = editor.Index;
            }

            if (selection != m_Selection.intValue)
            {
                m_Selection.intValue = selection;

                GUI.changed = true;
            }
        }

        void DrawBindingsEditor()
        {
            var take = default(Take);
            var selection = m_ShotPlayer.Selection;
            var director = m_ShotPlayer.Director;
            var shotLibrary = m_ShotPlayer.ShotLibrary;

            if (shotLibrary != null && selection != -1)
            {
                var shot = shotLibrary.GetShot(selection);

                take = shot.Take;
            }

            if (take == null)
            {
                return;
            }

            Editor.CreateCachedEditorWithContext(take, director, typeof(TakeBindingsEditor), ref m_BindingsEditor);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                m_BindingsEditor.OnInspectorGUI();

                if (change.changed)
                {
                    m_ShotPlayer.ClearSceneBindings();
                    m_ShotPlayer.SetSceneBindings();
                    director.RebuildGraph();
                }
            }
        }

        void ShotLibraryField()
        {
            var lineRect = EditorGUILayout.GetControlRect();
            var fieldWidth = lineRect.width - EditorGUIUtility.labelWidth;
            var buttonWidth = Mathf.Min(60f, fieldWidth);
            var fieldRect = new Rect(lineRect.x, lineRect.y, lineRect.width - buttonWidth, lineRect.height);
            var buttonNewRect = new Rect(fieldRect.xMax, lineRect.y, buttonWidth, lineRect.height);
            var isNull = m_ShotLibrary.objectReferenceValue == null;

            EditorGUI.PropertyField(fieldRect, m_ShotLibrary, Contents.ShotLibrary);

            if (isNull && m_ShotLibrary.objectReferenceValue != null)
            {
                m_Selection.intValue = 0;
            }

            if (GUI.Button(buttonNewRect, Contents.LibraryCreateNew, EditorStyles.miniButton))
            {
                if (TryCreateShotLibrary(Contents.k_NewShotLibrary, out var library))
                {
                    m_ShotLibrary.objectReferenceValue = library;

                    EditorGUIUtility.PingObject(library);
                }
            }
        }

        internal static bool TryCreateShotLibrary(string name, out ShotLibrary library)
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create new Shot Library",
                name,
                "asset",
                "Create a new Shot Library at the selected directory"
            );

            library = default(ShotLibrary);

            if (!string.IsNullOrEmpty(path))
            {
                library = ScriptableObject.CreateInstance<ShotLibrary>();

                library.Shots = new[]
                {
                    new Shot()
                    {
                        Name = "New Shot"
                    }
                };

                AssetDatabase.CreateAsset(library, path);
                AssetDatabase.Refresh();

                return true;
            }

            return false;
        }
    }
}
