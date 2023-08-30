using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    interface ILaunchScreenView : IDialogView
    {
        event Action SystemPermissionsClicked;
        event Action DocumentationClicked;
        event Action SupportClicked;
        event Action RestartWizardClicked;
    }
}
