using System;
using Unity.LiveCapture.ARKitFaceCapture.Networking;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Protocols;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// An interface used to communicate with the face capture companion app.
    /// </summary>
    public interface IFaceClient : ICompanionAppClient
    {
    }

    /// <inheritdoc cref="IFaceClient"/>
    interface IFaceClientInternal : IFaceClient, ICompanionAppClientInternal
    {
        /// <summary>
        /// An event invoked when a face pose sample is received.
        /// </summary>
        event Action<FaceSample> FacePoseSampleReceived;
    }

    /// <summary>
    /// A class used to communicate with the face capture companion app.
    /// </summary>
    [Preserve]
    [Client(k_ClientType)]
    class FaceClient : CompanionAppClient, IFaceClientInternal
    {
        /// <summary>
        /// The type of client this device supports.
        /// </summary>
        const string k_ClientType = "ARKit Face Capture";

        /// <inheritdoc />
        public event Action<FaceSample> FacePoseSampleReceived;

        /// <inheritdoc />
        public FaceClient(NetworkBase network, Remote remote, ClientInitialization data)
            : base(network, remote, data)
        {
            m_Protocol.Add(new BinaryReceiver<FaceSampleV1>(FaceMessages.ToServer.FacePoseSample_V1,
                ChannelType.UnreliableUnordered)).AddHandler(pose =>
                {
                    FacePoseSampleReceived?.Invoke((FaceSample)pose);
                });
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => k_ClientType;
    }
}
