using System;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Provides a <see cref="PlayerInput"/> instance, which contains
    /// input action definitions and which generates input events.
    /// Provides a <see cref="ActionResolver"/> instance.
    /// Tracks input device state (currently the first active generic
    /// gamepad device is chosen as the input device).
    /// </summary>
    interface IGamepadDriver
    {
        bool HasDevice { get; }

        /// <summary>
        /// Returns the current input device's display name.
        /// </summary>
        /// <remarks>Returns null if there's no current input device.</remarks>
        string DeviceName { get; }

        /// <summary>
        /// The component that sets up InputSystem and forwards its events.
        /// </summary>
        PlayerInput Input { get; }

        /// <summary>
        /// A map for retrieving <see cref="ActionID"/> from InputSystem events.
        /// </summary>
        IActionResolver ActionResolver { get; }

        /// <summary>
        /// Called when the input device is lost or gained.
        /// </summary>
        event Action<bool> OnHasDeviceChanged;

        /// <summary>
        /// Called when an input action changes state.
        /// </summary>
        event Action<InputAction.CallbackContext> OnAction;

        /// <summary>
        /// Called when the input device changes, and when the input actions' bindings are modified.
        /// </summary>
        event Action OnControlsChanged;
    }
}
