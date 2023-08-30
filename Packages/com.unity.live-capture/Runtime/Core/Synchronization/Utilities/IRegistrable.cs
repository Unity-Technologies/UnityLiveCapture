namespace Unity.LiveCapture
{
    /// <summary>
    /// An interface which allows indexing using an ID string.
    /// </summary>
    public interface IRegistrable
    {
        /// <summary>
        /// Get the unique identifier for this instance.
        /// </summary>
        /// <remarks>
        /// The ID should not be null or empty, and is expected to remain constant for the life of the instance.
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// Get a human-readable name for the instance.
        /// </summary>
        string FriendlyName { get; }
    }
}
