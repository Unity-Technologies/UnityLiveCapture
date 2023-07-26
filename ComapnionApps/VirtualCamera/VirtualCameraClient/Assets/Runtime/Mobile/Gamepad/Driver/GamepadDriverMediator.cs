using UnityEngine.InputSystem;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Connects <see cref="GamepadSystem"/> to a source of input.
    /// </summary>
    class GamepadDriverMediator : IInitializable
    {
        [Inject]
        IGamepadDriver m_GamepadDriver;

        [Inject]
        GamepadSystem m_GamepadSystem;

        public void Initialize()
        {
            m_GamepadDriver.OnAction += OnAction;
            m_GamepadDriver.OnControlsChanged += OnControlsChanged;

            m_GamepadSystem.ActionResolver = m_GamepadDriver.ActionResolver;
            m_GamepadSystem.ActionMap = m_GamepadDriver.Input.currentActionMap;
        }

        void OnAction(InputAction.CallbackContext ctx)
        {
            var updateData = new ActionUpdateData(ctx);
            m_GamepadSystem.OnActionTriggered(ctx.action, updateData);
        }

        void OnControlsChanged()
        {
            m_GamepadSystem.OnControlsChanged();
        }
    }
}
