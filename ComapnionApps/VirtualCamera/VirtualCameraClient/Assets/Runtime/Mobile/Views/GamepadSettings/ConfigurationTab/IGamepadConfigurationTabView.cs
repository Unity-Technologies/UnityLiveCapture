using System;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IGamepadConfigurationTabView
    {
        void SetDeviceName(string name);
        void SetPedestalSpace(Space space);
        void SetMotionSpace(Space space);

        event Action<Space> onPedestalSpaceChanged;
        event Action<Space> onMotionSpaceChanged;
        event Action onViewLayoutPressed;
    }
}
