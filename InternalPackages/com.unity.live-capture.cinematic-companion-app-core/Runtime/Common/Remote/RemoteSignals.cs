using Unity.LiveCapture.CompanionApp;

namespace Unity.CompanionAppCommon
{
    class RemoteDisconnectedSignal {}

    class RemoteConnectedSignal
    {
        public CompanionAppHost companionAppHost;
        public string ip;
        public int port;
    }

    class RemoteConnectionFailedSignal {}
}
