using System;
using Unity.LiveCapture.CompanionApp;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    struct FaceSample : ISample
    {
        double m_Time;

        public FacePose FacePose;

        /// <inheritdoc/>
        public double Time
        {
            get => m_Time;
            set => m_Time = value;
        }
    }
}
