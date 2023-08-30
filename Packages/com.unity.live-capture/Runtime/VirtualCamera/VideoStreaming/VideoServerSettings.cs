using Unity.LiveCapture.VideoStreaming.Server;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    [SettingFilePath("UserSettings/LiveCapture/VideoServerSettings.asset", SettingFilePathAttribute.Location.ProjectFolder)]
    [HelpURL(Documentation.baseURL + "ref-user-preferences-video-server" + Documentation.endURL)]
    class VideoServerSettings : SettingAsset<VideoServerSettings>
    {
        const float k_MinResolutionScale = 0.1f;
        const float k_MaxQuality = 100f;
        const int k_MaxFrameRate = 60;

        [SerializeField, Tooltip("The preferred encoder to use for video streaming.")]
        VideoEncoder m_Encoder;
        [SerializeField, Tooltip("The resolution of the video stream relative to the source camera. Lower resolutions have better latency and performance, but have lower visual quality.")]
        [Range(k_MinResolutionScale, 1f)]
        float m_ResolutionScale;
        [SerializeField, Tooltip("The number of frames per second in the video stream.")]
        [Range(1f, k_MaxFrameRate)]
        int m_FrameRate;
        [SerializeField, Tooltip("The quality of encoded video given as a percentage of the maximum supported quality. A higher value will use a higher bit-rate.")]
        [Range(1, k_MaxQuality)]
        float m_Quality;
        [SerializeField, Tooltip("Attempt to minimize the latency of retrieving rendered frames from the GPU at the cost of performance. If you encounter stuttering in the editor while enabled, try reducing the resolution.")]
        bool m_PrioritizeLatency;

        /// <summary>
        /// The preferred encoder to use for video streaming.
        /// </summary>
        public VideoEncoder Encoder => m_Encoder;

        /// <summary>
        /// The resolution of the video stream relative to the source camera.
        /// </summary>
        /// <remarks>
        /// Lower resolutions have better latency and performance, but have lower visual quality.
        /// </remarks>
        public float ResolutionScale => m_ResolutionScale;

        /// <summary>
        /// The number of frames per second in the video stream.
        /// </summary>
        public int FrameRate => m_FrameRate;

        /// <summary>
        /// The quality of encoded video given as a percentage of the maximum supported quality. A higher value will use a higher bit-rate.
        /// </summary>
        public float Quality => m_Quality;

        /// <summary>
        /// Attempt to minimize the latency of retrieving rendered frames from the GPU at the cost of performance.
        /// </summary>
        public bool PrioritizeLatency => m_PrioritizeLatency;

        void OnValidate()
        {
            if (EncoderUtilities.IsSupported(m_Encoder) == EncoderSupport.NotSupportedOnPlatform)
            {
                m_Encoder = EncoderUtilities.DefaultSupportedEncoder();
            }

            m_ResolutionScale = Mathf.Clamp(m_ResolutionScale, k_MinResolutionScale, 1f);
            m_FrameRate = Mathf.Clamp(m_FrameRate, 1, k_MaxFrameRate);
            m_Quality = Mathf.Clamp(m_Quality, 1f, k_MaxQuality);
        }

        /// <summary>
        /// Resets the settings to the default values.
        /// </summary>
        public void Reset()
        {
            m_Encoder = EncoderUtilities.DefaultSupportedEncoder();
            m_ResolutionScale = 1f;
            m_FrameRate = 60;
            m_Quality = 50f;
            m_PrioritizeLatency = true;
        }
    }
}
