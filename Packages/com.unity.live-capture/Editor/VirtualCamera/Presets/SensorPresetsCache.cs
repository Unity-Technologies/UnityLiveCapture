using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    class SensorPresetsAssetChangeDetector : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            SensorPresetsCache.SetDirty();
        }
    }

    [InitializeOnLoad]
    static class SensorPresetsCache
    {
        static class Contents
        {
            public static string CustomStr = "Custom";
        }

        static Vector2[] s_SensorSizes;
        static string[] s_SensorNames;
        static string[] s_SensorNamesWithCustom;
        static GUIContent[] s_SensorNameContents;
        static GUIContent[] s_SensorNameContentsWithCustom;
        static bool s_Dirty;

        static SensorPresetsCache()
        {
            Undo.postprocessModifications += PostprocessModifications;
            SensorPresetsCacheProxy.GetSensorSizeName = GetSensorSizeName;

            UpdateCache();
        }

        static UndoPropertyModification[] PostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var modification in modifications)
            {
                if (modification.currentValue != null &&
                    modification.currentValue.target is SensorPresets)
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
            var presets = LoadAllAssetsOfType<SensorPresets>();
            var sensorPresets = presets
                .SelectMany(f => f.Sensors)
                .ToLookup(s => s.SensorSize, s => s.Name);

            s_SensorSizes = sensorPresets.Select(s => s.Key).ToArray();
            s_SensorNames = sensorPresets
                .Select(s => $"[{Format(s.Key.x)} x {Format(s.Key.y)}] {string.Join(", ", s)}")
                .ToArray();
            s_SensorNamesWithCustom = s_SensorNames.Append(Contents.CustomStr).ToArray();
            s_SensorNameContents = s_SensorNames.Select(n => new GUIContent(n)).ToArray();
            s_SensorNameContentsWithCustom = s_SensorNamesWithCustom.Select(n => new GUIContent(n)).ToArray();
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

        public static Vector2[] GetSensorSizes()
        {
            UpdateIfNeeded();

            return s_SensorSizes;
        }

        public static string[] GetSensorNames()
        {
            UpdateIfNeeded();

            return s_SensorNames;
        }

        public static GUIContent[] GetSensorNameContents()
        {
            UpdateIfNeeded();

            return s_SensorNameContents;
        }

        public static string[] GetSensorNamesWithCustom()
        {
            UpdateIfNeeded();

            return s_SensorNamesWithCustom;
        }

        public static GUIContent[] GetSensorNameContentsWithCustom()
        {
            UpdateIfNeeded();

            return s_SensorNameContentsWithCustom;
        }

        static string GetSensorSizeName(Vector2 size)
        {
            for (var i = 0; i != s_SensorSizes.Length; ++i)
            {
                var s = s_SensorSizes[i];
                if (Mathf.Approximately(s.x, size.x) && Mathf.Approximately(s.y, size.y))
                {
                    return s_SensorNames[i];
                }
            }

            return String.Empty;
        }
    }
}
