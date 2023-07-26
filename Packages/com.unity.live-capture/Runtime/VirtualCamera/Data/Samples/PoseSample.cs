using System;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A struct containing a pose sample.
    /// </summary>
    struct PoseSample : ISample
    {
        double m_Time;

        /// <summary>
        /// The transform pose.
        /// </summary>
        public Pose Pose;

        /// <inheritdoc/>
        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }
    }
}
