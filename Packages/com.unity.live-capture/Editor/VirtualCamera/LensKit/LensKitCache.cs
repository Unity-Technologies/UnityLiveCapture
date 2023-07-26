using System.Linq;
using System.Collections.Generic;
using Unity.LiveCapture.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    class LensKitChangeDetector : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            LensKitCache.SetDirty();
        }
    }

    [InitializeOnLoad]
    static class LensKitCache
    {
        static LensAsset[] s_Presets;
        static GUIContent[] s_Contents;
        static bool s_Dirty;

        static LensKitCache()
        {
            Undo.postprocessModifications += PostprocessModifications;

            UpdateCache();
        }

        static UndoPropertyModification[] PostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var modification in modifications)
            {
                if (modification.currentValue != null &&
                    (modification.currentValue.target is LensKit ||
                     modification.currentValue.target is LensAsset))
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
            var lensKits = LoadAllAssetsOfType<LensKit>();

            s_Presets = lensKits.SelectMany(k => k.Lenses).ToArray();
            s_Contents = lensKits.SelectMany(k => GetContents(k)).ToArray();
        }

        static IEnumerable<GUIContent> GetContents(LensKit lensKit)
        {
            var formatter = new UniqueNameFormatter();

            return lensKit.Lenses.Select(l => new GUIContent($"{lensKit.name}/{formatter.Format(l.name)}"));
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
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        internal static void Get(out LensAsset[] presets, out GUIContent[] contents)
        {
            UpdateIfNeeded();

            presets = s_Presets;
            contents = s_Contents;
        }
    }
}
