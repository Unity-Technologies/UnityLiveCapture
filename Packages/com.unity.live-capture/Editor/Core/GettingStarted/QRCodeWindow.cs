using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A window to display QR code links to the companion apps.
    /// </summary>
    class QRCodeWindow : EditorWindow
    {
        static class Contents
        {
            public const string WindowName = "Download iOS Apps";
            public static readonly GUIContent WindowTitle = EditorGUIUtility.TrTextContent(WindowName);
            public static readonly Vector2 MinWindowSize = new Vector2(200f, 300f);
        }

        [SerializeField]
        VisualTreeAsset m_WindowUxml;
        [SerializeField]
        StyleSheet m_WindowUss;

        public static EditorWindow DisplayModal()
        {
            var window = CreateInstance(typeof(QRCodeWindow)) as QRCodeWindow;
            window.ShowModalUtility();
            return window;
        }

        void OnEnable()
        {
            titleContent = Contents.WindowTitle;
            minSize = Contents.MinWindowSize;
            CenterOnScreen();
        }

        void CenterOnScreen()
        {
            var windowCenter = EditorGUIUtility.GetMainWindowPosition().center;
            var pos = position;
            pos.center = windowCenter;
            position = pos;
        }

        public void CreateGUI()
        {
            m_WindowUxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(m_WindowUss);
        }
    }
}
