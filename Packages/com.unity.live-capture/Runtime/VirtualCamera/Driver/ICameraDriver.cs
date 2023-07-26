using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Interface expected by the <see cref="VirtualCameraDevice"/>
    /// </summary>
    interface ICameraDriver
    {
        /// <summary>
        /// Provides access to the currently rendering Camera.
        /// </summary>
        /// <remarks>
        /// Access to the currently rendering Camera is needed for video streaming for instance.
        /// </remarks>
        /// <returns>
        /// The current rendering Camera.
        /// </returns>
        Camera GetCamera();
    }
}
