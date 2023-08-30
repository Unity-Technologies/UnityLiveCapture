using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Convenience functions for applying user overrides on InputBindings.
    /// </summary>
    static class BindingOverrides
    {
        /// <summary>
        /// Sets <see cref="InputBinding.overrideInteractions"/>.
        /// </summary>
        /// <remarks>
        /// Early exit if the new and old overrides are identical to
        /// avoid triggering an InputSystem resolve call
        /// </remarks>
        /// <param name="action">The action to override</param>
        /// <param name="binding">The specific binding to override</param>
        /// <param name="newInteractions">The new interactions to set</param>
        public static void OverrideInteractions(InputAction action, InputBinding binding, string newInteractions)
        {
            var oldInteractions = binding.overrideInteractions;
            if (newInteractions != oldInteractions)
            {
                var bindingOverride = new InputBinding
                {
                    id = binding.id,
                    overrideInteractions = newInteractions,
                    overridePath = binding.overridePath,
                    overrideProcessors = binding.overrideProcessors
                };
                action.actionMap.ApplyBindingOverride(bindingOverride);
            }
        }
    }
}
