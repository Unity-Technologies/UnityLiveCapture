using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Interface that describes a render pipeline specific focus plane implementation.
    /// </summary>
    interface IFocusPlaneImpl
    {
        /// <summary>
        /// Set the camera to render the focus plane on.
        /// </summary>
        void SetCamera(Camera camera);

        /// <summary>
        /// Initializes the implementation, allocating resources such as materials.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Disposes the implementation, releasing all resources, including materials and render target.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Updates the implementation.
        /// </summary>
        void Update();

        /// <summary>
        /// Provides access to a render target of the specified type, if supported.
        /// </summary>
        /// <typeparam name="T">The render target.</typeparam>
        /// <returns>Indicates whether or not a render target of the specified type is supported.</returns>
        bool TryGetRenderTarget<T>(out T target);

        /// <summary>
        /// Allocates (or re-allocates) the intermediary render target in which the focus plane is rendered.
        /// </summary>
        /// <remarks>
        /// This render texture is allocated in a deferred manner for one needs to wait for the render pipeline to be initialized.
        /// </remarks>
        /// <returns>True if an allocation occured.</returns>
        bool AllocateTargetIfNeeded(int width, int height);
    }
}
