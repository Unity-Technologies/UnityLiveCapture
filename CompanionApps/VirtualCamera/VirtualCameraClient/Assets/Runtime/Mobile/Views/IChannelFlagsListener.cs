using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IChannelFlagsListener
    {
        void SetChannelFlags(VirtualCameraChannelFlags channelFlags);
    }
}
