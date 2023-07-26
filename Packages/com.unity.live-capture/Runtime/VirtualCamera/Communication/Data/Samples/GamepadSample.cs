using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GamepadSampleV0
    {
        double m_Time;
        Vector3 m_Move;
        Vector3 m_Look;

        public static explicit operator GamepadSampleV0(GamepadSample sample)
        {
            return new GamepadSampleV0
            {
                m_Time = sample.Time,
                m_Move = sample.Move,
                m_Look = sample.Look,
            };
        }

        public static explicit operator GamepadSample(GamepadSampleV0 sample)
        {
            return new GamepadSample
            {
                Time = sample.m_Time,
                Move = sample.m_Move,
                Look = sample.m_Look,
            };
        }
    }
}
