using Unity.LiveCapture.VideoStreaming.Client.Utils;

namespace Unity.LiveCapture.VideoStreaming.Client.Rtp
{
    internal interface IRtpSequenceAssembler
    {
        RefAction<RtpPacket> PacketPassed { get; set; }

        void ProcessPacket(ref RtpPacket rtpPacket);
    }
}
