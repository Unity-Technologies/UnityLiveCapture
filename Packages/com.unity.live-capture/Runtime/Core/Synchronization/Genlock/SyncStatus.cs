namespace Unity.LiveCapture
{
    /// <summary>
    /// The <see cref="ISyncProvider"/> synchronization statuses.
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// The sync provider is not running.
        /// </summary>
        Stopped,

        /// <summary>
        /// The sync provider is successfully synchronizing.
        /// </summary>
        Synchronized,

        /// <summary>
        /// The sync provider is failing to synchronize.
        /// </summary>
        NotSynchronized,
    }
}
