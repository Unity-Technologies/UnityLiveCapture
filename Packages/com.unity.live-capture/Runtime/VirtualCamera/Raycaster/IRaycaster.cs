using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Provides a mechanism to get the distance of objects rendered in a camera.
    /// </summary>
    interface IRaycaster
    {
        /// <summary>
        /// Get the distance from the camera to the object rendered at the normalized screen coordinates.
        /// </summary>
        /// <param name="camera">The camera to perform the raycast from.</param>
        /// <param name="normalizedPosition">The screen-space normalized position of the reticle.</param>
        /// <param name="distance">The computed distance from the camera.</param>
        /// <returns>True if the distance can be computed. False otherwise</returns>
        bool Raycast(Camera camera, Vector2 normalizedPosition, out float distance);

        /// <summary>
        /// Get the distance from the camera to the object rendered at the normalized screen coordinates, as well as a reference to the object.
        /// </summary>
        /// <param name="camera">The camera to perform the raycast from.</param>
        /// <param name="normalizedPosition">The screen-space normalized position of the reticle.</param>
        /// <param name="ray">The world space ray used for raycasting.</param>
        /// <param name="gameObject">Scene object hit by the ray, if any.</param>
        /// <param name="hit">Raycast hit data, if any.</param>
        /// <returns>True if the distance can be computed. False otherwise</returns>
        bool Raycast(Camera camera, Vector2 normalizedPosition, out Ray ray, out GameObject gameObject, out RaycastHit hit);
    }
}
