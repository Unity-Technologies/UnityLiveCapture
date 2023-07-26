using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.Editor
{
    [CustomPropertyDrawer(typeof(RegisteredRef<>), true)]
    abstract class RegisteredRefPropertyDrawer<T> : PropertyDrawer where T : class, IRegistrable
    {
        protected virtual Registry<T> DefaultRegistry => null;

        static class Contents
        {
            public const string NoneOption = "None";
            public static readonly Vector2 OptionDropdownSize = new Vector2(300f, 250f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var prop = new EditorGUI.PropertyScope(position, label, property);

            if (prop.content != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, prop.content);
            }

            var idProp = property.FindPropertyRelative("m_Id");
            var registryProp = property.FindPropertyRelative("m_Registry");

            // if the serialized registry doesn't exist or is not created yet we fall back to a default registry if we can
            if (!Registry<T>.TryGetRegistry(registryProp.stringValue, out var registry))
            {
                if (DefaultRegistry == null)
                {
                    return;
                }

                registry = DefaultRegistry;
                registryProp.stringValue = DefaultRegistry.Name;
                registryProp.serializedObject.ApplyModifiedProperties();
            }

            var selectedInstance = registry[idProp.stringValue];
            var selectedName = selectedInstance != null ? selectedInstance.FriendlyName : Contents.NoneOption;
            var selectedContent = EditorGUIUtility.TrTempContent(selectedName);

            if (GUI.Button(position, selectedContent, EditorStyles.popup))
            {
                var instances = registry.Entries
                    .Where(s => s != null)
                    .OrderBy(s => s.FriendlyName)
                    .ToArray();

                var names = new[] { Contents.NoneOption }
                    .Concat(instances.Select(s => s.FriendlyName))
                    .Select(s => new GUIContent(s))
                    .ToArray();

                OptionSelectWindow.SelectOption(position, Contents.OptionDropdownSize, names, (index, value) =>
                {
                    idProp.stringValue = index > 0 ? instances[index - 1].Id : null;
                    idProp.serializedObject.ApplyModifiedProperties();
                });
            }
        }
    }
}
