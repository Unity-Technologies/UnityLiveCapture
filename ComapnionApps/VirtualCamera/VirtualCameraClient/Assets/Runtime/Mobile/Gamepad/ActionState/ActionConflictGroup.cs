using System;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Stores the associations resolved by <see cref="ConflictGroupResolution"/>.
    /// </para>
    ///
    /// <para>
    /// Allows to define the interactions of for example the A (modifier),
    /// LT (main), and A + LT (composite) actions.
    /// </para>
    /// </summary>
    class ActionConflictGroup
    {
        public enum Role
        {
            /// <summary>
            /// Not overlapping with any composite actions.
            /// </summary>
            None,

            /// <summary>
            /// The action bound to A + LT
            /// </summary>
            Composite,

            /// <summary>
            /// The action bound to LT
            /// </summary>
            Main,

            /// <summary>
            /// The action bound to A
            /// </summary>
            Modifier
        }

        /// <summary>
        /// Represents the action bound to A + LT
        /// </summary>
        public ActionState Composite;

        /// <summary>
        /// Represents the action bound to LT
        /// </summary>
        public ActionState Main;

        /// <summary>
        /// Represents the action bound to A
        /// </summary>
        public ButtonActionState Modifier;

        /// <summary>
        /// Represents the A button on the device
        /// (provides access to the raw modifier value)
        /// </summary>
        public InputControl ModifierControl;

        /// <summary>
        /// True while <see cref="ModifierControl"/> is held.
        /// </summary>
        public bool ModifierIsDown => m_ModifierDown;

        bool m_ModifierDown;

        public void Register(ActionState state, Role role)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            state.Role = role;

            if (role == Role.Composite)
            {
                state.ConflictGroup = this;
                Composite = state;
            }
            else if (role == Role.Main)
            {
                state.ConflictGroups.Add(this);
                Main = state;
            }
            else if (role == Role.Modifier)
            {
                if (state is ButtonActionState modifierButton)
                {
                    modifierButton.ConflictGroups.Add(this);
                    Modifier = modifierButton;
                }
            }
        }

        public void Update()
        {
            m_ModifierDown = ModifierControl.IsPressed();
        }
    }
}
