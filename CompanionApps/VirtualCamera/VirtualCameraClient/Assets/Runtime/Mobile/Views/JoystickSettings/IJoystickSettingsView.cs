using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IJoystickSettingsView : IDialogView
    {
        event Action onDoneClicked;
    }
}
