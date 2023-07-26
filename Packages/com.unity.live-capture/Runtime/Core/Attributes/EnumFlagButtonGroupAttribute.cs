using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An attribute placed on serialized flags enum properties to draw the enum using a group of
    /// buttons instead of the default dropdown.
    /// </summary>
    /// <remarks>
    /// This is useful to avoid blocking the main thread while the dropdown popup is active.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumFlagButtonGroupAttribute : EnumButtonGroupAttribute
    {
        /// <inheritdoc />
        public EnumFlagButtonGroupAttribute(float width)
            : base(width) { }
    }
}
