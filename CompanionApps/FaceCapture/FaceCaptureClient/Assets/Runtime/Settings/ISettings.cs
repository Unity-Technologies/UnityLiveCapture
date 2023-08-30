namespace Unity.CompanionApps.FaceCapture
{
    interface ISettings
    {
        bool FlipHorizontally { get; }
        bool RecordVideo { get; }
        bool RecordAudio { get; }
        string TimecodeSourceId { get; }
        bool FaceWireframe { get; }
        bool CountdownEnabled { get; }
        int CountdownTime { get; }
        bool ShowRecordButton { get; }
        bool DimScreen { get; }
        bool CameraPassthrough { get; }
        bool AutoHideUI { get; }
        CalibrationPose? CalibrationPose { get; }
    }
}
