using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A window to introduce users to the Live Capture package.
    /// </summary>
    class GettingStartedWindow : EditorWindow
    {
        static class Contents
        {
            public const string WindowName = "Getting Started";
            public const string WindowPath = "Window/Live Capture/" + WindowName;
            public static readonly GUIContent WindowTitle = EditorGUIUtility.TrTextContent(WindowName);
            public static readonly Vector2 MinWindowSize = new Vector2(240f, 300f);
        }

        static class Samples
        {
            public const string FaceCapture = "ARKit Face Sample";
        }

        static class URLs
        {
            public const string VirtualCamera = Documentation.baseURL + "virtual-camera-getting-started" + Documentation.endURL;
            public const string FaceCapture = Documentation.baseURL + "face-capture-getting-started" + Documentation.endURL;
            public const string ConnectionIssues = Documentation.baseURL + "troubleshooting" + Documentation.endURL;
            public const string Docs = Documentation.baseURL + "index" + Documentation.endURL;
            public const string Forum = "https://forum.unity.com/forums/virtual-production.466/";
            public const string CinematicsBeta = "mailto:cinematics-beta@unity3d.com";
        }

        static class Names
        {
            public const string VirtualCamera = "virtual-camera";
            public const string FaceCapture = "face-capture";
            public const string Download = "download";
            public const string ConnectionIssues = "connection-issues";
            public const string FaceCaptureSample = "open-face-capture-sample";
            public const string Documentation = "documentation";
            public const string Forum = "forum-help";
            public const string CinematicsBeta = "cinematics-beta";
        }

        [SerializeField]
        VisualTreeAsset m_WindowUxml;
        [SerializeField]
        StyleSheet m_WindowUss;

        /// <summary>
        /// Opens an instance of the getting started window.
        /// </summary>
        [MenuItem(Contents.WindowPath)]
        public static void ShowWindow()
        {
            GetWindow<GettingStartedWindow>();
        }

        void OnEnable()
        {
            titleContent = Contents.WindowTitle;
            minSize = Contents.MinWindowSize;
        }

        public void CreateGUI()
        {
            m_WindowUxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(m_WindowUss);

            rootVisualElement.Q<Button>(Names.VirtualCamera).clickable.clicked += () => Application.OpenURL(URLs.VirtualCamera);
            rootVisualElement.Q<Button>(Names.FaceCapture).clicked += () => Application.OpenURL(URLs.FaceCapture);
            rootVisualElement.Q<Button>(Names.Download).clicked += () => QRCodeWindow.DisplayModal();
            rootVisualElement.Q<Button>(Names.ConnectionIssues).clicked += () => Application.OpenURL(URLs.ConnectionIssues);
            rootVisualElement.Q<Button>(Names.FaceCaptureSample).clicked += OnFaceCaptureSampleClicked;
            rootVisualElement.Q<Button>(Names.Documentation).clicked += () => Application.OpenURL(URLs.Docs);
            rootVisualElement.Q<Button>(Names.Forum).clicked += () => Application.OpenURL(URLs.Forum);
            rootVisualElement.Q<Button>(Names.CinematicsBeta).clicked += () => Application.OpenURL(URLs.CinematicsBeta);
        }

        bool GetSample(string displayName, out Sample sample)
        {
            var samples = Sample.FindByPackage(LiveCaptureInfo.Name, LiveCaptureInfo.Version);

            foreach (var entry in samples)
            {
                if (entry.displayName == displayName)
                {
                    sample = entry;
                    return true;
                }
            }

            sample = default;
            return false;
        }

        bool ImportSample(Sample sample)
        {
            return sample.Import();
        }

        void BrowseToSample(Sample sample)
        {
            Debug.Log($"Imported sample {sample.displayName} into: {sample.importPath}");

#if UNITY_2021_3_OR_NEWER
            var projectPath = Path.GetDirectoryName(Application.dataPath);
            var importPath = Path.GetRelativePath(projectPath, sample.importPath);
            var o = AssetDatabase.LoadAssetAtPath(importPath, typeof(Object));

            EditorUtility.FocusProjectWindow();
            ProjectWindowUtil.ShowCreatedAsset(o);
            Selection.activeObject = o;
#endif
        }

        void OnFaceCaptureSampleClicked()
        {
            if (GetSample(Samples.FaceCapture, out var sample))
            {
                ImportSample(sample);
                BrowseToSample(sample);
            }
        }
    }
}
