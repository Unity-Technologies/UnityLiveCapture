using Unity.LiveCapture.ARKitFaceCapture;

namespace Unity.CompanionApps.FaceCapture
{
    interface IFaceTracker
    {
        bool IsTracking { get; }
        public bool HasCalibrationPose { get; }
        public FacePose CalibrationPose { get; }
        void SetCalibrationPose(FacePose pose);
        void ClearCalibrationPose();
        bool ConsumeFacePose(out FacePose facePose, bool applyCalibration = true);
    }
}
