namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A class that contains the message IDs which define the virtual camera protocol.
    /// </summary>
    static class VirtualCameraMessages
    {
        public static class ToClient
        {
            const string k_Base = "VirtualCamera_ToClient_";

            public const string ChannelFlags = k_Base + "ChannelFlags";
            public const string FocalLength = k_Base + "FocalLength";
            public const string FocusDistance = k_Base + "FocusDistance";
            public const string Aperture = k_Base + "Aperture";
            public const string SensorSize = k_Base + "SensorSize";
            public const string Iso = k_Base + "Iso";
            public const string ShutterSpeed = k_Base + "ShutterSpeed";
            public const string DampingEnabled = k_Base + "DampingEnabled";
            public const string BodyDamping = k_Base + "BodyDamping";
            public const string AimDamping = k_Base + "AimDamping";
            public const string FocalLengthDamping = k_Base + "FocalLengthDamping";
            public const string FocusDistanceDamping = k_Base + "FocusDistanceDamping";
            public const string ApertureDamping = k_Base + "ApertureDamping";
            public const string PositionLock = k_Base + "PositionLock";
            public const string RotationLock = k_Base + "RotationLock";
            public const string AutoHorizon = k_Base + "AutoHorizon";
            public const string ErgonomicTilt = k_Base + "ErgonomicTilt";
            public const string Rebasing = k_Base + "Rebasing";
            public const string MotionScale = k_Base + "MotionScale";
            public const string JoystickSensitivity = k_Base + "JoystickSensitivity";
            public const string PedestalSpace = k_Base + "PedestalSpace";
            public const string MotionSpace = k_Base + "MotionSpace";
            public const string FocusMode = k_Base + "FocusMode";
            public const string FocusReticlePosition = k_Base + "FocusReticlePosition";
            public const string FocusDistanceOffset = k_Base + "FocusDistanceOffset";
            public const string CropAspect = k_Base + "CropAspect";
            public const string GateFit = k_Base + "GateFit";
            public const string ShowGateMask = k_Base + "ShowGateMask";
            public const string ShowFrameLines = k_Base + "ShowFrameLines";
            public const string ShowCenterMarker = k_Base + "ShowCenterMarker";
            public const string ShowFocusPlane = k_Base + "ShowFocusPlane";
            public const string VideoStreamIsRunning = k_Base + "VideoStreamIsRunning";
            public const string VideoStreamPort = k_Base + "VideoStreamPort";
            public const string LensKitDescriptor_V0 = k_Base + "LensKitDescriptor_V0";
            public const string SelectedLensAsset = k_Base + "SelectedLensAsset";
            public const string SnapshotListDescriptor_V0 = k_Base + "SnapshotListDescriptor_V0";
            public const string VcamTrackMetadataListDescriptor_V0 = k_Base + "VirtualCameraTrackMetadataListDescriptor_V0";
        }

        public static class ToServer
        {
            const string k_Base = "VirtualCamera_ToServer_";

            public const string ChannelFlags = k_Base + "ChannelFlags";
            public const string JoysticksSample_V0 = k_Base + "JoysticksSample_V0";
            public const string GamepadSample_V0 = k_Base + "GamepadSample_V0";
            public const string PoseSample_V0 = k_Base + "PoseSample_V0";
            public const string PoseSample_V1 = k_Base + "PoseSample_V1";
            public const string FocalLengthSample_V0 = k_Base + "FocalLengthSample_V0";
            public const string FocalLengthSample_V1 = k_Base + "FocalLengthSample_V1";
            public const string FocusDistanceSample_V0 = k_Base + "FocusDistanceSample_V0";
            public const string FocusDistanceSample_V1 = k_Base + "FocusDistanceSample_V1";
            public const string ApertureSample_V0 = k_Base + "ApertureSample_V0";
            public const string ApertureSample_V1 = k_Base + "ApertureSample_V1";
            public const string InputSample_V0 = k_Base + "InputSample_V0";
            public const string DampingEnabled = k_Base + "DampingEnabled";
            public const string BodyDamping = k_Base + "BodyDamping";
            public const string AimDamping = k_Base + "AimDamping";
            public const string FocalLengthDamping = k_Base + "FocalLengthDamping";
            public const string FocusDistanceDamping = k_Base + "FocusDistanceDamping";
            public const string ApertureDamping = k_Base + "ApertureDamping";
            public const string PositionLock = k_Base + "PositionLock";
            public const string RotationLock = k_Base + "RotationLock";
            public const string AutoHorizon = k_Base + "AutoHorizon";
            public const string ErgonomicTilt = k_Base + "ErgonomicTilt";
            public const string Rebasing = k_Base + "Rebasing";
            public const string MotionScale = k_Base + "MotionScale";
            public const string JoystickSensitivity = k_Base + "JoystickSensitivity";
            public const string PedestalSpace = k_Base + "PedestalSpace";
            public const string MotionSpace = k_Base + "MotionSpace";
            public const string FocusMode = k_Base + "FocusMode";
            public const string FocusReticlePosition = k_Base + "FocusReticlePosition";
            public const string FocusDistanceOffset = k_Base + "FocusDistanceOffset";
            public const string CropAspect = k_Base + "CropAspect";
            public const string GateFit = k_Base + "GateFit";
            public const string ShowGateMask = k_Base + "ShowGateMask";
            public const string ShowFrameLines = k_Base + "ShowFrameLines";
            public const string ShowCenterMarker = k_Base + "ShowCenterMarker";
            public const string ShowFocusPlane = k_Base + "ShowFocusPlane";
            public const string SetPoseToOrigin = k_Base + "SetPoseToOrigin";
            public const string SetLensAsset = k_Base + "SetLensAsset";
            public const string TakeSnapshot = k_Base + "TakeSnapshot";
            public const string GoToSnapshot = k_Base + "GoToSnapshot";
            public const string LoadSnapshot = k_Base + "LoadSnapshot";
            public const string DeleteSnapshot = k_Base + "DeleteSnapshot";
        }
    }
}
