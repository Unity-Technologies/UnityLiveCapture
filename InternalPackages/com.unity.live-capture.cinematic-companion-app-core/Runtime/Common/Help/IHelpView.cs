using System;
using UnityEngine;

namespace Unity.CompanionAppCommon
{
    interface IHelpView : IDialogView
    {
        event Action DocumentationClicked;
        event Action SupportClicked;

        TextAnchor Alignment { get; set; }
    }
}
