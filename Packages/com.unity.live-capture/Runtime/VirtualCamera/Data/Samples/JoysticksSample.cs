using System;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing a joystick position sample.
    /// </summary>
    struct JoysticksSample : ISample
    {
        double m_Time;

        /// <summary>
        /// The direction of the joysticks.
        /// </summary>
        public Vector3 Joysticks;

        /// <inheritdoc/>
        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }
    }
}
