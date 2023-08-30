using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IConnectionScreenView : IDialogView
    {
        event Action<bool> OnShow;
        event Action SystemPermissionsClicked;
        event Action DocumentationClicked;
        event Action SupportClicked;
        event Action RestartWizardClicked;

        DialogDoneView DialogDone { get; }
    }
}
