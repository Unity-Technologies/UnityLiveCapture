using System.IO;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class VirtualCameraHostFactory : ICompanionAppHostFactory
    {
        public CompanionAppHost CreateHost(NetworkBase network, Remote remote, Stream stream)
        {
            return new VirtualCameraHost(network, remote, stream);
        }
    }
}
