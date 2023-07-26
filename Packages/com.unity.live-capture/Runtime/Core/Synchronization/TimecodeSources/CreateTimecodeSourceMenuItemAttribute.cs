using System;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An attribute placed on <see cref="ITimecodeSource"/> implementations to specify where
    /// the source appears in the create timecode source menu.
    /// </summary>
    /// <remarks>
    /// This only has any effect for timecode sources that inherit from UnityEngine.Component.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CreateTimecodeSourceMenuItemAttribute : MenuPathAttribute
    {
        /// <summary>
        /// Creates a new <see cref="CreateTimecodeSourceMenuItemAttribute"/> instance.
        /// </summary>
        /// <param name="itemName">The menu item represented like a path name. For example, the menu item
        /// could be "Sub Menu/Action".</param>
        /// <param name="priority">The order by which the menu items are displayed. Items in the same sub
        /// menu have a separator placed between them if their priority differs by more than 10.</param>
        public CreateTimecodeSourceMenuItemAttribute(string itemName, int priority = 0) : base(itemName, priority)
        {
        }
    }
}
