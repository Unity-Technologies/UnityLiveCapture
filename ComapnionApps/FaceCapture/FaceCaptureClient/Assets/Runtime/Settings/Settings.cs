namespace Unity.CompanionApps.FaceCapture
{
    class Settings : ISettings
    {
        public bool FlipHorizontally { get; set; }
        public bool RecordVideo { get; set; }
        public bool RecordAudio { get; set; }
        public string TimecodeSourceId { get; set; }
        public bool FaceWireframe { get; set; }
        public bool CountdownEnabled { get; set; }
        public int CountdownTime { get; set; }
        public bool ShowRecordButton { get; set; }
        public bool DimScreen { get; set; }
        public bool CameraPassthrough { get; set; }
        public bool AutoHideUI { get; set; }
        public CalibrationPose? CalibrationPose { get; set; }
    }
}
