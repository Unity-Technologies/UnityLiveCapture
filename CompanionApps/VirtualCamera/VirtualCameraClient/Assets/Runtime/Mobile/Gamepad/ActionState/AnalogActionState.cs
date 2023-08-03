using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Tracks the state of an analog action (like Move Forward).
    /// </para>
    ///
    /// <para>
    /// The control that this action is bound to may be discrete or continuous.
    /// Discrete control: max sensitivity when pressed, 0 when not pressed.
    /// Continuous control: apply value directly.
    /// </para>
    /// </summary>
    class AnalogActionState : ActionState
    {
        Action<float> m_Callback;
        AxisState m_Axis;
        AxisState.Direction m_AxisDirection;
        AnalogActionState m_OppositeAction;

        /// <summary>
        /// This callback is called when the value of the action changes.
        /// </summary>
        public Action<float> Callback
        {
            get => m_Callback;
            set => m_Callback = value;
        }

        /// <summary>
        /// The axis value tracker that takes into account the actuation of the opposite action.
        /// </summary>
        /// <remarks>Null if not part of an axis.</remarks>
        public AxisState Axis
        {
            get => m_Axis;
            set => m_Axis = value;
        }

        /// <summary>
        /// Specifies which part of the axis this action represents.
        /// </summary>
        public AxisState.Direction AxisDirection
        {
            get => m_AxisDirection;
            set => m_AxisDirection = value;
        }

        /// <summary>
        /// The opposite action on the axis.
        /// </summary>
        /// <remarks>Null if not part of an axis.</remarks>
        public AnalogActionState OppositeAction
        {
            get => m_OppositeAction;
            set => m_OppositeAction = value;
        }

        public AnalogActionState(ActionID ID, Action<float> callback) : base(ID)
        {
            m_Callback = callback;
        }

        public override void Reset()
        {
            base.Reset();
            m_Axis?.Reset();
        }

        /// <summary>
        /// Processes the new state of the input and potentially invokes the callback.
        /// </summary>
        /// <param name="ctx">The new state of the associated <see cref="InputAction"/></param>
        /// <param name="sensitivity">The sensitivity (or speed) associated with the action</param>
        public void ProcessInput(in ActionUpdateData updateData, float sensitivity)
        {
            base.ProcessInput(updateData, out var shouldSkip);
            if (shouldSkip)
                return;

            if (updateData.ActionType == InputActionType.Button)
            {
                // Bound to discrete controls on the device: maps to either 0 or max sensitivity

                var pressed = updateData.ValueAsButton;

                if (pressed && updateData.Performed)
                {
                    // Prevent the modifier action from triggering when it gets released
                    if (Role == ActionConflictGroup.Role.Composite)
                        ConflictGroup.Modifier?.Skip();

                    InvokeCallback(sensitivity);
                }
                else if (!pressed && updateData.Canceled)
                {
                    InvokeCallback(0.0f);
                }
            }
            else if (updateData.ActionType == InputActionType.Value)
            {
                // Bound to continuous controls on the device: can apply its value directly

                var value = updateData.Value;

                // If the controls are actuated, prevent the modifier action from triggering when it gets released
                if (Role == ActionConflictGroup.Role.Composite && !Mathf.Approximately(value, 0.0f))
                    ConflictGroup.Modifier?.Skip();

                InvokeCallback(value * sensitivity);
            }
        }

        // Invokes the callback using the resolved axis value
        // (takes into account the opposite action on the axis)
        void InvokeCallback(float value)
        {
            if (m_Axis != null)
                value = m_Axis.GetUpdatedValue(m_AxisDirection, value);
            m_Callback?.Invoke(value);
        }
    }
}
