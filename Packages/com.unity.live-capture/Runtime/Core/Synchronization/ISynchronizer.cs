namespace Unity.LiveCapture
{
    /// <summary>
    /// Manages a collection of <see cref="ITimedDataSource"/> instances for the purpose of
    /// coordinating their respective data to be presented in a temporally
    /// coherent manner.
    /// </summary>
    public interface ISynchronizer
    {
        /// <summary>
        /// The source of the timecodes used for synchronization.
        /// </summary>
        ITimecodeSource TimecodeSource { get; set; }

        /// <summary>
        /// The time to synchronize to for the current frame.
        /// </summary>
        /// <remarks>
        /// If no <see cref="TimecodeSource"/> is set, the value is <see langword="null"/>.
        /// </remarks>
        FrameTimeWithRate? PresentTime { get; }

        /// <summary>
        /// Get the number of timed data sources in the synchronization group.
        /// </summary>
        int DataSourceCount { get; }

        /// <summary>
        /// Add a timed data source to the synchronization group.
        /// </summary>
        /// <param name="source">The timed data source to add.</param>
        /// <returns><see langword="true"/> if the source is in the synchronization group; otherwise, <see langword="false"/>.</returns>
        bool AddDataSource(ITimedDataSource source);

        /// <summary>
        /// Remove a timed data source from the synchronization group.
        /// </summary>
        /// <param name="source">The timed data source to remove.</param>
        /// <returns><see langword="true"/> if the source was removed from the synchronization group; otherwise, <see langword="false"/>.</returns>
        bool RemoveDataSource(ITimedDataSource source);

        /// <summary>
        /// Get the timed data source at the specified index.
        /// </summary>
        /// <param name="index">The index of the timed data source.</param>
        /// <returns>The timed data source at the index; <see langword="null"/> if the index is invalid.</returns>
        ITimedDataSource GetDataSource(int index);

        /// <summary>
        /// Perform synchronized update on synchronized group.
        /// </summary>
        void Update();
    }
}
