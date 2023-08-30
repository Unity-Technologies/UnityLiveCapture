using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Represents an animation preview system. It allows to register properties to prevent Unity from
    /// marking Prefabs or the Scene as modified when you preview animations.
    /// </summary>
    public interface IPropertyPreviewer
    {
        /// <summary>
        /// Adds a property from a specified Component.
        /// </summary>
        /// <param name="component">The Component to register the property from.</param>
        /// <param name="propertyName">The name of the property to register.</param>
        void Register(Component component, string propertyName);

        /// <summary>
        /// Adds a property from a specified GameObject.
        /// </summary>
        /// <param name="go">The GameObject to register the property from.</param>
        /// <param name="propertyName">The name of the property to register.</param>
        void Register(GameObject go, string propertyName);
    }
}
