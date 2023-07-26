namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// The techniques which can be used to determine focus distance for a virtual camera.
    /// </summary>
    enum FocusMode : byte
    {
        /// <summary>
        /// Everything is in focus.
        /// </summary>
        /// <remarks>Depth of Field is disabled.</remarks>
        [Description("Everything is in focus.")]
        Clear = 0,
        /// <summary>
        /// The focus distance is manually set by tapping the screen or manipulating the dial.
        /// </summary>
        [Description("The focus distance is manually set by tapping the screen or manipulating the dial.")]
        Manual = 1,
        /// <summary>
        /// The focus adjusts to keep in focus the 3D point under a movable screen-space reticle.
        /// </summary>
        [Description("The focus adjusts to keep in focus the 3D point under a movable screen-space reticle.")]
        ReticleAF = 2,
        /// <summary>
        /// The focus adjusts to match a scene object's distance to the camera.
        /// </summary>
        [Description("The focus adjusts to match a scene object's distance to the camera.")]
        TrackingAF = 3,
    }
}
