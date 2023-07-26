using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Interface for saving/loading gamepad user settings.
    /// </summary>
    interface IGamepadSerializer
    {
        void SaveSensitivities(AxisSensitivities sensitivities);
        void SaveBindingOverrides(InputActionMap actions);

        bool TryLoadSensitivities(out AxisSensitivities sensitivities);
        bool TryLoadBindingOverrides(InputActionMap actions);
    }
}
