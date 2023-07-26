using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    // We bundle Take related commands.
    // TODO See if bundling by "area" is the best approach.
    class TakesCommand
    {
        [Inject]
        IRemoteSystem m_Remote;

        public void SendChannelFlags(SendChannelFlagsSignal signal)
        {
            var host = m_Remote.Host as VirtualCameraHost;

            if (host != null)
            {
                host.SendChannelFlags(signal.value);
            }
        }
    }
}
