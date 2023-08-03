using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// Modifiers in InputSystem don't exclude the main action from firing
    /// (ex the action bound to LT also fired when the action bound to A + LT fires).
    /// </para>
    ///
    /// <para>
    /// Additionally, any action bound to A can only be a discrete action, should
    /// only fire on release, and only if A + LT wasn't fired while it was being held.
    /// </para>
    ///
    /// <para>
    /// All this requires state tracking outside of InputSystem, but first we must
    /// find and associate all these conflicting inputs using this class.
    /// </para>
    /// </summary>
    static class ConflictGroupResolution
    {
        /// <summary>
        /// A set of potentially conflicting actions involved in a single composite action.
        /// </summary>
        public struct ConflictGroup
        {
            /// <summary>
            /// Bound to A + LT
            /// </summary>
            public InputAction CompositeAction;

            /// <summary>
            /// Bound to LT
            /// </summary>
            public InputAction MainAction;

            /// <summary>
            /// Bound to A
            /// </summary>
            public InputAction ModifierAction;

            /// <summary>
            /// The control representing A on the device
            /// (provides access to the raw modifier value)
            /// </summary>
            public InputControl ModifierControl;
        }

        struct CompositeBinding
        {
            public InputControl Main;
            public InputControl Modifier;
            public string MainPath;
            public string ModifierPath;

            public bool HasMain => Main != null;
            public bool HasModifier => HasMain && Modifier != null;
        }

        public static void Resolve(InputActionMap map, out List<ConflictGroup> groups)
        {
            groups = new List<ConflictGroup>();
            var pathToAction = new Dictionary<string, InputAction>();

            foreach (var action in map.actions)
            {
                var binding = ResolveCompositeBinding(action);

                if (binding.HasMain && !binding.HasModifier)
                    pathToAction.Add(binding.MainPath, action);
            }

            foreach (var action in map.actions)
            {
                var binding = ResolveCompositeBinding(action);

                if (binding.HasMain && binding.HasModifier)
                {
                    var group = new ConflictGroup
                    {
                        CompositeAction = action,
                        ModifierControl = binding.Modifier
                    };

                    if (pathToAction.TryGetValue(binding.MainPath, out var mainAction))
                        group.MainAction = mainAction;

                    if (pathToAction.TryGetValue(binding.ModifierPath, out var modifierAction))
                        group.ModifierAction = modifierAction;

                    groups.Add(group);
                }
            }
        }

        static CompositeBinding ResolveCompositeBinding(InputAction action)
        {
            var compositeBinding = new CompositeBinding
            {
                Main = null,
                Modifier = null,
                MainPath = string.Empty,
                ModifierPath = string.Empty
            };

            foreach (var control in action.controls)
            {
                var binding = action.GetBindingForControl(control);
                if (binding.HasValue && binding.Value.isPartOfComposite)
                {
                    if (binding.Value.name == OneModifierCustomComposite.MainBindingName)
                    {
                        compositeBinding.MainPath = binding.Value.effectivePath;
                        compositeBinding.Main = control;
                    }
                    else if (binding.Value.name == OneModifierCustomComposite.ModifierBindingName)
                    {
                        compositeBinding.ModifierPath = binding.Value.effectivePath;
                        compositeBinding.Modifier = control;
                    }
                }
            }

            return compositeBinding;
        }
    }
}
