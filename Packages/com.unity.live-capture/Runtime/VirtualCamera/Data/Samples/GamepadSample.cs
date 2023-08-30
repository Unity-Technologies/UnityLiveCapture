using System;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing a gamepad sample.
    /// </summary>
    struct GamepadSample : ISample
    {
        /// <inheritdoc/>
        public double Time { get; set; }

        /// <summary>
        /// The actuation of movement (X = Truck, Y = Pedestal, Z = Dolly).
        /// </summary>
        public Vector3 Move;

        /// <summary>
        /// The actuation of rotation in degrees (X = Tilt up, Y = Pan right, Z = Roll clockwise).
        /// </summary>
        public Vector3 Look;
    }
}
