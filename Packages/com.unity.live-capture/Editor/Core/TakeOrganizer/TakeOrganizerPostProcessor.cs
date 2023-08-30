using UnityEditor;
namespace Unity.LiveCapture.Editor
{
    class TakeOrganizerPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (TakeOrganizerWindow.Instance != null)
            {
                TakeOrganizerWindow.Instance.SetNeedsReload();
            }
        }
    }
}
