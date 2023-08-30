using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    class VanillaCameraDriverComponent : ICameraDriverComponent
    {
        /// <summary>
        /// The camera this driver component acts on.
        /// </summary>
        public Camera Camera { get; set; }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public void EnableDepthOfField(bool value) { }

        /// <inheritdoc/>
        public void SetDamping(Damping dampingData) { }

        /// <inheritdoc/>
        public void SetFocusDistance(float focusDistance) { }

        /// <inheritdoc/>
        public void SetPhysicalCameraProperties(Lens lens, LensIntrinsics intrinsics, CameraBody cameraBody)
        {
            Camera.usePhysicalProperties = true;
            Camera.sensorSize = cameraBody.SensorSize;
            Camera.lensShift = intrinsics.LensShift;
            Camera.focalLength = lens.FocalLength;
            Camera.aperture = lens.Aperture;
            Camera.iso = cameraBody.Iso;
            Camera.shutterSpeed = cameraBody.ShutterSpeed;
            Camera.anamorphism = intrinsics.Anamorphism;
            Camera.curvature = intrinsics.Curvature;
            Camera.barrelClipping = intrinsics.BarrelClipping;
            Camera.bladeCount = intrinsics.BladeCount;
        }
    }
}
