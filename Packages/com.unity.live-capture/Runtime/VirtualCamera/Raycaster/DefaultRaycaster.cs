using System;
using Unity.LiveCapture.VirtualCamera.Raycasting;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Raycaster that computes the distance between the object and the camera using the rendered depth.
    /// </summary>
    class DefaultRaycaster : IRaycaster
    {
        static readonly Vector2 k_Bounds = new Vector2(0.1f, 1000);

        GraphicsRaycaster m_GraphicsRaycaster;

        /// <summary>
        /// Creates an instance of DefaultRaycaster.
        /// </summary>
        /// <param name="graphicsRaycaster">The <see cref="GraphicsRaycaster"/> to use.</param>
        public DefaultRaycaster(GraphicsRaycaster graphicsRaycaster)
        {
            if (graphicsRaycaster == null)
            {
                throw new Exception("GraphicsRaycaster instance is null.");
            }

            m_GraphicsRaycaster = graphicsRaycaster;
        }

        /// <inheritdoc/>
        public bool Raycast(Camera camera, Vector2 normalizedPosition, out float distance)
        {
            if (CanPerformRaycast(camera, normalizedPosition, out var ray))
            {
                if (m_GraphicsRaycaster.Raycast(ray.origin, ray.direction, out var hit, k_Bounds.x, k_Bounds.y))
                {
                    distance = GetDistance(camera.transform, hit);
                    return true;
                }
            }

            distance = float.MaxValue;
            return false;
        }

        /// <inheritdoc/>
        public bool Raycast(Camera camera, Vector2 normalizedPosition, out Ray ray, out GameObject gameObject, out RaycastHit hit)
        {
            if (CanPerformRaycast(camera, normalizedPosition, out ray))
            {
                if (m_GraphicsRaycaster.Raycast(ray.origin, ray.direction, out hit, out gameObject, k_Bounds.x, k_Bounds.y))
                {
                    hit.distance = GetDistance(camera.transform, hit);
                    return true;
                }
            }

            gameObject = null;
            hit = default;
            return false;
        }

        bool CanPerformRaycast(Camera camera, Vector2 normalizedPosition, out Ray ray)
        {
            if (m_GraphicsRaycaster == null)
            {
                throw new Exception("GraphicsRaycaster instance has been destroyed.");
            }

            if (camera != null)
            {
                var screenPosition = new Vector2(camera.pixelWidth * normalizedPosition.x, camera.pixelHeight * normalizedPosition.y);
                ray = camera.ScreenPointToRay(screenPosition);
                return true;
            }

            ray = default;
            return false;
        }

        static float GetDistance(Transform cameraTransform, RaycastHit hit)
        {
            var hitVector = hit.point - cameraTransform.position;
            var depthVector = Vector3.Project(hitVector, cameraTransform.forward);
            return depthVector.magnitude;
        }
    }
}
