using UnityEngine;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    // TODO do we need None?
    enum HostMessageType
    {
        DampingEnabled,
        BodyDamping,
        AimDamping,
        FocalLengthDamping,
        FocusDistanceDamping,
        ApertureDamping,
        PositionLock,
        RotationLock,
        AutoHorizon,
        ErgonomicTilt,
        Rebasing,
        MotionScale,
        JoystickSensitivity,
        PedestalSpace,
        MotionSpace,
        FocusMode,
        FocusReticlePosition,
        FocusDistanceOffset,
        CropAspect,
        ShowGateMask,
        ShowFrameLines,
        ShowCenterMarker,
        ShowFocusPlane,
        SetPoseToOrigin,
        SensorSize,
        Iso,
        ShutterSpeed,
        FocalLength,
        FocusDistance,
        Aperture,
        LensIntrinsics,
        ChannelFlags
    }

    class HostSignal
    {
        public HostMessageType Type;
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public double DoubleValue;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public FocusMode FocusModeValue;
        public PositionAxis PositionAxisValue;
        public RotationAxis RotationAxisValue;
        public Space SpaceValue;
    }

    // TODO pool instances?
    class SendHostSignal : HostSignal
    {
    }

    class ReceiveHostSignal : Signal<HostMessageType>
    {
    }
}
