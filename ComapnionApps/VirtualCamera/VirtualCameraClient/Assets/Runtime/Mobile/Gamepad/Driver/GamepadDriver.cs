using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <inheritdoc cref="IGamepadDriver"/>
    class GamepadDriver : MonoBehaviour, IGamepadDriver
    {
        /// <inheritdoc/>
        public event Action<bool> OnHasDeviceChanged = delegate {};

        /// <inheritdoc/>
        public event Action<InputAction.CallbackContext> OnAction = delegate {};

        /// <inheritdoc/>
        public event Action OnControlsChanged = delegate {};

        [SerializeField]
        ActionResolver m_ActionResolver;

        bool m_LastHasDevice;
        PlayerInput m_Input;

        void Awake()
        {
            m_Input = GetComponent<PlayerInput>();

            m_Input.onActionTriggered += OnActionTriggered;
            m_Input.onControlsChanged += OnControlsChangedTriggered;
        }

        void OnDestroy()
        {
            m_Input.onActionTriggered -= OnActionTriggered;
            m_Input.onControlsChanged -= OnControlsChangedTriggered;
        }

        void Update()
        {
            var hasDevice = HasDevice;
            if (hasDevice != m_LastHasDevice)
            {
                m_LastHasDevice = hasDevice;
                OnHasDeviceChanged.Invoke(hasDevice);
            }
        }

        UnityEngine.InputSystem.Gamepad Device
        {
            get
            {
                foreach (var gamepad in UnityEngine.InputSystem.Gamepad.all)
                {
                    if (gamepad.enabled && gamepad.added)
                    {
                        return gamepad;
                    }
                }

                return null;
            }
        }

        /// <inheritdoc/>
        public bool HasDevice => Device != null;

        /// <inheritdoc/>
        public string DeviceName => Device?.displayName;

        /// <inheritdoc/>
        public PlayerInput Input => m_Input;

        /// <inheritdoc/>
        public IActionResolver ActionResolver => m_ActionResolver;

        void OnActionTriggered(InputAction.CallbackContext ctx)
        {
            OnAction.Invoke(ctx);
        }

        void OnControlsChangedTriggered(PlayerInput playerInput)
        {
            OnControlsChanged.Invoke();
        }
    }
}
