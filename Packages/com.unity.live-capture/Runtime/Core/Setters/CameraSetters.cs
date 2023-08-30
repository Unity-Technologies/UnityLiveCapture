using UnityEngine;

namespace Unity.LiveCapture.Setters
{
    sealed class CameraFieldOfViewSetter : Setter<Camera, float>
    {
        public override string PropertyName => "field of view";
        public override void Set(Camera camera, in float value) => camera.fieldOfView = value;
    }

    sealed class CameraFocalLengthSetter : Setter<Camera, float>
    {
        public override string PropertyName => "m_FocalLength";
        public override void Set(Camera camera, in float value) => camera.focalLength = value;
    }

    sealed class CameraFocusDistanceSetter : Setter<Camera, float>
    {
        public override string PropertyName => "m_FocusDistance";
        public override void Set(Camera camera, in float value) => camera.focusDistance = value;
    }

    sealed class CameraApertureSetter : Setter<Camera, float>
    {
        public override string PropertyName => "m_Aperture";
        public override void Set(Camera camera, in float value) => camera.aperture = value;
    }

    sealed class CameraShutterSpeedSetter : Setter<Camera, float>
    {
        public override string PropertyName => "m_ShutterSpeed";
        public override void Set(Camera camera, in float value) => camera.shutterSpeed = value;
    }

    sealed class CameraIsoSetter : Setter<Camera, int>
    {
        public override string PropertyName => "m_Iso";
        public override void Set(Camera camera, in int value) => camera.iso = value;
    }

    sealed class CameraUsePhysicalPropertiesSetter : Setter<Camera, bool>
    {
        public override string PropertyName => "m_UsePhysicalProperties";
        public override void Set(Camera camera, in bool value) => camera.usePhysicalProperties = value;
    }

    sealed class CameraBladeCountSetter : Setter<Camera, int>
    {
        public override string PropertyName => "m_BladeCount";
        public override void Set(Camera camera, in int value) => camera.bladeCount = value;
    }

    sealed class CameraCurvatureSetter : Setter<Camera, Vector2>
    {
        public override string PropertyName => "m_Curvature";
        public override void Set(Camera camera, in Vector2 value) => camera.curvature = value;
    }

    sealed class CameraBarrelClippingSetter : Setter<Camera, float>
    {
        public override string PropertyName => "m_BarrelClipping";
        public override void Set(Camera camera, in float value) => camera.barrelClipping = value;
    }

    sealed class CameraAnamorphismSetter : Setter<Camera, float>
    {
        public override string PropertyName => "m_Anamorphism";
        public override void Set(Camera camera, in float value) => camera.anamorphism = value;
    }

    sealed class CameraLensShiftSetter : Setter<Camera, Vector2>
    {
        public override string PropertyName => "m_LensShift";
        public override void Set(Camera camera, in Vector2 value) => camera.lensShift = value;
    }

    sealed class CameraSensorSizeSetter : Setter<Camera, Vector2>
    {
        public override string PropertyName => "m_SensorSize";
        public override void Set(Camera camera, in Vector2 value) => camera.sensorSize = value;
    }
}
