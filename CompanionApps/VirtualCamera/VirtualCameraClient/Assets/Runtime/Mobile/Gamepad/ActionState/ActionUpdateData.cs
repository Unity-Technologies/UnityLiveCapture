using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// This is an abstracted version of <see cref="InputAction.CallbackContext"/>.
    /// It contains only the fields we need, and can be used in automated tests
    /// for mock input.
    /// </summary>
    struct ActionUpdateData
    {
        /// <summary>
        /// Mirrors the fields of an <see cref="InputAction.CallbackContext"/>.
        /// The action values are initialized using
        /// <see cref="InputAction.CallbackContext.ReadValue{TValue}"/>
        /// and <see cref="InputAction.CallbackContext.ReadValueAsButton"/>.
        /// </summary>
        public ActionUpdateData(InputAction.CallbackContext ctx)
        {
            m_Phase = ctx.phase;
            m_ActionType = ctx.action.type;
            m_Value = ctx.ReadValue<float>();
            m_ValueAsButton = ctx.ReadValueAsButton();
        }

        public ActionUpdateData(InputActionPhase phase, InputActionType actionType, float value, bool valueAsButton)
        {
            m_Phase = phase;
            m_ActionType = actionType;
            m_Value = value;
            m_ValueAsButton = valueAsButton;
        }

        InputActionPhase m_Phase;

        InputActionType m_ActionType;

        float m_Value;

        bool m_ValueAsButton;

        /// <inheritdoc cref="InputAction.CallbackContext.phase"/>
        public InputActionPhase Phase => m_Phase;

        /// <inheritdoc cref="InputAction.type"/>
        public InputActionType ActionType => m_ActionType;

        /// <summary>
        /// The analog value of the action.
        /// </summary>
        public float Value => m_Value;

        /// <summary>
        /// The discrete value of the action.
        /// </summary>
        public bool ValueAsButton => m_ValueAsButton;

        /// <inheritdoc cref="InputAction.CallbackContext.started"/>
        public bool Started => m_Phase == InputActionPhase.Started;

        /// <inheritdoc cref="InputAction.CallbackContext.performed"/>
        public bool Performed => m_Phase == InputActionPhase.Performed;

        /// <inheritdoc cref="InputAction.CallbackContext.canceled"/>
        public bool Canceled => m_Phase == InputActionPhase.Canceled;
    }
}
