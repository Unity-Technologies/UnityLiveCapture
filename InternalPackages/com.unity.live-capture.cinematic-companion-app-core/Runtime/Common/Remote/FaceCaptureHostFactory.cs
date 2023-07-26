using System.IO;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.ARKitFaceCapture;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    class FaceCaptureHostFactory : ICompanionAppHostFactory
    {
        public CompanionAppHost CreateHost(NetworkBase network, Remote remote, Stream stream)
        {
            return new FaceCaptureHost(network, remote, stream);
        }
    }
}
