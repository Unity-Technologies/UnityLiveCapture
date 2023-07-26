using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.TransformCapture
{
    static class TransformCaptureUtility
    {
        public static AvatarMask CreateAvatarMask(Transform root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            var avatarMask = new AvatarMask();
            var transforms = root.GetComponentsInChildren<Transform>(true);

            avatarMask.transformCount = transforms.Length;

            for (var i = 0; i < transforms.Length; ++i)
            {
                var transform = transforms[i];
                var path = AnimationUtility.CalculateTransformPath(transform, root);

                avatarMask.SetTransformPath(i, path);
                avatarMask.SetTransformActive(i, true);
            }

            return avatarMask;
        }

        public static T CreateAssetUsingFilePanel<T>(T asset, string name, string extension, string directory, string title)
            where T : UnityObject
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name));

            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(nameof(extension));

            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException(nameof(directory));

            if (string.IsNullOrEmpty(title))
                throw new ArgumentException(nameof(title));

            var path = $"{directory}/{name}.{extension}";

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var fileName = Path.GetFileNameWithoutExtension(path);

            path = EditorUtility.SaveFilePanelInProject(title, fileName, extension, string.Empty, directory);

            if (!string.IsNullOrEmpty(path))
            {
                return CreateOrReplaceAsset(asset, path);
            }

            return null;
        }

        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : UnityObject
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentException(nameof(path));

            var existing = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existing != null)
            {
                asset.name = existing.name;

                EditorUtility.CopySerialized(asset, existing);
                UnityObject.DestroyImmediate(asset);

                return existing;
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();

                return asset;
            }
        }

        public static string GetActiveProjectPath()
        {
            var type = typeof(ProjectWindowUtil);
            var method = type.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            var obj = method.Invoke(null, null);
            var path = obj.ToString();

            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                path = "Assets";

            return path;
        }
    }
}
