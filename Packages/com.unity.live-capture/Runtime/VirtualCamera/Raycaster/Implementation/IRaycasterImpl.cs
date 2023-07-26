using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    interface IRaycasterImpl
    {
        /// <summary>
        /// Initialize the raycaster, allocate resources.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Dispose the raycaster, free resources.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Do a raycast test against the scene graphics.
        /// </summary>
        /// <param name="origin">The origin of the ray to test.</param>
        /// <param name="direction">The direction of the ray to test.</param>
        /// <param name="hit">Details about the hit point if the ray hit something.</param>
        /// <param name="minDistance">The closest distance a hit can be detected at.</param>
        /// <param name="maxDistance">The furthest distance a hit can be detected at.</param>
        /// <param name="layerMask">The layers of scene geometry to test against.</param>
        /// <returns>True if the ray intersected something; false otherwise.</returns>
        bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float minDistance, float maxDistance, int layerMask);

        /// <summary>
        /// Do a raycast test against the scene graphics.
        /// </summary>
        /// <param name="origin">The origin of the ray to test.</param>
        /// <param name="direction">The direction of the ray to test.</param>
        /// <param name="hit">Details about the hit point if the raycast hit something.</param>
        /// <param name="gameObject">Reference to the object being hit if any.</param>
        /// <param name="minDistance">The closest distance a hit can be detected at.</param>
        /// <param name="maxDistance">The furthest distance a hit can be detected at.</param>
        /// <param name="layerMask">The layers of scene geometry to test against.</param>
        /// <returns>True if the ray intersected something; false otherwise.</returns>
        bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, out GameObject gameObject, float minDistance, float maxDistance, int layerMask);
    }
}
