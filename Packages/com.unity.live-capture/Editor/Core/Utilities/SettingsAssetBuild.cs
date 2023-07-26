using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A utility class that copies <see cref="SettingAsset{T}"/> instances into a resources folder for builds.
    /// </summary>
    class SettingsAssetBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        static string s_Folder;

        /// <inheritdoc />
        public int callbackOrder { get; }

        /// <inheritdoc />
        public void OnPreprocessBuild(BuildReport report)
        {
            var path = $"Assets/{nameof(SettingsAssetBuild)}/Resources";
            var pathSplit = path.Split('/');

            s_Folder = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(pathSplit[0], pathSplit[1]));
            var resourcesFolder = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(s_Folder, pathSplit[2]));

            if (resourcesFolder == null)
            {
                return;
            }

            var settingTypes = TypeCache.GetTypesDerivedFrom(typeof(SettingAsset<>))
                .Where(t => !t.IsAbstract && !t.IsGenericType)
                .ToArray();

            foreach (var settingType in settingTypes)
            {
                var instanceProperty = settingType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (instanceProperty == null)
                {
                    continue;
                }

                var instance = instanceProperty.GetValue(null) as ScriptableObject;

                if (instance == null)
                {
                    continue;
                }

                var clone = Object.Instantiate(instance);

                AssetDatabase.CreateAsset(clone, Path.Combine(s_Folder, $"{clone.GetType().Name}.asset"));
                AssetDatabase.Refresh();
            }
        }

        /// <inheritdoc />
        public void OnPostprocessBuild(BuildReport report)
        {
            if (s_Folder != null)
            {
                AssetDatabase.DeleteAsset(s_Folder);
                s_Folder = null;
            }
        }
    }
}
