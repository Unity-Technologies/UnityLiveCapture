using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera
{
    interface IScreenshotImpl
    {
        Texture2D Take(Camera camera, int sceneNumber, string shotName, double time, FrameRate frameRate);
    }

    class ScreenshotImpl : IScreenshotImpl
    {
        public Texture2D Take(Camera camera, int sceneNumber, string shotName, double time, FrameRate frameRate)
        {
#if UNITY_EDITOR
            if (camera == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(shotName))
            {
                shotName = "Unknown";
            }

            var timecode = Timecode.FromSeconds(frameRate, time);
            var directory = SnapshotSettings.Instance.ScreenshotDirectory;
            var filename = $"[{sceneNumber.ToString("D3")}] {shotName} [{timecode}]";
            var texture = Screenshot.Take(camera);
            var assetPath = Screenshot.SaveAsPNG(texture, filename, directory);

            DestroyTexture(texture);

            AssetDatabase.ImportAsset(assetPath);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#else
            return null;
#endif
        }

        void DestroyTexture(Texture2D texture)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
