using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An attribute placed on serialized enum properties to draw the enum using a group of
    /// buttons instead of the default dropdown.
    /// </summary>
    /// <remarks>
    /// This is useful to avoid blocking the main thread while the dropdown popup is active.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumButtonGroupAttribute : PropertyAttribute
    {
        /// <summary>
        /// The desired width of the buttons in pixels.
        /// </summary>
        public readonly float SegmentWidth;

        /// <summary>
        /// Creates a new <see cref="EnumButtonGroupAttribute"/> instance.
        /// </summary>
        /// <param name="width">The desired width of the buttons in pixels.</param>
        public EnumButtonGroupAttribute(float width)
        {
            SegmentWidth = width;
        }
    }
}
