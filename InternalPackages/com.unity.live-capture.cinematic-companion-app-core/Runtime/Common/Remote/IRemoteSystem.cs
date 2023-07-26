using Unity.LiveCapture.CompanionApp;

namespace Unity.CompanionAppCommon
{
    interface IRemoteSystem
    {
        CompanionAppHost Host { get; }

        void Connect(string ip, int port, string clientType, int connectAttemptTimeout = 2000);
        void Disconnect();
    }
}
