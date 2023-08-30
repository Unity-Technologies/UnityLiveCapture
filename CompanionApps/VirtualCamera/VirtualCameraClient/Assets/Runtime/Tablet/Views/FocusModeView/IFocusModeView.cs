using System;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IFocusModeView : IDialogView, IFocusModeListener
    {
        event Action<FocusMode> FocusModeChanged;
    }
}
