using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.LiveCapture.Editor
{
    using Editor = UnityEditor.Editor;

    /// <summary>
    /// The default Inspector for <see cref="LiveStreamCaptureDevice"/>.
    /// </summary>
    [CustomEditor(typeof(LiveStreamCaptureDevice), true)]
    public abstract class LiveStreamCaptureDeviceEditor : Editor
    {
        List<(Transform target, Type type)> m_RequiredComponents = new List<(Transform target, Type type)>();

        static class Contents
        {
            public static GUIContent RequiredComponents = EditorGUIUtility.TrTextContentWithIcon(
                "The selected actor requires extra components to work properly.",
                "console.warnicon");
            public static string UndoAddCompoents = L10n.Tr("Add Required Components");
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        /// <seealso cref="DrawDefaultLiveStreamInspector"/>
        public override void OnInspectorGUI()
        {
            DrawDefaultLiveStreamInspector();
        }

        /// <summary>
        /// Makes a custom inspector GUI for <see cref="LiveStreamCaptureDevice"/>.
        /// </summary>
        /// <param name="root">The root transform of the live stream.</param>
        protected void DrawDefaultLiveStreamInspector(Transform root = null)
        {
            var device = target as LiveStreamCaptureDevice;
            var stream = device.Stream;

            if (root == null)
            {
                root = stream.Root;
            }

            if (root == null)
            {
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                m_RequiredComponents.Clear();

                var requiredRootComponents = GetRequiredComponents();

                if (requiredRootComponents != null)
                {
                    foreach (var type in requiredRootComponents)
                    {
                        if (!typeof(Component).IsAssignableFrom(type))
                        {
                            continue;
                        }

                        if (!root.TryGetComponent(type, out _))
                        {
                            m_RequiredComponents.Add((root, type));
                        }
                    }
                }

                foreach (var property in stream.Properties)
                {
                    if (property.Target != null)
                    {
                        continue;
                    }

                    var binding = property.Binding;
                    var transform = root.Find(binding.RelativePath);

                    if (transform == null)
                    {
                        continue;
                    }

                    if (!transform.TryGetComponent(binding.Type, out var component))
                    {
                        m_RequiredComponents.Add((transform, binding.Type));
                    }
                }
            }

            if (m_RequiredComponents.Count > 0)
            {
                LiveCaptureGUI.DrawFixMeBox(Contents.RequiredComponents, () =>
                {
                    AddRequiredComponents(m_RequiredComponents.ToArray());

                    stream.Rebind();
                });
            }
        }

        /// <summary>
        /// Override this method to specify required components that can't be inferred from the stream.
        /// </summary>
        /// <remarks>
        /// The components will be added at the root transform.
        /// </remarks>
        /// <returns>An enumeration of component types to be added at the root transform.</returns>
        protected virtual IEnumerable<Type> GetRequiredComponents() => Array.Empty<Type>();

        /// <summary>
        /// The components will be added to the targets.
        /// </summary>
        /// <param name="requiredComponents">
        /// An enumeration of pairs which are the transform and the required component.
        /// </param>
        internal static void AddRequiredComponents(IEnumerable<(Transform target, Type type)> requiredComponents)
        {
            Undo.SetCurrentGroupName(Contents.UndoAddCompoents);

            foreach (var requiredComponent in requiredComponents)
            {
                var target = requiredComponent.target;
                var type = requiredComponent.type;

                Debug.Assert(target != null);

                if (!target.TryGetComponent(type, out _))
                {
                    Undo.AddComponent(target.gameObject, type);
                }
            }
        }
    }
}
