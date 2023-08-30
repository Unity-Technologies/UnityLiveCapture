using System.Runtime.InteropServices;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FocalLengthSampleV0
    {
        float m_Timestamp;
        float m_FocalLength;

        public static explicit operator FocalLengthSampleV0(FocalLengthSample sample)
        {
            return new FocalLengthSampleV0
            {
                m_Timestamp = (float)sample.Time,
                m_FocalLength = sample.FocalLength,
            };
        }

        public static explicit operator FocalLengthSample(FocalLengthSampleV0 sample)
        {
            return new FocalLengthSample
            {
                Time = sample.m_Timestamp,
                FocalLength = sample.m_FocalLength,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FocalLengthSampleV1
    {
        double m_Time;
        float m_FocalLength;

        public static explicit operator FocalLengthSampleV1(FocalLengthSample sample)
        {
            return new FocalLengthSampleV1
            {
                m_Time = sample.Time,
                m_FocalLength = sample.FocalLength,
            };
        }

        public static explicit operator FocalLengthSample(FocalLengthSampleV1 sample)
        {
            return new FocalLengthSample
            {
                Time = sample.m_Time,
                FocalLength = sample.m_FocalLength,
            };
        }
    }
}
