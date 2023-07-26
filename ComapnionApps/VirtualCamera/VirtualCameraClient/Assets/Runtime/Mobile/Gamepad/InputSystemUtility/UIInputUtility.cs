using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Utilities for <see cref="InputSystemUIInputModule"/>.
    /// </summary>
    static class UIInputUtility
    {
        public const string TouchScheme = "Touch";

        /// <summary>
        /// InputSystem optimization, to skip UI navigation processing for devices that don't interact with the UI.
        /// </summary>
        public static void LimitToScheme(string name)
        {
            var inputModule = Object.FindObjectOfType<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                Debug.LogWarning($"No ${nameof(InputSystemUIInputModule)} instance found in the scene");
            }

            var actionsAsset = inputModule.actionsAsset;
            if (inputModule == null)
            {
                Debug.LogWarning($"The ${nameof(InputSystemUIInputModule)} instance has no assigned ${nameof(InputActionAsset)}");
            }

            var controlSchemes = actionsAsset.controlSchemes;
            foreach (var scheme in controlSchemes)
            {
                if (scheme.name == name)
                {
                    actionsAsset.bindingMask = InputBinding.MaskByGroup(scheme.bindingGroup);
                    return;
                }
            }

            Debug.LogWarning($"No scheme with name '${name}' was found in '${actionsAsset.name}'");
        }
    }
}
