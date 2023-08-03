using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface IMainViewMobile : IMainView,
        IHelpModeListener,
        IDeviceModeListener,
        IChannelFlagsListener
    {
        event Action LensSettingsClicked;
        event Action RigSettingsClicked;
        event Action JoystickSettingsClicked;
        event Action SettingsClicked;
        event Action<bool> DeviceModeToggled;
        event Action<bool> HelpToggled;
        event Action<bool> ResetMenuToggled;
        event Action<bool> TakeIterationToggled;

        void SetLensViewOpenState(bool isOpen);
        void SetMenuTogglesOffWithoutNotify();
        void SetGamepadState(bool connected);
    }
}
