using UnityEngine;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Contains global settings for UI
    /// </summary>
    public static class UIConfig
    {
        /// <summary>
        /// The color of the text of a property, like a button or slider, in its normal state
        /// </summary>
        public static Color propertyTextBaseColor { get; } = new Color32(228, 228, 228, 228);

        /// <summary>
        /// The color of the text of a property, like a button or slider, when it is selected
        /// </summary>
        public static Color propertyTextSelectedColor { get; } = new Color32(240, 240, 240, 255);

        /// <summary>
        /// The color of the text of a property, like a button or slider, when it is inactive
        /// </summary>
        public static Color propertyTextInactiveColor { get; } = new Color32(110, 110, 110, 255);

        /// <summary>
        /// The color of a property, like a button or slider, when it is selected or toggled on
        /// </summary>
        public static Color propertySelectedColor { get; } = new Color32(0, 153, 255, 255);

        /// <summary>
        /// The color of a property, like a button or slider, in it's normal state
        /// </summary>
        public static Color propertyBaseColor { get; } = new Color32(46, 46, 46, 255);

        /// <summary>
        /// The color of a property, like a button or slider, when it is disabled
        /// </summary>
        public static Color propertyBaseInactiveColor { get; } = new Color32(35, 35, 35, 255);

        /// <summary>
        /// The color of a property, like a button or slider, when it is being pressed down
        /// </summary>
        public static Color propertyPressedColor { get; } = new Color32(30, 30, 30, 255);

        /// <summary>
        /// The color of the dropdown arrow, when not selected
        /// </summary>
        public static Color dropDownArrowBaseColor { get; } = new Color32(170, 170, 170, 255);

        /// <summary>
        /// The duration in seconds of a dialog fading in and out when being opened or closed
        /// </summary>
        public static float dialogFadeTime { get; } = 0.1f;

        /// <summary>
        /// The color transition time (in seconds) of a property between states (e.g. pressed, selected, normal, etc)
        /// </summary>
        public static float propertyColorTransitionTime { get; } = 0.1f;

        public static Color xAxisColor { get; } = new Color32(238, 118, 77, 255);

        public static Color yAxixColor { get; } = new Color32(195, 251, 115, 255);

        public static Color zAxisColor { get; } = new Color32(148, 215, 251, 255);
    }
}
