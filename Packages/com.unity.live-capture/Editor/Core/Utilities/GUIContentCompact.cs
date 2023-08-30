using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    class GUIContentCompact
    {
        public static readonly GUIContentCompact None = new GUIContentCompact();

        GUIContent m_Content = GUIContent.none;
        GUIContent m_ContentIcon = GUIContent.none;
        float m_Width;

        protected GUIContentCompact() { }

        public GUIContentCompact(GUIStyle style, string label, string tooltip, Texture2D icon)
        {
            m_Content = EditorGUIUtility.TrTextContentWithIcon(label, tooltip, icon);
            m_ContentIcon = EditorGUIUtility.TrIconContent(icon, tooltip);

            InitWidth(style);
        }

        public GUIContentCompact(GUIStyle style, string label, string tooltip, string icon)
        {
            m_Content = EditorGUIUtility.TrTextContentWithIcon(label, tooltip, icon);
            m_ContentIcon = EditorGUIUtility.TrIconContent(icon, tooltip);

            InitWidth(style);
        }

        void InitWidth(GUIStyle style)
        {
            EditorGUIUtility.SetIconSize(Vector2.one * 16f);

            m_Width = style.CalcSize(m_Content).x + 16f;

            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        public GUIContent Resolve(float width)
        {
            if (width < m_Width)
                return m_ContentIcon;

            return m_Content;
        }
    }
}
