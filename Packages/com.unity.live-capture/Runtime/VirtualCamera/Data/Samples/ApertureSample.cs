using System;
using Unity.LiveCapture.CompanionApp;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing an aperture sample.
    /// </summary>
    struct ApertureSample : ISample
    {
        double m_Time;

        /// <summary>
        /// The aperture in millimeters.
        /// </summary>
        public float Aperture;

        /// <inheritdoc/>
        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }
    }
}
