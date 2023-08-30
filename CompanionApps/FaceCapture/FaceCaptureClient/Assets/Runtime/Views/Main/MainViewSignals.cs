namespace Unity.CompanionApps.FaceCapture
{
    sealed class MainViewSignals
    {
        public class OpenSettings {}
        public class ToggleHelp
        {
            public bool IsActive { get; set; }
        }
        public class ToggleRecording {}
        public class TrackingClicked {}
        public class TakesClicked {}
    }
}
