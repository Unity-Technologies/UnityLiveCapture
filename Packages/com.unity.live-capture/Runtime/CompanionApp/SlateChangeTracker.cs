namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// Use this class for tracking changes in a <see cref="Shot"/>.
    /// </summary>
    class ShotChangeTracker
    {
        Shot? m_Shot;

        /// <summary>
        /// Restarts the tracking.
        /// </summary>
        public void Reset()
        {
            m_Shot = null;
        }

        /// <summary>
        /// Checks whether the given shot is different from a previous call to this method.
        /// </summary>
        /// <returns> true if different; otherwise, false.</returns>
        public bool Changed(Shot? shot)
        {
            var changed = m_Shot != shot;

            m_Shot = shot;

            return changed;
        }
    }
}
