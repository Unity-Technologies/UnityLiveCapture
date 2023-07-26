using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct InputSampleV0
    {
        public double Time;

        public Pose ARPose;

        public Vector3 VirtualJoysticks;

        public Vector3 GamepadMove;

        public Vector3 GamepadLook;

        public static explicit operator InputSampleV0(InputSample sample)
        {
            return new InputSampleV0
            {
                Time = sample.Time,
                ARPose = sample.ARPose,
                VirtualJoysticks = sample.VirtualJoysticks,
                GamepadMove = sample.GamepadMove,
                GamepadLook = sample.GamepadLook,
            };
        }

        public static explicit operator InputSample(InputSampleV0 sample)
        {
            return new InputSample
            {
                Time = sample.Time,
                ARPose = sample.ARPose,
                VirtualJoysticks = sample.VirtualJoysticks,
                GamepadMove = sample.GamepadMove,
                GamepadLook = sample.GamepadLook,
            };
        }
    }
}
