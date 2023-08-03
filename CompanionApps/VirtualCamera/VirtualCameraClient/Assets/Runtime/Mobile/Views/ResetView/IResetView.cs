using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IResetView : IDialogView
    {
        event Action onResetPoseClicked;
        event Action onResetLensClicked;
        event Action onRebaseToggled;

        void SetRebasing(bool value);
    }
}
