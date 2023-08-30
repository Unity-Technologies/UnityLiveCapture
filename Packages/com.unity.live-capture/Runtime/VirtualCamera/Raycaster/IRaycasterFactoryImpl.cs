namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Provides a mechanism for managing <see cref="IRaycaster"/> instances.
    /// </summary>
    interface IRaycasterFactoryImpl
    {
        /// <summary>
        /// Create a <see cref="IRaycaster"/> instance.
        /// </summary>
        /// <returns>The instantiated <see cref="IRaycaster"/>.</returns>
        IRaycaster Create();

        /// <summary>
        /// Dispose a <see cref="IRaycaster"/> instance.
        /// </summary>
        /// <param name="raycaster">The <see cref="IRaycaster"/> to dispose.</param>
        void Dispose(IRaycaster raycaster);
    }
}
