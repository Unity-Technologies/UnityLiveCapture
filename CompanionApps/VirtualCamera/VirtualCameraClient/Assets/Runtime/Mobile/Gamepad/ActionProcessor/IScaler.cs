namespace Unity.CompanionApps.VirtualCamera.Gamepad
{
    /// <summary>
    /// Used by <see cref="IValueTracker{T}"/> implementations to map between
    /// the input and the value scales.
    /// </summary>
    interface IScaler<T>
    {
        /// <summary>
        /// Constrains a value to the input scale.
        /// </summary>
        T ClampToInputScale(T value);

        /// <summary>
        /// Converts a value from value to input scale.
        /// </summary>
        T ToInputScale(T value);

        /// <summary>
        /// Converts a value from input to value scale.
        /// </summary>
        T ToValueScale(T value);
    }
}
