using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.LiveCapture
{
    /// <summary>
    /// A serializable implementation of <see cref="ITakeBinding"/> that contains values of
    /// type UnityEngine.Object.
    /// </summary>
    /// <typeparam name="T">The type of the TakeBinding. It must be a UnityObject.</typeparam>
    [Serializable]
    public class TakeBinding<T> : ITakeBinding where T : UnityObject
    {
        [SerializeField]
        ExposedReference<T> m_ExposedReference;

        /// <inheritdoc/>
        public Type Type => typeof(T);

        /// <inheritdoc/>
        public PropertyName PropertyName => m_ExposedReference.exposedName;

        /// <inheritdoc/>
        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name));

            m_ExposedReference.exposedName = new PropertyName(name);
        }

        /// <inheritdoc/>
        UnityObject ITakeBinding.GetValue(IExposedPropertyTable resolver)
        {
            return this.GetValue(resolver);
        }

        /// <summary>
        /// Gets the resolved value of the binding.
        /// </summary>
        /// <param name="resolver">The resolve table.</param>
        /// <returns> The resolved object reference. </returns>
        public T GetValue(IExposedPropertyTable resolver)
        {
            return m_ExposedReference.Resolve(resolver) as T;
        }

        /// <inheritdoc/>
        public void SetValue(UnityObject value, IExposedPropertyTable resolver)
        {
            resolver.SetReferenceValue(m_ExposedReference.exposedName, value);
        }

        /// <inheritdoc/>
        public void ClearValue(IExposedPropertyTable resolver)
        {
            resolver.ClearReferenceValue(m_ExposedReference.exposedName);
        }
    }
}
