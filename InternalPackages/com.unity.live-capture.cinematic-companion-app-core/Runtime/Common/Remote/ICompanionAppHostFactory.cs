using System.IO;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.CompanionApp;

namespace Unity.CompanionAppCommon
{
    interface ICompanionAppHostFactory
    {
        CompanionAppHost CreateHost(NetworkBase network, Remote remote, Stream stream);
    }
}
