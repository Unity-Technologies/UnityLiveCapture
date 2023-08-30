using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Unity.CompanionAppCommonEditor
{
    internal class AssetAccessProxy : IAssetAccessProxy
    {
        public bool IsValidFolder(string folderPath)
        {
            return AssetDatabase.IsValidFolder(folderPath);
        }

        public void CreateFolder(string parentFolder, string folderName)
        {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        public void CreateAsset(UnityEngine.Object assetToCreate, string path)
        {
            AssetDatabase.CreateAsset(assetToCreate, path);
        }

        public void SaveAssets()
        {
            AssetDatabase.SaveAssets();
        }
    }
}
