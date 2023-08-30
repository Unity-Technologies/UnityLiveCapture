using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Wraps <see cref="UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation"/>
    /// to detect an optional action modifier during binding.
    /// Actuations are detected in chronological order, with the first assumed to be the modifier.
    /// </summary>
    /// <remarks>Beware the <see cref="GetGenericGamepadPath"/> limitation.</remarks>
    class GamepadRebindingOperation
    {
        public event Action<InputAction> OnBindingApplied = delegate {};

        InputAction m_Action;
        InputActionRebindingExtensions.RebindingOperation m_Operation;
        readonly List<InputControl> m_ChronologicalCandidates = new List<InputControl>();

        public void Start(InputAction action)
        {
            m_Action = action;
            m_ChronologicalCandidates.Clear();

            m_Operation?.Dispose();

            m_Operation = new InputActionRebindingExtensions.RebindingOperation()
                .WithControlsHavingToMatchPath("<Gamepad>")
                .WithExpectedControlType<ButtonControl>()
                .WithMagnitudeHavingToBeGreaterThan(0.5f)
                .WithMatchingEventsBeingSuppressed()
                .OnMatchWaitForAnother(0.5f)
                .WithTimeout(2.5f);

            m_Operation.OnPotentialMatch(OnPotentialMatch);
            m_Operation.OnApplyBinding(OnApplyBinding);
            m_Operation.OnComplete(OnComplete);
            m_Operation.OnCancel(OnCancel);

            m_Operation.Start();
        }

        public bool IsListening()
        {
            return m_Operation != null;
        }

        void OnPotentialMatch(InputActionRebindingExtensions.RebindingOperation operation)
        {
            foreach (var candidate in operation.candidates)
            {
                if (!m_ChronologicalCandidates.Contains(candidate))
                {
                    m_ChronologicalCandidates.Add(candidate);
                }
            }
        }

        void OnApplyBinding(InputActionRebindingExtensions.RebindingOperation operation, string path)
        {
            // Skip, apply binding in OnComplete instead which will be called shortly after
        }

        void OnComplete(InputActionRebindingExtensions.RebindingOperation operation)
        {
            var bindings = m_Action.bindings;

            for (var bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
            {
                var binding = bindings[bindingIndex];

                if (!binding.isPartOfComposite)
                    continue;

                if (binding.name == OneModifierCustomComposite.ModifierBindingName)
                {
                    if (m_ChronologicalCandidates.Count >= 2)
                    {
                        var control = m_ChronologicalCandidates[0];
                        m_Action.ApplyBindingOverride(bindingIndex, GetGenericGamepadPath(control));
                    }
                    else
                    {
                        m_Action.ApplyBindingOverride(bindingIndex, string.Empty);
                    }
                }
                else if (binding.name == OneModifierCustomComposite.MainBindingName && m_ChronologicalCandidates.Count >= 1)
                {
                    var control = m_ChronologicalCandidates[m_ChronologicalCandidates.Count - 1];
                    m_Action.ApplyBindingOverride(bindingIndex, GetGenericGamepadPath(control));
                }
            }

            OnBindingApplied.Invoke(m_Action);

            operation.Dispose();
            m_Operation = null;
        }

        void OnCancel(InputActionRebindingExtensions.RebindingOperation operation)
        {
            operation.Dispose();
            m_Operation = null;
        }

        /// <summary>
        /// Gamepad-specific hack until RebindingOperation.GeneratePathForControl is made public.
        /// Returns "<Gamepad>/buttonSouth" for a control whose path could be "/XInputControllerWindows/buttonSouth".
        /// </summary>
        /// <param name="validate">
        /// Checks that the generic path actually exists on a connected device.
        /// If it doesn't exist, the specific path will be returned instead.
        /// </param>
        string GetGenericGamepadPath(InputControl control, bool validate = true)
        {
            var specificControl = control;
            StringBuilder builder = new StringBuilder();

            while (control != null)
            {
                if (control is UnityEngine.InputSystem.Gamepad)
                {
                    builder.Insert(0, "<Gamepad>");
                }
                else
                {
                    builder.Insert(0, control.name);
                    builder.Insert(0, InputControlPath.Separator);
                }

                control = control.parent;
            }

            var genericPath = builder.ToString();
            if (validate)
            {
                var matchedControl = InputSystem.FindControl(genericPath);
                return matchedControl == null ? specificControl.path : genericPath;
            }

            return genericPath;
        }
    }
}
