using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    class StateModel // TODO more specific / less redundant name
    {
        readonly Platform m_Platform;

        public StateModel(Platform platform)
        {
            m_Platform = platform;
        }

        public Platform Platform => m_Platform;
        public bool IsHelpMode { get; set; }
        // TODO proper place to store this field?
        public VirtualCameraChannelFlags ChannelFlags { get; set; }
    }
}
