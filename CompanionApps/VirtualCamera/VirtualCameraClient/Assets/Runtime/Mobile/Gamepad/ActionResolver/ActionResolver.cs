using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Allows us to define 1-to-1 mapping of <see cref="InputAction"/>
    /// to <see cref="ActionID"/> using a ScriptableObject asset with a basic UI.
    /// </summary>
    class ActionResolver : ScriptableObject, IActionResolver
    {
        [Serializable]
        class Binding
        {
            [SerializeField]
            InputActionReference m_InputAction;

            [SerializeField]
            ActionID m_ActionID;

            public InputActionReference InputAction => m_InputAction;

            public ActionID ActionID => m_ActionID;
        }

        // Provides a ReorderableList UI
        [SerializeField]
        List<Binding> m_Bindings = new List<Binding>();

        readonly Dictionary<Guid, ActionID> m_Lookup = new Dictionary<Guid, ActionID>();
        readonly Dictionary<ActionID, Guid> m_InverseLookup = new Dictionary<ActionID, Guid>();

        public bool TryResolveActionID(InputAction action, out ActionID actionID)
        {
            return m_Lookup.TryGetValue(action.id, out actionID);
        }

        public bool TryResolveActionGuid(ActionID actionID, out Guid actionGuid)
        {
            return m_InverseLookup.TryGetValue(actionID, out actionGuid);
        }

        void OnEnable()
        {
            m_Lookup.Clear();

            foreach (var binding in m_Bindings)
            {
                m_Lookup.Add(binding.InputAction.action.id, binding.ActionID);
                m_InverseLookup.Add(binding.ActionID, binding.InputAction.action.id);
            }
        }
    }
}
