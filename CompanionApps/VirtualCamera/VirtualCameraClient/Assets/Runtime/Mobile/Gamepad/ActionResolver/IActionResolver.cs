using System;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Performs 1-to-1 mapping of <see cref="InputAction"/> (defined in a <see cref="InputActionMap"/>)
    /// to <see cref="ActionID"/>.
    /// </summary>
    interface IActionResolver
    {
        bool TryResolveActionID(InputAction action, out ActionID actionID);
        bool TryResolveActionGuid(ActionID actionID, out Guid actionGuid);
    }
}
