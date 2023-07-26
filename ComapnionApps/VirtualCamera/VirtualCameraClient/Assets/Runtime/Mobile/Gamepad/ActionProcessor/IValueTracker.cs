namespace Unity.CompanionApps.VirtualCamera.Gamepad
{

    /// <summary>
    /// Represents a smoothly increasing/decreasing value such as time or lens aperture.
    /// </summary>
    interface IValueTracker<T>
    {
        /// <summary>
        /// The current value.
        /// </summary>
        T Current { get; }

        /// <summary>
        /// True when the value is being increased/decreased in response to user input.
        /// </summary>
        bool Changing { get; }
    }
}
