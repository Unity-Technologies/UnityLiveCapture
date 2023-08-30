using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class DisconnectedCommand
    {
        [Inject]
        VideoStreamingSystem m_VideoStreamingSystem;

        public void Execute(RemoteDisconnectedSignal disconnectedSignal)
        {
            m_VideoStreamingSystem.StopVideoStream();
        }
    }
}
