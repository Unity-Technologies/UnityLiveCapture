using System;
using Unity.LiveCapture.CompanionApp;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing a focus distance sample.
    /// </summary>
    struct FocusDistanceSample : ISample
    {
        double m_Time;

        /// <summary>
        /// The focus distance in meters.
        /// </summary>
        public float FocusDistance;

        /// <inheritdoc/>
        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }
    }
}
