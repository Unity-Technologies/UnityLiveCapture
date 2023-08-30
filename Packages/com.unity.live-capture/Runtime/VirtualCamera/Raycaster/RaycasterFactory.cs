namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Factory that produces <see cref="IRaycaster"/> instances.
    /// </summary>
    static class RaycasterFactory
    {
        static IRaycasterFactoryImpl s_DefaultImpl = new DefaultRaycasterFactoryImpl();

        static IRaycasterFactoryImpl s_Impl = s_DefaultImpl;

        /// <summary>
        /// Override internal implementation of the factory. Added for testing purposes.
        /// </summary>
        internal static void SetImplementation(IRaycasterFactoryImpl impl)
        {
            s_Impl = impl;
        }

        /// <summary>
        /// Restore internal implementation of the factory. Added for testing purposes.
        /// </summary>
        internal static void RestoreDefaultImplementation()
        {
            s_Impl = s_DefaultImpl;
        }

        /// <summary>
        /// Create a <see cref="IRaycaster"/> instance.
        /// </summary>
        /// <returns>The instantiated <see cref="IRaycaster"/>.</returns>
        public static IRaycaster Create()
        {
            return s_Impl.Create();
        }

        /// <summary>
        /// Dispose a <see cref="IRaycaster"/> instance.
        /// </summary>
        /// <param name="raycaster">The <see cref="IRaycaster"/> to dispose.</param>
        public static void Dispose(IRaycaster raycaster)
        {
            s_Impl.Dispose(raycaster);
        }
    }
}
