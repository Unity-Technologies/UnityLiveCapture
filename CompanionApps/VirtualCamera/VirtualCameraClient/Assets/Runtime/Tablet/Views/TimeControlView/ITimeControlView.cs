using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ITimeControlView : IDialogView,
        ITimeListener,
        IPlayStateListener
    {
        event Action<float> ValueChanged;
        event Action PlayButtonClicked;
        event Action<int> FrameSkipButtonClicked;
    }
}
