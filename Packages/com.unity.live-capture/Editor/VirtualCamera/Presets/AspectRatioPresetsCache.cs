using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    class AspectRatioPresetsAssetChangeDetector : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            AspectRatioPresetsCache.SetDirty();
        }
    }

    [InitializeOnLoad]
    static class AspectRatioPresetsCache
    {
        static class Contents
        {
            public static string CustomStr = "Custom";
        }

        static float[] s_AspectRatios;
        static string[] s_AspectRatioNames;
        static string[] s_AspectRatioNamesWithCustom;
        static GUIContent[] s_AspectRatioNameContents;
        static GUIContent[] s_AspectRatioNameContentsWithCustom;
        static bool s_Dirty;

        static AspectRatioPresetsCache()
        {
            Undo.postprocessModifications += PostprocessModifications;

            UpdateCache();
        }

        static UndoPropertyModification[] PostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var modification in modifications)
            {
                if (modification.currentValue != null &&
                    modification.currentValue.target is AspectRatioPresets)
                {
                    SetDirty();

                    break;
                }
            }

            return modifications;
        }

        public static void SetDirty()
        {
            s_Dirty = true;
        }

        static void UpdateIfNeeded()
        {
            if (s_Dirty)
            {
                UpdateCache();

                s_Dirty = false;
            }
        }

        static void UpdateCache()
        {
            var presets = LoadAllAssetsOfType<AspectRatioPresets>();

            var aspectPresets = presets
                .SelectMany(f => f.AspectRatios)
                .ToLookup(a => a.AspectRatio, a => a.Name);

            s_AspectRatios = aspectPresets.Select(a => a.Key).ToArray();
            s_AspectRatioNames = aspectPresets
                .Select(a => $"{a.Key} : 1 - {string.Join(", ", a)}")
                .ToArray();
            s_AspectRatioNamesWithCustom = s_AspectRatioNames.Append(Contents.CustomStr).ToArray();
            s_AspectRatioNameContents = s_AspectRatioNames.Select(n => new GUIContent(n)).ToArray();
            s_AspectRatioNameContentsWithCustom = s_AspectRatioNamesWithCustom.Select(n => new GUIContent(n)).ToArray();
        }

        static string Format(float number)
        {
            return string.Format("{0:0.00}", number);
        }

        static T[] LoadAllAssetsOfType<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => LoadAssetAtPath<T>(path))
                .ToArray();
        }

        static T LoadAssetAtPath<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path) as T;
        }

        public static float[] GetAspectRatios()
        {
            UpdateIfNeeded();

            return s_AspectRatios;
        }

        public static string[] GetAspectRatioNames()
        {
            UpdateIfNeeded();

            return s_AspectRatioNames;
        }

        public static GUIContent[] GetAspectRatioNameContents()
        {
            UpdateIfNeeded();

            return s_AspectRatioNameContents;
        }

        public static string[] GetAspectRatioNamesWithCustom()
        {
            UpdateIfNeeded();

            return s_AspectRatioNamesWithCustom;
        }

        public static GUIContent[] GetAspectRatioNameContentsWithCustom()
        {
            UpdateIfNeeded();

            return s_AspectRatioNameContentsWithCustom;
        }
    }
}
