using UnityEngine;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    class SettingsModel
    {
        public bool DampingEnabled { get; set; }
        public Vector3 BodyDamping { get; set; }
        public float AimDamping { get; set; }
        public PositionAxis PositionLock { get; set; }
        public RotationAxis RotationLock { get; set; }
        public bool AutoHorizon { get; set; }
        public float ErgonomicTilt { get; set; }
        public bool Rebasing { get; set; }
        public Vector3 MotionScale { get; set; }
        public Vector3 JoystickSensitivity { get; set; }
        public Space PedestalSpace { get; set; }
        public Space MotionSpace { get; set; }
        public float AspectRatio { get; set; }
        public FocusMode FocusMode { get; set; }
        public Vector2 ReticlePosition { get; set; }
        public float FocusDistanceOffset { get; set; }
        public float FocusDistanceDamping { get; set; }
        public float FocalLengthDamping { get; set; }
        public float ApertureDamping { get; set; }
        public GateFit GateFit { get; set; }
        public bool GateMask { get; set; }
        public bool AspectRatioLines { get; set; }
        public bool CenterMarker { get; set; }
        public bool FocusPlane { get; set; }
    }
}
