using System;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ITakeIterationView : IDialogView, IChannelFlagsListener
    {
        event Action<VirtualCameraChannelFlags> onChannelFlagsChanged;
        event Action onClearIterationBase;
        event Action onSetIterationBase;
        event Action onCloseClicked;

        bool CanClearIterationBase { set; }
        string IterationBaseName { set; }
    }
}
