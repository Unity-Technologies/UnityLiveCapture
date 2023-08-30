namespace Unity.CompanionAppCommonEditor
{
    internal interface IAssetAccessProxy
    {
        bool IsValidFolder(string folderPath);
        void CreateFolder(string parentFolder, string folderName);
        void CreateAsset(UnityEngine.Object assetToCreate, string path);
        void SaveAssets();
    }
}
