using System;

namespace Unity.CompanionAppCommon
{
    interface IWizardView : IDialogView
    {
        event Action SkipClicked;
        event Action NextClicked;
    }
}
