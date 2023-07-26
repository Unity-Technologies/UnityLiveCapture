using System;

namespace Unity.LiveCapture.VideoStreaming.Client.Rtcp
{
    interface IRtcpSenderStatisticsProvider
    {
        DateTime LastTimeReportReceived { get; }
        long LastNtpTimeReportReceived { get; }
    }
}
