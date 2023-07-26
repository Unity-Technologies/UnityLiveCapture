using System;

namespace Unity.CompanionAppCommon
{
    interface IWizardPermissionsView : IWizardView
    {
        event Action OpenPermissionsSettings;
    }
}
