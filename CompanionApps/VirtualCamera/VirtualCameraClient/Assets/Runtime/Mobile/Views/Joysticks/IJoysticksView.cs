using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IJoysticksView : IPresentable
    {
        event Action<float> onForwardAxisChanged;
        event Action<float> onLateralAxisChanged;
        event Action<float> onVerticalAxisChanged;

        void SetGamepadIsValid(bool isValid);
    }
}
