using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Editor
{
    [Serializable]
    class ContextEditorCache : IDisposable
    {
        static readonly Type s_EditorType = typeof(TakeRecorderContextEditor);

        [SerializeField]
        TakeRecorderContextEditor m_CachedEditor;
        bool m_Disposed;
        Dictionary<Type, Type> m_Types = new Dictionary<Type, Type>();

        public ContextEditorCache()
        {
            var editorTypes = TypeCache.GetTypesDerivedFrom<TakeRecorderContextEditor>();

            foreach (var editorType in editorTypes)
            {
                var attribute = editorType.GetAttribute<ContextEditor>(true);

                if (attribute != null)
                {
                    m_Types.Add(attribute.Type, editorType);
                }
            }
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(ContextEditorCache));
            }

            m_Disposed = true;

            Destroy();
        }

        public TakeRecorderContextEditor CreateCachedEditor(ITakeRecorderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var type = context.GetType();

            if (!m_Types.TryGetValue(type, out var editorType))
            {
                editorType = s_EditorType;
            }

            var cachedType = m_CachedEditor != null ? m_CachedEditor.GetType() : null;

            if (cachedType != editorType)
            {
                Destroy();

                m_CachedEditor = ScriptableObject.CreateInstance(editorType) as TakeRecorderContextEditor;
                m_CachedEditor.hideFlags = HideFlags.HideAndDontSave;
            }

            m_CachedEditor.SetContext(context);

            return m_CachedEditor;
        }

        void Destroy()
        {
            Object.DestroyImmediate(m_CachedEditor);
        }
    }
}
