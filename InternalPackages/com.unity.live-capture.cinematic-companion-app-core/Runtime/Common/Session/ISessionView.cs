using System;

namespace Unity.CompanionAppCommon
{
    interface ISessionView : IDialogView
    {
        event Action DocumentationClicked;

        void SetMessage(string value);
    }
}
