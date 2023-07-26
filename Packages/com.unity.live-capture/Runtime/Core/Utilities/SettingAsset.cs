using System;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A class used to define a singleton instance that is stored as an asset.
    /// </summary>
    /// <remarks>
    /// Unlike ScriptableSingleton, this class can be used outside of the editor.
    /// </remarks>
    /// <typeparam name="T">The type of the setting asset.</typeparam>
    abstract class SettingAsset<T> : ScriptableObject where T : ScriptableObject
    {
        static T s_Instance;

        /// <summary>
        /// The asset instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = CreateOrLoad();
                return s_Instance;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SettingAsset{T}"/> instance.
        /// </summary>
        protected SettingAsset()
        {
            if (s_Instance == null)
            {
                s_Instance = this as T;
            }
        }

        static T CreateOrLoad()
        {
#if UNITY_EDITOR
            var filePath = GetFilePath();

            if (!string.IsNullOrEmpty(filePath))
            {
                s_Instance = InternalEditorUtility.LoadSerializedFileAndForget(filePath)
                    .Cast<T>()
                    .FirstOrDefault(s => s != null);
            }
#else
            s_Instance = Resources.Load<T>(typeof(T).Name);
#endif
            if (s_Instance == null)
            {
                s_Instance = CreateInstance<T>();
                s_Instance.hideFlags = HideFlags.DontSave;
            }

            return s_Instance;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Serializes the asset to disk.
        /// </summary>
        public static void Save()
        {
            if (s_Instance == null)
            {
                Debug.LogError($"Cannot save {nameof(SettingAsset<T>)}: no instance!");
                return;
            }

            var filePath = GetFilePath();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { s_Instance }, filePath, true);
        }

        /// <summary>
        /// Gets the file path of the asset relative to the project root folder.
        /// </summary>
        /// <returns>The file path of the asset.</returns>
        protected static string GetFilePath()
        {
            foreach (var customAttribute in typeof(T).GetCustomAttributes(true))
            {
                if (customAttribute is SettingFilePathAttribute attribute)
                {
                    return attribute.FilePath;
                }
            }
            return string.Empty;
        }

#endif
    }

    /// <summary>
    /// An attribute that specifies a file location relative to the Project folder or Unity's preferences folder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class SettingFilePathAttribute : Attribute
    {
        internal string FilePath { get; }

        public SettingFilePathAttribute(string relativePath, Location location)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("Path is empty.", nameof(relativePath));

            FilePath = CombineFilePath(relativePath, location);
        }

        static string CombineFilePath(string relativePath, Location location)
        {
            if (relativePath[0] == '/')
                relativePath = relativePath.Substring(1);

            switch (location)
            {
#if UNITY_EDITOR
                case Location.PreferencesFolder:
                    return InternalEditorUtility.unityPreferencesFolder + '/' + relativePath;
#endif
                case Location.ProjectFolder:
                    return relativePath;
                default:
                    Debug.LogError(("Unhandled enum: " + location));
                    return relativePath;
            }
        }

        /// <summary>
        /// Specifies the folder location that Unity uses together with the relative path provided in the <see cref="SettingFilePathAttribute"/> constructor.
        /// </summary>
        public enum Location
        {
            /// <summary>
            /// Use this location to save a file relative to the preferences folder. Useful for per-user files that are across all projects.
            /// </summary>
            PreferencesFolder,
            /// <summary>
            /// Use this location to save a file relative to the Project Folder. Useful for per-project files (not shared between projects).
            /// </summary>
            ProjectFolder,
        }
    }
}
