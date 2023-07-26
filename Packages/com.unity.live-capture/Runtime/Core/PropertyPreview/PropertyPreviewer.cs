using UnityEngine;
using UnityObject = UnityEngine.Object;
using Unity.LiveCapture.Internal;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A class that allows to register properties to prevent Unity from
    /// marking Prefabs or the Scene as modified when you preview animations.
    /// </summary>
    public class PropertyPreviewer : IPropertyPreviewer
    {
        UnityObject m_Driver;

        /// <summary>
        /// The key to identify the group of registered properties.
        /// </summary>
        public UnityObject Driver => m_Driver;

        /// <summary>
        /// Constructs a new <see cref="PropertyPreviewer"/>.
        /// </summary>
        /// <param name="driver">Key to identify the group of registered properties.</param>
        public PropertyPreviewer(UnityObject driver)
        {
            m_Driver = driver;
        }

        /// <summary>
        /// Restores the properties previously registered.
        /// </summary>>
        public void Restore()
        {
            DrivenPropertyManagerInternal.UnregisterProperties(m_Driver);
        }

        /// <inheritdoc/>
        public void Register(Component component, string propertyName)
        {
            DrivenPropertyManagerInternal.RegisterProperty(m_Driver, component, propertyName);
        }

        /// <inheritdoc/>
        public void Register(GameObject go, string propertyName)
        {
            DrivenPropertyManagerInternal.RegisterProperty(m_Driver, go, propertyName);
        }
    }
}
