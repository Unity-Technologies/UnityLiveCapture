using System;

namespace Unity.LiveCapture.ARKitFaceCapture
{
    /// <summary>
    /// A class used to store data needed for specific mapper implementations in a <see cref="FaceActor"/>.
    /// </summary>
    /// <remarks>
    /// The cache can be used to store references to transforms and renderers on an actor, preventing
    /// the need to constantly reacquire the references in the mapper.
    /// </remarks>
    public abstract class FaceMapperCache : IDisposable
    {
        /// <summary>
        /// Has this cache been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// The finalizer that attempts to dispose the cache if it is not properly disposed.
        /// </summary>
        ~FaceMapperCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the cache.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            OnDispose(disposing);

            Disposed = true;
        }

        /// <summary>
        /// Frees the resources held by the cache.
        /// </summary>
        /// <param name="disposing">True when dispose was called; false when invoked by the finalizer.</param>
        protected virtual void OnDispose(bool disposing) { }
    }
}
