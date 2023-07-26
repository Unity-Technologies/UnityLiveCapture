using System;

namespace Unity.LiveCapture
{
    /// <summary>
    /// An attribute placed on <see cref="LiveCaptureDevice"/> implementations to specify where
    /// the device appears in the create device menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CreateDeviceMenuItemAttribute : MenuPathAttribute
    {
        /// <summary>
        /// Creates a new <see cref="CreateDeviceMenuItemAttribute"/> instance.
        /// </summary>
        /// <param name="itemName">The menu item represented like a path name. For example, the menu item
        /// could be "Sub Menu/Action".</param>
        /// <param name="priority">The order by which the menu items are displayed. Items in the same sub
        /// menu have a separator placed between them if their priority differs by more than 10.</param>
        public CreateDeviceMenuItemAttribute(string itemName, int priority = 0) : base(itemName, priority)
        {
        }
    }
}
