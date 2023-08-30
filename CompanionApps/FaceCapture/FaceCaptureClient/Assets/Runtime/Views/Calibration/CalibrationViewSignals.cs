using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    sealed class CalibrationViewSignals
    {
        public class PerformCalibration {}
        public class ConfirmCalibration : Signal<CalibrationPose> {}
        public class ClearCalibration {}
        public class CalibrationFailed {}
        public class CalibrationSucceeded {}
    }
}
