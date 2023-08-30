using System;

namespace Unity.LiveCapture.VirtualCamera
{
    interface ICameraDriverImpl : IDisposable
    {
        /// <summary>
        /// Activates the depth of field effect.
        /// </summary>
        /// <param name="value">The active state to set.</param>
        void EnableDepthOfField(bool value);

        /// <summary>
        /// Sets the focus distance.
        /// </summary>
        /// <param name="focusDistance">The focus distance in world units.</param>
        /// <remarks>
        /// Depth Of Field needs to be active for focus distance to take effect, <see cref="EnableDepthOfField"/>
        /// </remarks>
        void SetFocusDistance(float focusDistance);

        /// <summary>
        /// Set physical camera properties, such as lens intrinsics, focus mode, etc.
        /// </summary>
        /// <param name="lens">Lens data.</param>
        /// <param name="intrinsics">Lens intrinsic parameters.</param>
        /// <param name="cameraBody">Camera body data.</param>
        void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody);
    }
}
