using System;
using UnityEngine;
using UnityEngine.Rendering;

#if HDRP_14_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;

#endif
#if URP_14_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.LiveCapture.VirtualCamera.Raycasting
{
    /// <summary>
    /// Conducts raycasts by rendering the scene depth to a camera to ensure the
    /// raycast matches how the scene visually appears.
    /// </summary>
    class GraphicsRaycaster : IDisposable
    {
        const float k_MinNearPlane = 0.01f;

        IRaycasterImpl m_Impl;

        /// <summary>
        /// Instantiate a new <see cref="GraphicsRaycaster"/> instance.
        /// </summary>
        /// <remarks>
        /// The implementation will be selected based on the currently active render pipeline.
        /// </remarks>
        public GraphicsRaycaster()
        {
#if HDRP_14_0_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset is HDRenderPipelineAsset)
            {
                m_Impl = new HighDefinitionRaycasterImpl();
            }
#endif
#if URP_14_0_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset)
            {
                m_Impl = new UniversalRaycasterImpl();
            }
#endif

            // Default to legacy render pipeline.
            if (m_Impl == null)
            {
                m_Impl = new LegacyRaycasterImpl();
            }

            m_Impl.Initialize();
        }

        /// <summary>
        /// Deinitialize the raycaster, resources are freed here.
        /// </summary>
        public void Dispose()
        {
            m_Impl.Dispose();
            m_Impl = null;
        }

        /// <summary>
        /// Do a raycast test against the scene graphics. This is done using a depth buffer.
        /// </summary>
        /// <param name="camera">The camera whose transform and projection parameters will determine the ray to be casted.</param>
        /// <param name="screenPosition">The ray goes through the screen position on the near clip plane.</param>
        /// <param name="hit">Details about the hit point if the ray hit something.</param>
        /// <param name="layerMask">The layers of scene geometry to test against.</param>
        /// <returns>True if the ray intersected something; false otherwise.</returns>
        public bool Raycast(Camera camera, Vector2 screenPosition, out RaycastHit hit, int layerMask = -1)
        {
            var ray = camera.ScreenPointToRay(screenPosition);
            return Raycast(ray.origin, ray.direction, out hit, camera.nearClipPlane, camera.farClipPlane, layerMask);
        }

        /// <summary>
        /// Do a raycast test against the scene graphics. This is done using a depth buffer, so be sure that
        /// the <paramref name="minDistance"/> and <paramref name="maxDistance"/> define as small of a range
        /// as is needed to increase precision.
        /// </summary>
        /// <param name="origin">The origin of the ray to test.</param>
        /// <param name="direction">The direction of the ray to test.</param>
        /// <param name="hit">Details about the hit point if the ray hit something.</param>
        /// <param name="minDistance">The closest distance a hit can be detected at.</param>
        /// <param name="maxDistance">The furthest distance a hit can be detected at.</param>
        /// <param name="layerMask">The layers of scene geometry to test against.</param>
        /// <returns>True if the ray intersected something; false otherwise.</returns>
        public bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float minDistance = k_MinNearPlane, float maxDistance = 1000f, int layerMask = -1)
        {
            Validate(minDistance, maxDistance);
            return m_Impl.Raycast(origin, direction, out hit, minDistance, maxDistance, layerMask);
        }

        /// <summary>
        /// Do a raycast test against the scene graphics. This is done using a color buffer where object ids are encoded and a depth buffer.
        /// </summary>
        /// <param name="camera">The camera whose transform and projection parameters will determine the ray to be casted.</param>
        /// <param name="screenPosition">The ray goes through the screen position on the near clip plane.</param>
        /// <param name="hit">Details about the hit point if the raycast hit something.</param>
        /// <param name="gameObject">Reference to the object being hit if any.</param>
        /// <param name="layerMask">The layers of scene geometry to test against.</param>
        /// <returns>True if the ray intersected something; false otherwise.</returns>
        public bool Raycast(Camera camera, Vector2 screenPosition, out RaycastHit hit, out GameObject gameObject, int layerMask = -1)
        {
            var ray = camera.ScreenPointToRay(screenPosition);
            return Raycast(ray.origin, ray.direction, out hit, out gameObject, camera.nearClipPlane, camera.farClipPlane, layerMask);
        }

        /// <summary>
        /// Do a raycast test against the scene graphics. This is done using a color buffer where object ids are encoded and a depth buffer, so be sure that
        /// the <paramref name="minDistance"/> and <paramref name="maxDistance"/> define as small of a range
        /// as is needed to increase precision.
        /// </summary>
        /// <param name="origin">The origin of the ray to test.</param>
        /// <param name="direction">The direction of the ray to test.</param>
        /// <param name="hit">Details about the hit point if the raycast hit something.</param>
        /// <param name="gameObject">Reference to the object being hit if any.</param>
        /// <param name="minDistance">The closest distance a hit can be detected at.</param>
        /// <param name="maxDistance">The furthest distance a hit can be detected at.</param>
        /// <param name="layerMask">The layers of scene geometry to test against.</param>
        /// <returns>True if the ray intersected something; false otherwise.</returns>
        public bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, out GameObject gameObject, float minDistance = k_MinNearPlane, float maxDistance = 1000f, int layerMask = -1)
        {
            Validate(minDistance, maxDistance);
            return m_Impl.Raycast(origin, direction, out hit, out gameObject, minDistance, maxDistance, layerMask);
        }

        void Validate(float minDistance, float maxDistance)
        {
            if (minDistance < k_MinNearPlane)
                throw new ArgumentOutOfRangeException(nameof(minDistance), minDistance,
                    $"Value should be equal or superior to [{k_MinNearPlane}].");

            if (maxDistance < minDistance + Mathf.Epsilon)
                throw new ArgumentOutOfRangeException(nameof(maxDistance), maxDistance,
                    $"Value should be superior to {nameof(minDistance)}");
        }
    }
}
