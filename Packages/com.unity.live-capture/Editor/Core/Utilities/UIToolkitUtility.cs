using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    static class UIToolkitUtility
    {
        public static void SetDisplay(this VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void MoveChildrenTo(this VisualElement from, VisualElement to)
        {
            while (from.childCount > 0)
            {
                var child = from.Children().First();
                child.RemoveFromHierarchy();
                to.Add(child);
            }
        }

        /// <summary>
        /// Convenience extension to get a callback after initial geometry creation, making it easier to use lambdas.
        /// Callback will only be called once. Works in inspectors and PropertyDrawers.
        /// </summary>
        public static void RegisterGeometryChangedEventCallbackOnce(this VisualElement owner, Action callback)
        {
            owner.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            void OnGeometryChanged(GeometryChangedEvent _)
            {
                owner.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged); // call only once
                callback();
            }
        }
    }
}
