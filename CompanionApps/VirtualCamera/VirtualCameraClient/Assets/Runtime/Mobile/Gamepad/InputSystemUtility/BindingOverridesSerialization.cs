using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// <para>
    /// This is a copy of InputSystem's SaveBindingOverridesAsJson and LoadBindingOverridesFromJson
    /// that fixes the ambiguity of empty vs null strings.
    /// </para>
    ///
    /// <para>
    /// InputSystem's LoadBindingOverridesFromJson throws <see cref="NotImplementedException"/>
    /// when it fails to match a binding override's guid with an input binding.
    /// This prevents any subsequent potentially valid binding overrides from being applied.
    /// This version skips throwing the exception.
    /// </para>
    ///
    /// <para>
    /// Unity's JsonUtility converts null strings to empty strings, so we add bool fields
    /// to serialize the difference.
    /// </para>
    ///
    /// <para>
    /// This is necessary because empty and null strings have different meanings for overrides:
    /// empty means override and clear while null means don't override.
    /// </para>
    /// </summary>
    static class BindingOverridesSerialization
    {
        [Serializable]
        struct BindingOverrideListJson
        {
            public List<BindingOverrideJson> bindings;
        }

        [Serializable]
        struct BindingOverrideJson
        {
            public string action;
            public string id;
            public string path;
            public string interactions;
            public string processors;

            public bool overridePath;
            public bool overrideInteractions;
            public bool overrideProcessors;

            public void PreSave()
            {
                overridePath = path != null;
                overrideInteractions = interactions != null;
                overrideProcessors = processors != null;
            }

            public void PostLoad()
            {
                path = overridePath ? path : null;
                interactions = overrideInteractions ? interactions : null;
                processors = overrideProcessors ? processors : null;
            }
        }

        public static string SaveBindingOverridesAsJson(IInputActionCollection2 actions)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            var overrides = new List<BindingOverrideJson>();
            foreach (var binding in actions.bindings)
                AddBindingOverrideJsonTo(actions, binding, overrides);

            if (overrides.Count == 0)
                return string.Empty;

            return JsonUtility.ToJson(new BindingOverrideListJson {bindings = overrides});
        }

        public static void LoadBindingOverridesFromJson(IInputActionCollection2 actions, string json, bool removeExisting = true)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            if (removeExisting)
                actions.RemoveAllBindingOverrides();

            LoadBindingOverridesFromJsonInternal(actions, json);
        }

        static void AddBindingOverrideJsonTo(IInputActionCollection2 actions, InputBinding binding,
            List<BindingOverrideJson> list, InputAction action = null)
        {
            if (!binding.hasOverrides)
                return;

            if (action == null)
                action = actions.FindAction(binding.action);

            var @override = new BindingOverrideJson
            {
                action = action != null ? $"{action.actionMap.name}/{action.name}" : null,
                id = binding.id.ToString(),
                path = binding.overridePath,
                interactions = binding.overrideInteractions,
                processors = binding.overrideProcessors
            };

            @override.PreSave();

            list.Add(@override);
        }

        static void LoadBindingOverridesFromJsonInternal(IInputActionCollection2 actions, string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            var overrides = JsonUtility.FromJson<BindingOverrideListJson>(json);
            foreach (var entry in overrides.bindings)
            {
                entry.PostLoad();

                if (!string.IsNullOrEmpty(entry.id))
                {
                    var idguid = new Guid(entry.id);
                    var bindingIndex = actions.FindBinding(new InputBinding { id = idguid }, out var action);
                    if (bindingIndex != -1)
                    {
                        action.ApplyBindingOverride(bindingIndex, new InputBinding
                        {
                            overridePath = entry.path,
                            overrideInteractions = entry.interactions,
                            overrideProcessors = entry.processors,
                        });
                    }
                }
            }
        }
    }
}
