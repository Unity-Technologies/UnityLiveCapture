using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    class TakeOrganizerWindow : EditorWindow
    {
        static class Styles
        {
            public const float kDragPadding = 3f;
            public const float kMinDragWidth = 20f;
            public const float kBottomToolbarHeight = 21f;
            public static readonly GUIStyle PreToolbar = "preToolbar";
            public static readonly GUIStyle PreToolbar2 = "preToolbar2";
            public static readonly GUIStyle PreToolbarLabel = "ToolbarBoldLabel";
            public static GUIStyle PreBackground = "preBackground";
            public static readonly GUIStyle DragHandle = "RL DragHandle";
            public static readonly GUIContent PreTitle = EditorGUIUtility.TrTextContent("Preview");
        }

        const float kBottomToolbarHeight = 21f;
        const string k_WindowName = "Take Organizer";
        const string k_WindowPath = "Window/Live Capture/" + k_WindowName;

        internal static TakeOrganizerWindow Instance { get; private set; }

        [SerializeField]
        VisualTreeAsset m_WindowUxml;
        [SerializeField]
        StyleSheet m_WindowCommonUss;
        [SerializeField]
        StyleSheet m_WindowLightUss;
        [SerializeField]
        StyleSheet m_WindowDarkUss;
        [SerializeField]
        DirectoryTreeView m_DirectoryTreeView = new DirectoryTreeView();
        [SerializeField]
        TakeTreeView m_TakeTreeView = new TakeTreeView();
        bool m_NeedsReload;

        Editor m_Editor;
        VisualElement m_EditorRootElement;

        [MenuItem(k_WindowPath)]
        static void ShowWindow()
        {
            GetWindow<TakeOrganizerWindow>();
        }

        void OnEnable()
        {
            Instance = this;

            titleContent = new GUIContent(k_WindowName);

            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void OnDisable()
        {
            Instance = null;

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            DestroyImmediate(m_Editor);
        }

        void UndoRedoPerformed()
        {
            DestroyImmediate(m_Editor);

            Repaint();
        }

        internal void SetNeedsReload()
        {
            m_NeedsReload = true;
        }

        void CreateGUI()
        {
            m_WindowUxml.CloneTree(rootVisualElement);

            var hierarchy = rootVisualElement.Q<IMGUIContainer>("hierarchy");

            hierarchy.onGUIHandler = () =>
            {
                var rect = hierarchy.parent.parent.contentRect;

                EditorGUILayout.GetControlRect(false, rect.height);

                m_DirectoryTreeView.OnGUI(rect);
            };

            var list = rootVisualElement.Q<IMGUIContainer>("list");

            list.onGUIHandler = () =>
            {
                var rect = list.parent.parent.contentRect;

                EditorGUILayout.GetControlRect(false, rect.height);

                m_TakeTreeView.SetTakes(m_DirectoryTreeView.SelectedTakes);
                m_TakeTreeView.OnGUI(rect);
            };

            var inspector = rootVisualElement.Q<IMGUIContainer>("inspector");

            inspector.onGUIHandler = () =>
            {
                using (new InspectorScope(inspector.contentRect.width))
                {
                    var takes = m_TakeTreeView.Selection;

                    PrepareEditor(takes, ref m_Editor, ref m_EditorRootElement);

                    if (m_Editor != null)
                    {
                        m_Editor.OnInspectorGUI();

                        EditorGUILayout.Space();
                    }
                }
            };

            var previewHeader = rootVisualElement.Q<IMGUIContainer>("preview-header");
            previewHeader.AddManipulator(new PreviewResizer());

            previewHeader.onGUIHandler = () =>
            {
                if (m_Editor != null)
                {
                    EditorGUILayout.BeginHorizontal(Styles.PreToolbar, GUILayout.Height(Styles.kBottomToolbarHeight));
                    {
                        GUILayout.FlexibleSpace();

                        var previewTitle = m_Editor.GetPreviewTitle() ?? Styles.PreTitle;
                        var dragRect = GUILayoutUtility.GetLastRect();
                        var dragImageRect = GetDragImageRect(dragRect);
                        var labelRect = GetLabelRect(dragRect, dragImageRect, previewTitle);

                        GUI.Label(labelRect, previewTitle, Styles.PreToolbarLabel);

                        if (Event.current.type == EventType.Repaint)
                        {
                            dragImageRect.xMin = labelRect.xMax + Styles.kDragPadding;

                            Styles.DragHandle.Draw(dragImageRect, GUIContent.none, false, false, false, false);
                        }

                    }
                    EditorGUILayout.EndHorizontal();
                }
            };

            var preview = rootVisualElement.Q<IMGUIContainer>("preview");

            preview.onGUIHandler = () =>
            {
                var rect = preview.contentRect;

                if (m_Editor != null && rect.height > 0f)
                {
                    m_Editor.DrawPreview(rect);
                }
            };
        }

        Rect GetDragImageRect(Rect dragRect)
        {
            return new Rect()
            {
                x = dragRect.x + Styles.kDragPadding,
                y = dragRect.y + 1 + (kBottomToolbarHeight - Styles.DragHandle.fixedHeight) / 2,
                width = dragRect.width - Styles.kDragPadding * 2,
                height = Styles.DragHandle.fixedHeight
            };
        }

        Rect GetLabelRect(Rect dragRect, Rect dragImageRect, GUIContent label)
        {
            var maxLabelWidth = (dragImageRect.xMax - dragRect.xMin) - Styles.kDragPadding - Styles.kMinDragWidth;
            var labelWidth = Mathf.Min(maxLabelWidth, Styles.PreToolbar2.CalcSize(label).x);

            return new Rect(dragRect.x, dragRect.y, labelWidth, dragRect.height);
        }

        void Update()
        {
            if (m_NeedsReload)
            {
                m_DirectoryTreeView.Reload();
                m_NeedsReload = false;
            }
        }

        static void PrepareEditor(UnityObject[] targets, ref Editor editor, ref VisualElement root)
        {
            var hasTargets = targets != null && targets.Length > 0;

            if (!hasTargets)
            {
                DestroyImmediate(editor);
                root = null;

                return;
            }

            if (editor == null || !Enumerable.SequenceEqual(editor.targets, targets))
            {
                Editor.CreateCachedEditor(targets, null, ref editor);
                root = editor.CreateInspectorGUI();
            }
        }
    }
}
