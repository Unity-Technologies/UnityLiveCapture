using System;
using Unity.LiveCapture.ARKitFaceCapture;

namespace Unity.CompanionApps.FaceCapture
{
    [Serializable]
    struct CalibrationPose
    {
        public FacePose Pose;
    }
}
