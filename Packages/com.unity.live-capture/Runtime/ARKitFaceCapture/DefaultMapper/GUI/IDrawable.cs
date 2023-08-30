using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper
{
    /// <summary>
    /// An interface for classes that can be drawn using IMGUI.
    /// </summary>
    interface IDrawable
    {
#if UNITY_EDITOR
        /// <summary>
        /// Gets the vertical space needed to draw the GUI for this instance.
        /// </summary>
        float GetHeight();

        /// <summary>
        /// Draws the inspector GUI for this instance.
        /// </summary>
        /// <param name="rect">The rect to draw the property in.</param>
        void OnGUI(Rect rect);
#endif
    }
}
