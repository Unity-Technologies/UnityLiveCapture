using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture
{
    static class Screenshot
    {
        public static Texture2D Take(Camera camera)
        {
            return Take(camera, 1f);
        }

        public static Texture2D Take(Camera camera, float scale)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            scale = Mathf.Clamp01(scale);

            var width = Mathf.Max(1, Mathf.RoundToInt(camera.pixelWidth * scale));
            var height = Mathf.Max(1, Mathf.RoundToInt(camera.pixelHeight * scale));
            var prevCameraRenderTexture = camera.targetTexture;
            var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

            camera.targetTexture = renderTexture;
            camera.Render();

            var prevRenderTexture = RenderTexture.active;

            RenderTexture.active = camera.targetTexture;

            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            texture.Apply(false);

            camera.targetTexture = prevCameraRenderTexture;
            RenderTexture.active = prevRenderTexture;

            return texture;
        }

        public static string SaveAsPNG(Texture2D texture, string filename, string directory)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            var formatter = new FileNameFormatter();

            filename = formatter.Format(filename);

            var assetPath = $"{directory}/{filename}.png";

            Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToPNG();

            File.WriteAllBytes(assetPath, bytes);

            return assetPath;
        }
    }
}
