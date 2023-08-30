using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IRigSettingsView : IDialogView
    {
        event Action onRotationViewShowed;
        event Action onPositionViewShowed;
        event Action onDampingViewShowed;
        event Action onDoneClicked;
    }
}
