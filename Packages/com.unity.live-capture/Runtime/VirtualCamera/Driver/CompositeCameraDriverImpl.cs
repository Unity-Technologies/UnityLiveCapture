using System.Collections.Generic;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// The driver must support multiple render pipelines, and optionally, Cinemachine.
    /// We keep the induced complexity in check by having each pipeline/package's features
    /// living in independent components, who are then added/used depending on the project's configuration.
    /// </summary>
    class CompositeCameraDriverImpl : ICameraDriverImpl
    {
        IEnumerable<ICameraDriverComponent> m_Components;

        public CompositeCameraDriverImpl(IEnumerable<ICameraDriverComponent> components)
        {
            m_Components = new List<ICameraDriverComponent>(components);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var component in m_Components)
                component.Dispose();
        }

        /// <inheritdoc/>
        public void EnableDepthOfField(bool value)
        {
            foreach (var component in m_Components)
                component.EnableDepthOfField(value);
        }

        /// <inheritdoc/>
        public void SetFocusDistance(float focusDistance)
        {
            foreach (var component in m_Components)
                component.SetFocusDistance(focusDistance);
        }

        /// <inheritdoc/>
        public void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
            foreach (var component in m_Components)
                component.SetPhysicalCameraProperties(lens, intrinsics, cameraBody);
        }
    }
}
