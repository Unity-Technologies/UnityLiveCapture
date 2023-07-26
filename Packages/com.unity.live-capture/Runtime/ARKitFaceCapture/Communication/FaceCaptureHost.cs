using System;
using System.IO;
using Unity.LiveCapture.ARKitFaceCapture.Networking;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Protocols;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// A class used to communicate with a face capture device in the Unity editor from the companion app.
    /// </summary>
    class FaceCaptureHost : CompanionAppHost
    {
        readonly Action<FaceSample> m_SendPoseImpl;

        /// <inheritdoc />
        public FaceCaptureHost(NetworkBase network, Remote remote, Stream stream)
            : base(network, remote, stream)
        {
            if (BinarySender<FaceSampleV1>.TryGet(m_Protocol,
                FaceMessages.ToServer.FacePoseSample_V1,
                out var senderV1))
            {
                m_SendPoseImpl = sample => senderV1.Send((FaceSampleV1)sample);
            }
            else if (BinarySender<FaceSampleV0>.TryGet(m_Protocol,
                FaceMessages.ToServer.FacePoseSample_V0,
                out var senderV0))
            {
                // V1 message type not supported. Fall back to V0 (timecodes will get truncated)
                m_SendPoseImpl = sample => senderV0.Send((FaceSampleV0)sample);
            }
        }

        /// <summary>
        /// Sends a face pose sample to the server.
        /// </summary>
        /// <param name="sample">The pose sample.</param>
        public void SendPose(FaceSample sample)
        {
            m_SendPoseImpl?.Invoke(sample);
        }
    }
}
