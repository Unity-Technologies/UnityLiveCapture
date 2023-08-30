using System.Runtime.InteropServices;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ApertureSampleV0
    {
        float m_Timestamp;
        float m_Aperture;

        public static explicit operator ApertureSampleV0(ApertureSample sample)
        {
            return new ApertureSampleV0
            {
                m_Timestamp = (float)sample.Time,
                m_Aperture = sample.Aperture,
            };
        }

        public static explicit operator ApertureSample(ApertureSampleV0 sample)
        {
            return new ApertureSample
            {
                Time = sample.m_Timestamp,
                Aperture = sample.m_Aperture,
            };
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ApertureSampleV1
    {
        double m_Time;
        float m_Aperture;

        public static explicit operator ApertureSampleV1(ApertureSample sample)
        {
            return new ApertureSampleV1
            {
                m_Time = sample.Time,
                m_Aperture = sample.Aperture,
            };
        }

        public static explicit operator ApertureSample(ApertureSampleV1 sample)
        {
            return new ApertureSample
            {
                Time = sample.m_Time,
                Aperture = sample.m_Aperture,
            };
        }
    }
}
