using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.LiveCapture.VirtualCamera
{
    static class SnapshotLibraryUtility
    {
        internal static SnapshotLibrary CreateSnapshotLibrary(string path)
        {
            var asset = ScriptableObject.CreateInstance<SnapshotLibrary>();
#if UNITY_EDITOR
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.Refresh();
            }
#endif
            return asset;
        }

        internal static void AddSnapshot(SnapshotLibrary snapshotLibrary, Snapshot snapshot)
        {
            snapshotLibrary.Add(snapshot);
#if UNITY_EDITOR
            EditorUtility.SetDirty(snapshotLibrary);
            AssetDatabase.SaveAssets();
#endif
        }

        internal static string GetSnapshotLibraryDefaultName(VirtualCameraDevice device)
        {
            return $"{device.name} SnapshotLibrary";
        }

        internal static void EnforceSnapshotLibrary(VirtualCameraDevice device)
        {
            if (device.SnapshotLibrary == null)
            {
                var path = $"Assets/{GetSnapshotLibraryDefaultName(device)}.asset";

                device.SnapshotLibrary = CreateSnapshotLibrary(path);
#if UNITY_EDITOR
                EditorUtility.SetDirty(device);
#endif
            }
        }

        internal static void MigrateSnapshotsToSnapshotLibrary(VirtualCameraDevice device)
        {
            SnapshotLibraryUtility.EnforceSnapshotLibrary(device);

            device.SnapshotLibrary.Set(device.m_Snapshots);
            device.m_Snapshots.Clear();

#if UNITY_EDITOR
            EditorUtility.SetDirty(device);
#endif
        }
    }
}
