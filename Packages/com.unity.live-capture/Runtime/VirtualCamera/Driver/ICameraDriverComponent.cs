using System;

namespace Unity.LiveCapture.VirtualCamera
{
    interface ICameraDriverComponent : IDisposable
    {
        /// <summary>
        /// Control Depth Of Field activation.
        /// </summary>
        void EnableDepthOfField(bool value);

        /// <summary>
        /// Set damping data.
        /// </summary>
        void SetDamping(Damping dampingData);

        /// <summary>
        /// Set Focus distance.
        /// </summary>
        void SetFocusDistance(float focusDistance);

        /// <summary>
        /// Set Camera physical properties.
        /// </summary>
        void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody);
    }
}
