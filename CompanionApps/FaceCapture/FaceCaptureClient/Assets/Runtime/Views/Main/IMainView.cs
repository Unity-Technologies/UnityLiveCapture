using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    interface IMainView : IDialogView, AutoHideController.IHideable, IRecordingStateListener
    {
        event Action<bool> VisibilityChanged;
        event Action SettingsButtonClicked;
        event Action TakesButtonClicked;
        event Action TrackingButtonClicked;
        event Action RecordButtonClicked;
        event Action<bool> HelpToggleChanged;

        float RecordButtonOpacity { set; }
        bool RecordButtonActive { set; }

        void SetTrackingState(bool value);
    }
}
