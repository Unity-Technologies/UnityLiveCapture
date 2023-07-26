using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IGamepadLayoutView : IDialogView
    {
        event Action onCloseClicked;
    }
}
