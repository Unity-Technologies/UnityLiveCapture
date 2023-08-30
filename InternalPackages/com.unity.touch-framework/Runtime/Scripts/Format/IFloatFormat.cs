namespace Unity.TouchFramework
{
    /// <summary>
    /// Abstraction for float format.
    /// </summary>
    public interface IFloatFormat
    {
        /// <summary>
        /// Returns the string format for a float
        /// </summary>
        /// <param name="value">The value to check for the formating</param>
        /// <returns>The requested format for this value</returns>
        string GetStringFormat(float value);
    }
}
