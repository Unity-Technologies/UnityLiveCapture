using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IGamepadSettingsView : IDialogView
    {
        event Action onDoneClicked;
    }
}
