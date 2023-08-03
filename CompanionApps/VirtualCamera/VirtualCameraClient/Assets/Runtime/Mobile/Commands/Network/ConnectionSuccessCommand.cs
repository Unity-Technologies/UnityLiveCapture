using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ConnectionSuccessCommand
    {
        [Inject]
        DeviceDataSystem m_DeviceDataSystem;
        [Inject]
        IRemoteSystem m_Remote;

        public void Execute(RemoteConnectedSignal signal)
        {
            if (m_Remote.Host != null)
                m_Remote.Host.SetServerMode(LiveCapture.CompanionApp.DeviceMode.LiveStream);
        }
    }
}
