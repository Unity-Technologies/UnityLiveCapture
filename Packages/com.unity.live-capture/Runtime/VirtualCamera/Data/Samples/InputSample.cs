using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing client input data and a timestamp.
    /// </summary>
    struct InputSample
    {
        /// <summary>
        /// The time of the sample expressed in seconds.
        /// </summary>
        /// <remarks>
        /// In the absence of an external timecode source, this value presents the time in seconds since the client connected.
        /// This can be used to determine the order and timing of this sample relative to other samples received.
        /// </remarks>
        public double Time;

        /// <summary>
        /// The AR pose.
        /// </summary>
        public Pose ARPose;

        /// <summary>
        /// The direction of the joysticks.
        /// </summary>
        public Vector3 VirtualJoysticks;

        /// <summary>
        /// The actuation of movement (X = Truck, Y = Pedestal, Z = Dolly).
        /// </summary>
        public Vector3 GamepadMove;

        /// <summary>
        /// The actuation of rotation in degrees (X = Tilt up, Y = Pan right, Z = Roll clockwise).
        /// </summary>
        public Vector3 GamepadLook;

    }
}
