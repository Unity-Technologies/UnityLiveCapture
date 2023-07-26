using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    class TakeRecorderContextEditor : ScriptableObject
    {
        [SerializeField]
        TreeViewState m_DefaultTreeViewState = new TreeViewState();
        DefaultTreeView m_DefaultTreeView;
        ShotEditor m_DefaultShotEditor = new ShotEditor();
        Editor m_BindingsEditor;

        public ITakeRecorderContext Context { get; private set; }

        protected void OnDisable()
        {
            DestroyImmediate(m_BindingsEditor);
        }

        internal void SetContext(ITakeRecorderContext context)
        {
            Debug.Assert(context != null);

            Context = context;
        }

        public virtual void OnShotGUI(Rect rect)
        {
            DrawDefaultShotList(rect);
        }

        public virtual void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        public void DrawDefaultShotList(Rect rect)
        {
            InitializeIfNeeded();

            m_DefaultTreeView.Update(Context);
            m_DefaultTreeView.OnGUI(rect);
        }

        public void DrawDefaultInspector()
        {
            Debug.Assert(Context != null);

            if (!Context.IsValid() || Context.GetSelectedShot() is not Shot shot)
            {
                return;
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var newShot = m_DefaultShotEditor.OnGUI(shot);

                if (change.changed)
                {
                    var resolver = Context.GetResolver() as UnityEngine.Object;

                    if (resolver != null)
                    {
                        Undo.RegisterCompleteObjectUndo(resolver, "Inspector");
                    }

                    var storage = Context.GetShotStorage();

                    if (storage != null)
                    {
                        Undo.RegisterCompleteObjectUndo(storage, "Inspector");
                        EditorUtility.SetDirty(storage);
                    }

                    Context.SetShotAndBindings(newShot);
                    Context.Rebuild();
                }
            }
        }

        internal void DrawBindingsInspector()
        {
            DrawBindingsInspector(Context, ref m_BindingsEditor);
        }

        void InitializeIfNeeded()
        {
            if (m_DefaultTreeView == null)
            {
                m_DefaultTreeView = new DefaultTreeView(m_DefaultTreeViewState);
                m_DefaultTreeView.Reload();
            }
        }

        internal static void DrawBindingsInspector(ITakeRecorderContext context, ref Editor editor)
        {
            Debug.Assert(context != null);

            var resolver = context.GetResolver() as Object;

            if (resolver == null || context.GetSelectedShot() is not Shot shot || shot.Take == null)
            {
                return;
            }

            var take = shot.Take;

            Editor.CreateCachedEditorWithContext(take, resolver, typeof(TakeBindingsEditor), ref editor);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                editor.OnInspectorGUI();

                if (change.changed)
                {
                    context.ClearSceneBindings();
                    context.SetSceneBindings();
                    context.Rebuild();
                }
            }
        }
    }
}
