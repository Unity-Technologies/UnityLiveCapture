using System;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Tracks the state of a button action (like Toggle Recording).
    /// </para>
    ///
    /// <para>
    /// The control that this action is bound to may be discrete or continuous.
    /// Discrete control: invoke callback when pressed.
    /// Continuous control: invoke callback when the value surpasses InputSystem's default threshold.
    /// </para>
    /// </summary>
    class ButtonActionState : ActionState
    {
        Action m_Callback;

        /// <summary>
        /// This callback is called when the button action is first performed.
        /// </summary>
        public Action Callback
        {
            get => m_Callback;
            set => m_Callback = value;
        }

        // Prevents repeatedly invoking the callback for analog inputs that generate events over multiple frames
        bool m_Fired;

        public ButtonActionState(ActionID ID, Action callback) : base(ID)
        {
            m_Callback = callback;
        }

        public override void Reset()
        {
            base.Reset();
            m_Fired = false;
        }

        /// <summary>
        /// Processes the new state of the input and potentially invokes the callback.
        /// </summary>
        /// <param name="ctx">The new state of the associated <see cref="InputAction"/></param>
        public void ProcessInput(in ActionUpdateData updateData)
        {
            base.ProcessInput(updateData, out var shouldSkip);

            if (!Actuated)
                m_Fired = false;

            if (shouldSkip)
                return;

            if (Role == ActionConflictGroup.Role.Modifier)
            {
                // We know modifiers use the "tap" interaction, which we handle
                // differently because the value would be 0 (just released)

                if (updateData.Performed)
                    m_Callback?.Invoke();
            }
            else
            {
                var pressed = updateData.ValueAsButton;

                if (pressed && updateData.Performed && !m_Fired)
                {
                    m_Fired = true;

                    // Prevent the modifier action from triggering when it gets released
                    if (Role == ActionConflictGroup.Role.Composite)
                        ConflictGroup.Modifier?.Skip();

                    m_Callback?.Invoke();
                }
            }
        }
    }
}
