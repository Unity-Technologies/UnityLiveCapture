using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// A version of <see cref="UnityEngine.InputSystem.Composites.OneModifierComposite"/> built to handle
    /// the case where the modifier is unset. The stock implementation will never fire the action in that case,
    /// while ours will treat the modifier as always pressed.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad] // Automatically register in editor.
#endif
    [DisplayStringFormat("{modifier}+{binding}")]
    [DisplayName("Binding With One Modifier (Custom)")]
    public class OneModifierCustomComposite : InputBindingComposite<float>
    {
        /// <summary>
        /// Corresponds to the <see cref="InputBinding.name"/> of the binding part of the composite input.
        /// </summary>
        public const string MainBindingName = "binding";

        /// <summary>
        /// Corresponds to the <see cref="InputBinding.name"/> of the modifier part of the composite input.
        /// </summary>
        public const string ModifierBindingName = "modifier";

        [InputControl(layout = "Button")] public int modifier;

        [InputControl] public int binding;

        bool m_HasModifier;

        /// <inheritdoc/>
        public override float ReadValue(ref InputBindingCompositeContext context)
        {
            if (!m_HasModifier || context.ReadValueAsButton(modifier))
                return context.ReadValue<float>(binding);
            return default;
        }

        /// <inheritdoc/>
        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            if (!m_HasModifier || context.ReadValueAsButton(modifier))
                return context.EvaluateMagnitude(binding);
            return default;
        }

        /// <inheritdoc/>
        protected override void FinishSetup(ref InputBindingCompositeContext context)
        {
            HasPart(ref context, modifier, out m_HasModifier);
        }

        /// <summary>
        /// Checks whether the assigned composite part was assigned a control.
        /// </summary>
        static void HasPart(ref InputBindingCompositeContext context, int part, out bool result)
        {
            result = false;

            foreach (var control in context.controls)
            {
                if (control.part == part && control.control.path != string.Empty)
                {
                    result = true;
                    break;
                }
            }
        }

        static OneModifierCustomComposite()
        {
            InputSystem.RegisterBindingComposite<OneModifierCustomComposite>();
        }

        [RuntimeInitializeOnLoadMethod]
        static void Init() {} // Trigger static constructor.
    }
}
