namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Implement this interface if you use a class that has to override the virtual camera damping.
    /// </summary>
    interface ICustomDamping
    {
        /// <summary>
        /// Sets damping data.
        /// </summary>
        /// <param name="damping">Damping data</param>
        void SetDamping(Damping damping);
    }
}
