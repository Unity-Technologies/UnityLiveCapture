using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// Manages Unity editor window layouts.
    /// </summary>
    static class Layouts
    {
        static class FilePaths
        {
            public const string Base = "Packages/com.unity.live-capture/Editor/Core/Layouts/";
            public const string LiveCaptureDefaultLayout = Base + "Live Capture.wlt";
            public const string LiveCaptureWithSynchronizationLayout = Base + "Synchronization.wlt";
        }

        static class MenuItemPaths
        {
            public const string Base = "Window/Live Capture/Layout/";
            public const string LiveCaptureDefaultLayout = Base + "Live Capture Default";
            public const string LiveCaptureWithSynchronizationLayout = Base + "Live Capture with Synchronization";
        }

        private const int k_BasePriority = 100;

        /// <summary>
        /// Activates the Live Capture window layout.
        /// </summary>
        [MenuItem(MenuItemPaths.LiveCaptureDefaultLayout, priority = k_BasePriority + 0)]
        public static void ActivateLiveCaptureLayout()
        {
            LoadLayout(FilePaths.LiveCaptureDefaultLayout);
        }

        /// <summary>
        /// Activates the Live Capture with Synchronization window layout.
        /// </summary>
        [MenuItem(MenuItemPaths.LiveCaptureWithSynchronizationLayout, priority = k_BasePriority + 1)]
        public static void ActivateLiveCaptureWithSynchronizationLayout()
        {
            LoadLayout(FilePaths.LiveCaptureWithSynchronizationLayout);
        }

        static void LoadLayout(string path)
        {
            if (System.IO.File.Exists(path))
            {
                EditorUtility.LoadWindowLayout(path);
            }
            else
            {
                Debug.LogWarning("Layout not loaded. Layout file missing at: " + path);
            }
        }
    }
}
