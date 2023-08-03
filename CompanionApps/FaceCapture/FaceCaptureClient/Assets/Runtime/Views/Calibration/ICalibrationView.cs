using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    interface ICalibrationView : IDialogView, AutoHideController.IHideable, IRecordingStateListener
    {
        enum State
        {
            MenuClosed,
            MenuOpen,
            Calibrating,
            Reviewing
        }

        event Action<State> StateChanged;

        event Action CalibrateButtonClicked;
        event Action RecordButtonClicked;
        event Action CalibrateMenuButtonClicked;
        event Action ClearMenuButtonClicked;
        event Action CancelButtonClicked;
        event Action DoneButtonClicked;

        State ViewState { get; set; }
        bool IsCalibrated { set; }
    }
}
