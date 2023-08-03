using UnityEngine;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    class VideoStreamChangedSignal : Signal<VideoStreamState>
    {
    }

    class RigSettingsViewSignals
    {
        public class Close {}
    }

    class JoystickSettingsViewSignals
    {
        public class Close {}
    }

    class GamepadSettingsViewSignals
    {
        public class Close {}
    }

    class ConnectionScreenViewSignals
    {
        public class Close {}
    }

    class LensSettingsViewSignals
    {
        public class Close {}
    }

    class SettingsViewSignals
    {
        public class Close {}
    }

    class HelpModeSignals : BaseHelpModeSignals<HelpTooltipId>{}

    class ResetLensSignal
    {
    }

    class StartServerDiscoverySignal
    {
    }

    class SetToCurrentTiltSignal
    {
    }

    class SetJoystickLateralSignal : Signal<float>
    {
    }

    class SetJoystickVerticalSignal : Signal<float>
    {
    }

    class SetJoystickForwardSignal : Signal<float>
    {
    }

    class SetGamepadLateralSignal : Signal<float>
    {
    }

    class SetGamepadVerticalSignal : Signal<float>
    {
    }

    class SetGamepadForwardSignal : Signal<float>
    {
    }

    class SetGamepadTiltSignal : Signal<float>
    {
    }

    class SetGamepadPanSignal : Signal<float>
    {
    }

    class SetGamepadRollSignal : Signal<float>
    {
    }

    class SetMainViewOptionSignal : Signal<(MainViewOptions, bool)>
    {
    }

    class SetRecordingCountdownEnabledSignal : Signal<bool>
    {
    }

    class SetRecordingCountdownDurationSignal : Signal<int>
    {
    }

    class SetTimecodeSourceSignal
    {
        public string Id;
    }

    class ShowTakeLibrarySignal : Signal<bool> {}

    class SendChannelFlagsSignal : Signal<VirtualCameraChannelFlags> {}

    class ShowTakeIterationSignal : Signal<bool> {}
    class ShowTakeIterationOrHelpSignal : Signal<bool> {}

    class RequestStartRecordingSignal {}
    class RequestStopRecordingSignal {}

    class TakeDescriptorsChangedSignal {}
    class TakeMetadataDescriptorsChangedSignal {}

    class TogglePlaybackSignal {}

    class SkipFramesSignal : Signal<int> {}
}
