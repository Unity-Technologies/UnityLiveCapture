using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct JoysticksSampleV0
    {
        float m_Timestamp;
        Vector3 m_Joysticks;

        public static explicit operator JoysticksSampleV0(JoysticksSample sample)
        {
            return new JoysticksSampleV0
            {
                m_Timestamp = (float)sample.Time,
                m_Joysticks = sample.Joysticks,
            };
        }

        public static explicit operator JoysticksSample(JoysticksSampleV0 sample)
        {
            return new JoysticksSample
            {
                Time = sample.m_Timestamp,
                Joysticks = sample.m_Joysticks,
            };
        }
    }
}
