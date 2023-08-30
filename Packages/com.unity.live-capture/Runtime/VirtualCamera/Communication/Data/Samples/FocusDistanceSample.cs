using System.Runtime.InteropServices;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FocusDistanceSampleV0
    {
        float m_Timestamp;
        float m_FocusDistance;

        public static explicit operator FocusDistanceSampleV0(FocusDistanceSample sample)
        {
            return new FocusDistanceSampleV0
            {
                m_Timestamp = (float)sample.Time,
                m_FocusDistance = sample.FocusDistance,
            };
        }

        public static explicit operator FocusDistanceSample(FocusDistanceSampleV0 sample)
        {
            return new FocusDistanceSample
            {
                Time = sample.m_Timestamp,
                FocusDistance = sample.m_FocusDistance,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FocusDistanceSampleV1
    {
        double m_Time;
        float m_FocusDistance;

        public static explicit operator FocusDistanceSampleV1(FocusDistanceSample sample)
        {
            return new FocusDistanceSampleV1
            {
                m_Time = sample.Time,
                m_FocusDistance = sample.FocusDistance,
            };
        }

        public static explicit operator FocusDistanceSample(FocusDistanceSampleV1 sample)
        {
            return new FocusDistanceSample
            {
                Time = sample.m_Time,
                FocusDistance = sample.m_FocusDistance,
            };
        }
    }
}
