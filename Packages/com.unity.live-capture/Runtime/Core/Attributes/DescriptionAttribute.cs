using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An attribute placed on enum values to provide a user friendly description of the enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DescriptionAttribute : PropertyAttribute
    {
        /// <summary>
        /// The description of the enum value.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Creates a new <see cref="DescriptionAttribute"/> instance.
        /// </summary>
        /// <param name="description">The description of the enum value</param>
        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
