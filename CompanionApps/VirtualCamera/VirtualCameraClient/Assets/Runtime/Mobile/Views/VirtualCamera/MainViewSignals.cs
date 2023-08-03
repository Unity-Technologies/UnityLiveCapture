using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class MainViewSignals
    {
        public class OpenRigSettingsView {}
        public class OpenJoystickSettingsView {}
        public class OpenLensSettingsView {}
        public class OpenSettingsView {}
        public class ToggleHelp : Signal<bool> {}
        public class ToggleRecording {}
        public class ToggleResetView : Signal<bool> {}
        public class ToggleDeviceMode {}
        public class ResetViewOptionSelected {}
    }
}
