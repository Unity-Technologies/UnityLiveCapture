using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    interface IBackgroundEventsView : IDialogView
    {
        event Action<int, int> OnClick;
        event Action<int> OnTouch;
    }
}
