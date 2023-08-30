using System;
using Unity.LiveCapture.CompanionApp;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing a focal length sample.
    /// </summary>
    struct FocalLengthSample : ISample
    {
        double m_Time;

        /// <summary>
        /// The focal length in millimeters.
        /// </summary>
        public float FocalLength;

        /// <inheritdoc/>
        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }
    }
}
