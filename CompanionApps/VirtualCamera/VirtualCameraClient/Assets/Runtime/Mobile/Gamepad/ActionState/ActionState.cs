using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Tracks the state of an associated <see cref="InputAction"/>.
    /// </summary>
    abstract class ActionState
    {
        readonly ActionID m_ID;
        bool m_Actuated;
        bool m_Active;
        bool m_Skipped;

        ActionConflictGroup m_ConflictGroup;
        List<ActionConflictGroup> m_ConflictGroups = new List<ActionConflictGroup>();
        ActionConflictGroup.Role m_Role;

        /// <summary>
        /// The associated action ID.
        /// </summary>
        public ActionID ID => m_ID;

        /// <summary>
        /// True while the action is being held (for discrete controls)
        /// or pushed beyond InputSystem's default threshold (for continuous controls).
        /// </summary>
        public bool Actuated
        {
            get => m_Actuated;
            protected set => m_Actuated = value;
        }

        /// <summary>
        /// True while the action is actuated and being performed
        /// (meaning it hasn't been skipped because of other conflicting actions).
        /// </summary>
        public bool Active
        {
            get => m_Active;
            protected set => m_Active = value;
        }

        /// <summary>
        /// Whether the current actuation of this action is being ignored
        /// (preventing it from invoking its callback).
        /// </summary>
        public bool Skipped
        {
            get => m_Skipped;
            protected set => m_Skipped = value;
        }

        /// <summary>
        /// Ignores the action's current actuation
        /// (prevents it from invoking its callback).
        /// </summary>
        public void Skip()
        {
            m_Skipped = true;
        }

        /// <summary>
        /// The conflict group that this action belongs to if its role is <see cref="ActionConflictGroup.Role.Composite"/>.
        /// </summary>
        public ActionConflictGroup ConflictGroup
        {
            get => m_ConflictGroup;
            set => m_ConflictGroup = value;
        }

        /// <summary>
        /// The conflict group that this actions belongs to (can be multiple) if its role is
        /// <see cref="ActionConflictGroup.Role.Main"/> or <see cref="ActionConflictGroup.Role.Modifier"/>.
        /// </summary>
        public List<ActionConflictGroup> ConflictGroups
        {
            get => m_ConflictGroups;
            set => m_ConflictGroups = value;
        }

        public ActionConflictGroup.Role Role
        {
            get => m_Role;
            set => m_Role = value;
        }

        protected ActionState(ActionID ID)
        {
            m_ID = ID;
        }

        public virtual void Reset()
        {
            m_Skipped = false;
            m_Actuated = false;
            m_Active = false;

            m_ConflictGroup = null;
            m_ConflictGroups.Clear();
            m_Role = ActionConflictGroup.Role.None;
        }

        /// <summary>
        /// Processes the new state of the input.
        /// </summary>
        /// <param name="ctx">The new state of the associated <see cref="InputAction"/></param>
        /// <param name="shouldSkip">Whether the action is currently being ignored (due to other conflicting actions)</param>
        protected void ProcessInput(in ActionUpdateData updateData, out bool shouldSkip)
        {
            // Update actuated state
            if (updateData.Phase.IsInProgress())
                Actuated = !Mathf.Approximately(updateData.Value, 0.0f);
            else if (updateData.Canceled)
                Actuated = false;

            // Check for conflicts and update skipped state
            if (updateData.Started)
                Skipped = Conflicting();
            if (Skipped)
            {
                if (updateData.Canceled)
                {
                    Active = false;
                    Skipped = false;
                }

                shouldSkip = true;
                return;
            }

            // Update active state
            Active = Actuated;

            shouldSkip = false;
        }

        /// <summary>
        /// True when an action related via <see cref="ActionConflictGroup"/> is
        /// actuated, conflicting, and has higher priority than this action.
        /// </summary>
        bool Conflicting()
        {
            if (m_Role == ActionConflictGroup.Role.Main)
                return m_ConflictGroups.Exists(group => group.ModifierIsDown);
            if (m_Role == ActionConflictGroup.Role.Modifier)
                return m_ConflictGroups.Exists(group => group.Composite.Active);
            return false;
        }
    }
}
