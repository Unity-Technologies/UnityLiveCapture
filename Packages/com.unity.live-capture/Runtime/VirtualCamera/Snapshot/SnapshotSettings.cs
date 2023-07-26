using Unity.LiveCapture.VideoStreaming.Server;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [SettingFilePath("UserSettings/LiveCapture/SnapshotSettings.asset", SettingFilePathAttribute.Location.ProjectFolder)]
    class SnapshotSettings : SettingAsset<SnapshotSettings>
    {
        static readonly string k_DefaultScreenshotDirectory = "Assets/Screenshots";

        [SerializeField]
        string m_ScreenshotDirectory = k_DefaultScreenshotDirectory;

        public string ScreenshotDirectory => m_ScreenshotDirectory;

        /// <summary>
        /// Resets the settings to the default values.
        /// </summary>
        public void Reset()
        {
            m_ScreenshotDirectory = k_DefaultScreenshotDirectory;
        }
    }
}
