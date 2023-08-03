using System;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IGamepadSlidersTabView
    {
        void SetSensitivity(Gamepad.AxisID axis, float sensitivity);

        event Action<Gamepad.AxisID, float> onSensitivityChanged;
    }
}
