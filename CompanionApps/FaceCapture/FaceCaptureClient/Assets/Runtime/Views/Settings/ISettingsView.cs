using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    interface ISettingsView : IDialogView, ISettingsPropertyListener
    {
        event Action DoneButtonClicked;
        event Action ResetButtonClicked;
        event Action PrivacyPolicyClicked;
        event Action DocumentationClicked;
        event Action SupportClicked;
        event Action<bool> AutoHideUIChanged;
        event Action<bool> CameraPassthroughChanged;
        event Action<bool> DimScreenChanged;
        event Action<bool> FlipHorizontallyChanged;
        event Action<bool> RecordAudioChanged;
        event Action<bool> RecordVideoChanged;
        event Action<bool> RecordingCountdownChanged;
        event Action<bool> ShowFaceChanged;
        event Action<bool> ShowRecordButtonChanged;
        event Action<int> RecordingCountdownValueChanged;
        event Action<string> TimecodeSourceChanged;
    }
}
