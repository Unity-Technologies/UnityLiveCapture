using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PoseSampleV0
    {
        float m_Timestamp;
        Pose m_Pose;

        public static explicit operator PoseSampleV0(PoseSample sample)
        {
            return new PoseSampleV0
            {
                m_Timestamp = (float)sample.Time,
                m_Pose = sample.Pose,
            };
        }

        public static explicit operator PoseSample(PoseSampleV0 sample)
        {
            return new PoseSample
            {
                Time = sample.m_Timestamp,
                Pose = sample.m_Pose,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PoseSampleV1
    {
        double m_Time;
        Pose m_Pose;

        public static explicit operator PoseSampleV1(PoseSample sample)
        {
            return new PoseSampleV1
            {
                m_Time = sample.Time,
                m_Pose = sample.Pose,
            };
        }

        public static explicit operator PoseSample(PoseSampleV1 sample)
        {
            return new PoseSample
            {
                Time = sample.m_Time,
                Pose = sample.m_Pose,
            };
        }
    }
}
