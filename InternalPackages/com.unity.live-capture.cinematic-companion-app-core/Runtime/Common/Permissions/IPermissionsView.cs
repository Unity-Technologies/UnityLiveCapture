using System;

namespace Unity.CompanionAppCommon
{
    interface IPermissionsView : IDialogView
    {
        event Action OpenSettingsClicked;
        event Action CancelClicked;

        void Show(string message, bool cancelable);
    }
}
