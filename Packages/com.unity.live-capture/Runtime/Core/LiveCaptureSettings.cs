using UnityEngine;

namespace Unity.LiveCapture
{
    [SettingFilePath("ProjectSettings/LiveCaptureSettings.asset", SettingFilePathAttribute.Location.ProjectFolder)]
    class LiveCaptureSettings : SettingAsset<LiveCaptureSettings>
    {
        static readonly string k_DefaultTakeNameFormat = $"[{TakeNameFormatter.Wildcards.Scene}] {TakeNameFormatter.Wildcards.Shot} [{TakeNameFormatter.Wildcards.Take}]";
        static readonly string k_DefaultAssetNameFormat = $"{TakeNameFormatter.Wildcards.Name} [{TakeNameFormatter.Wildcards.Shot}] [{TakeNameFormatter.Wildcards.Take}]";

        [SerializeField, OnlyStandardFrameRates]
        FrameRate m_FrameRate = StandardFrameRate.FPS_24_00;
        [SerializeField]
        string m_TakeNameFormat = k_DefaultTakeNameFormat;
        [SerializeField]
        string m_AssetNameFormat = k_DefaultAssetNameFormat;
        [SerializeReference]
        ISyncProvider m_SyncProvider = null;

        /// <summary>
        /// The frame rate to use for recording.
        /// </summary>
        public FrameRate FrameRate
        {
            get => m_FrameRate;
            set => m_FrameRate = value;
        }

        public string TakeNameFormat => string.IsNullOrWhiteSpace(m_TakeNameFormat) ? k_DefaultTakeNameFormat : m_TakeNameFormat;
        public string AssetNameFormat => string.IsNullOrWhiteSpace(m_AssetNameFormat) ? k_DefaultAssetNameFormat : m_AssetNameFormat;

        internal ISyncProvider SyncProvider => m_SyncProvider;

        /// <summary>
        /// Resets the settings to the default values.
        /// </summary>
        public void Reset()
        {
            m_TakeNameFormat = k_DefaultTakeNameFormat;
            m_AssetNameFormat = k_DefaultAssetNameFormat;
            m_SyncProvider = null;
        }
    }
}
