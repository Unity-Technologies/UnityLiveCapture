using System;
using System.IO;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Protocols;
using Unity.LiveCapture.VirtualCamera.Networking;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A class used to communicate with a virtual camera device in the Unity editor from the companion app.
    /// </summary>
    class VirtualCameraHost : CompanionAppHost
    {
        readonly BinarySender<VirtualCameraChannelFlags> m_ChannelFlagsSender;
        readonly BinarySender<InputSampleV0> m_InputSender;
        readonly BinarySender<JoysticksSampleV0> m_JoysticksSender;
        readonly BinarySender<GamepadSampleV0> m_GamepadSender;
        readonly BoolSender m_DampingEnabledSender;
        readonly BinarySender<Vector3> m_BodyDampingSender;
        readonly BinarySender<float> m_AimDampingSender;
        readonly BinarySender<float> m_FocalLengthDampingSender;
        readonly BinarySender<float> m_FocusDistanceDampingSender;
        readonly BinarySender<float> m_ApertureDampingSender;
        readonly BinarySender<PositionAxis> m_PositionLockSender;
        readonly BinarySender<RotationAxis> m_RotationLockSender;
        readonly BoolSender m_AutoHorizonSender;
        readonly BinarySender<float> m_ErgonomicTiltSender;
        readonly BoolSender m_RebasingSender;
        readonly BinarySender<Vector3> m_MotionScaleSender;
        readonly BinarySender<Vector3> m_JoystickSensitivitySender;
        readonly BinarySender<Space> m_PedestalSpaceSender;
        readonly BinarySender<Space> m_MotionSpaceSender;
        readonly BinarySender<FocusMode> m_FocusModeSender;
        readonly BinarySender<Vector2> m_FocusReticlePositionSender;
        readonly BinarySender<float> m_FocusDistanceOffsetSender;
        readonly BinarySender<float> m_CropAspectSender;
        readonly BinarySender<GateFit> m_GateFitSender;
        readonly BoolSender m_ShowGateMaskSender;
        readonly BoolSender m_ShowFrameLinesSender;
        readonly BoolSender m_ShowCenterMarkerSender;
        readonly BoolSender m_ShowFocusPlaneSender;
        readonly EventSender m_PoseToOriginSender;
        readonly BinarySender<SerializableGuid> m_SetLensAssetSender;
        readonly EventSender m_TakeSnapshotSender;
        readonly BinarySender<int> m_GoToSnapshotSender;
        readonly BinarySender<int> m_LoadSnapshotSender;
        readonly BinarySender<int> m_DeleteSnapshotSender;

        public event Action<VirtualCameraChannelFlags> ChannelFlagsReceived;
        public event Action<float> FocalLengthReceived;
        public event Action<float> FocusDistanceReceived;
        public event Action<float> ApertureReceived;
        public event Action<Vector2> SensorSizeReceived;
        public event Action<int> IsoReceived;
        public event Action<float> ShutterSpeedReceived;
        public event Action<bool> DampingEnabledReceived;
        public event Action<Vector3> BodyDampingReceived;
        public event Action<float> AimDampingReceived;
        public event Action<float> FocalLengthDampingReceived;
        public event Action<float> FocusDistanceDampingReceived;
        public event Action<float> ApertureDampingReceived;
        public event Action<PositionAxis> PositionLockReceived;
        public event Action<RotationAxis> RotationLockReceived;
        public event Action<bool> AutoHorizonReceived;
        public event Action<float> ErgonomicTiltReceived;
        public event Action<bool> RebasingReceived;
        public event Action<Vector3> MotionScaleReceived;
        public event Action<Vector3> JoystickSensitivityReceived;
        public event Action<Space> PedestalSpaceReceived;
        public event Action<Space> MotionSpaceReceived;
        public event Action<FocusMode> FocusModeReceived;
        public event Action<Vector2> FocusReticlePositionReceived;
        public event Action<float> FocusDistanceOffsetReceived;
        public event Action<float> CropAspectReceived;
        public event Action<bool> ShowGateMaskReceived;
        public event Action<bool> ShowFrameLinesReceived;
        public event Action<bool> ShowCenterMarkerReceived;
        public event Action<bool> ShowFocusPlaneReceived;
        public event Action<bool> VideoStreamIsRunningReceived;
        public event Action<int> VideoStreamPortReceived;
        public event Action<LensKitDescriptor> LensKitDescriptorReceived;
        public event Action<int> SelectedLensAssetReceived;
        public event Action<SnapshotListDescriptor> SnapshotListDescriptorReceived;
        public event Action<VcamTrackMetadataListDescriptor> VcamTrackMetadataListDescriptorReceived;

        readonly Action<PoseSample> m_SendPoseImpl;
        readonly Action<FocalLengthSample> m_SendFocalLengthImpl;
        readonly Action<FocusDistanceSample> m_SendFocusDistanceImpl;
        readonly Action<ApertureSample> m_SendApertureImpl;

        /// <inheritdoc />
        public VirtualCameraHost(NetworkBase network, Remote remote, Stream stream)
            : base(network, remote, stream)
        {
            BinarySender<VirtualCameraChannelFlags>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ChannelFlags, out m_ChannelFlagsSender);
            BinarySender<InputSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.InputSample_V0, out m_InputSender);
            BinarySender<JoysticksSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.JoysticksSample_V0, out m_JoysticksSender);
            BinarySender<GamepadSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.GamepadSample_V0, out m_GamepadSender);

            // Pick the right data format based on the server's protocols. We strive to support older server versions (up to a point)
            // This seems repetitive. We should come up with a generic way to instantiate the right sender type from the protocol (may require reflection)
            if (BinarySender<PoseSampleV1>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.PoseSample_V1, out var poseSenderV1))
            {
                m_SendPoseImpl = sample => poseSenderV1.Send((PoseSampleV1)sample);
            }
            else if (BinarySender<PoseSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.PoseSample_V0, out var poseSenderV0))
            {
                m_SendPoseImpl = sample => poseSenderV0.Send((PoseSampleV0)sample);
            }

            if (BinarySender<FocalLengthSampleV1>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocalLengthSample_V1, out var focalLengthSenderV1))
            {
                m_SendFocalLengthImpl = sample => focalLengthSenderV1.Send((FocalLengthSampleV1)sample);
            }
            else if (BinarySender<FocalLengthSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocalLengthSample_V0, out var focalLengthSenderV0))
            {
                m_SendFocalLengthImpl = sample => focalLengthSenderV0.Send((FocalLengthSampleV0)sample);
            }

            if (BinarySender<FocusDistanceSampleV1>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocusDistanceSample_V1, out var focusDistanceSenderV1))
            {
                m_SendFocusDistanceImpl = sample => focusDistanceSenderV1.Send((FocusDistanceSampleV1)sample);
            }
            else if (BinarySender<FocusDistanceSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocusDistanceSample_V0, out var focusDistanceSenderV0))
            {
                m_SendFocusDistanceImpl = sample => focusDistanceSenderV0.Send((FocusDistanceSampleV0)sample);
            }

            if (BinarySender<ApertureSampleV1>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ApertureSample_V1, out var apertureSenderV1))
            {
                m_SendApertureImpl = sample => apertureSenderV1.Send((ApertureSampleV1)sample);
            }
            else if (BinarySender<ApertureSampleV0>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ApertureSample_V0, out var apertureSenderV0))
            {
                m_SendApertureImpl = sample => apertureSenderV0.Send((ApertureSampleV0)sample);
            }

            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.DampingEnabled, out m_DampingEnabledSender);
            BinarySender<Vector3>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.BodyDamping, out m_BodyDampingSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.AimDamping, out m_AimDampingSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocalLengthDamping, out m_FocalLengthDampingSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocusDistanceDamping, out m_FocusDistanceDampingSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ApertureDamping, out m_ApertureDampingSender);
            BinarySender<PositionAxis>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.PositionLock, out m_PositionLockSender);
            BinarySender<RotationAxis>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.RotationLock, out m_RotationLockSender);
            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.AutoHorizon, out m_AutoHorizonSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ErgonomicTilt, out m_ErgonomicTiltSender);
            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.Rebasing, out m_RebasingSender);
            BinarySender<Vector3>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.MotionScale, out m_MotionScaleSender);
            BinarySender<Vector3>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.JoystickSensitivity, out m_JoystickSensitivitySender);
            BinarySender<Space>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.PedestalSpace, out m_PedestalSpaceSender);
            BinarySender<Space>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.MotionSpace, out m_MotionSpaceSender);
            BinarySender<FocusMode>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocusMode, out m_FocusModeSender);
            BinarySender<Vector2>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocusReticlePosition, out m_FocusReticlePositionSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.FocusDistanceOffset, out m_FocusDistanceOffsetSender);
            BinarySender<float>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.CropAspect, out m_CropAspectSender);
            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ShowGateMask, out m_ShowGateMaskSender);
            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ShowFrameLines, out m_ShowFrameLinesSender);
            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ShowCenterMarker, out m_ShowCenterMarkerSender);
            BoolSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.ShowFocusPlane, out m_ShowFocusPlaneSender);
            EventSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.SetPoseToOrigin, out m_PoseToOriginSender);
            BinarySender<SerializableGuid>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.SetLensAsset, out m_SetLensAssetSender);
            EventSender.TryGet(m_Protocol, VirtualCameraMessages.ToServer.TakeSnapshot, out m_TakeSnapshotSender);
            BinarySender<int>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.GoToSnapshot, out m_GoToSnapshotSender);
            BinarySender<int>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.LoadSnapshot, out m_LoadSnapshotSender);
            BinarySender<int>.TryGet(m_Protocol, VirtualCameraMessages.ToServer.DeleteSnapshot, out m_DeleteSnapshotSender);

            if (BinaryReceiver<VirtualCameraChannelFlags>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ChannelFlags, out var channelFlagsChanged))
            {
                channelFlagsChanged.AddHandler(flags =>
                {
                    ChannelFlagsReceived?.Invoke(flags);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocalLength, out var focalLengthChanged))
            {
                focalLengthChanged.AddHandler(focalLength =>
                {
                    FocalLengthReceived?.Invoke(focalLength);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocusDistance, out var focusDistanceChanged))
            {
                focusDistanceChanged.AddHandler(focusDistance =>
                {
                    FocusDistanceReceived?.Invoke(focusDistance);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.Aperture, out var apertureChanged))
            {
                apertureChanged.AddHandler(aperture =>
                {
                    ApertureReceived?.Invoke(aperture);
                });
            }
            if (BinaryReceiver<Vector2>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.SensorSize, out var sensorSizeChanged))
            {
                sensorSizeChanged.AddHandler(sensorSize =>
                {
                    SensorSizeReceived?.Invoke(sensorSize);
                });
            }
            if (BinaryReceiver<int>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.Iso, out var isoChanged))
            {
                isoChanged.AddHandler(iso =>
                {
                    IsoReceived?.Invoke(iso);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ShutterSpeed, out var shutterSpeedChanged))
            {
                shutterSpeedChanged.AddHandler(shutterSpeed =>
                {
                    ShutterSpeedReceived?.Invoke(shutterSpeed);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.DampingEnabled, out var dampingEnabledChanged))
            {
                dampingEnabledChanged.AddHandler(dampingEnabled =>
                {
                    DampingEnabledReceived?.Invoke(dampingEnabled);
                });
            }
            if (BinaryReceiver<Vector3>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.BodyDamping, out var bodyDampingChanged))
            {
                bodyDampingChanged.AddHandler(damping =>
                {
                    BodyDampingReceived?.Invoke(damping);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.AimDamping, out var aimDampingChanged))
            {
                aimDampingChanged.AddHandler(damping =>
                {
                    AimDampingReceived?.Invoke(damping);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocalLengthDamping, out var focalLengthDampingChanged))
            {
                focalLengthDampingChanged.AddHandler(damping =>
                {
                    FocalLengthDampingReceived?.Invoke(damping);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocusDistanceDamping, out var focusDistanceDamping))
            {
                focusDistanceDamping.AddHandler(damping =>
                {
                    FocusDistanceDampingReceived?.Invoke(damping);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ApertureDamping, out var apertureDampingChanged))
            {
                apertureDampingChanged.AddHandler(damping =>
                {
                    ApertureDampingReceived?.Invoke(damping);
                });
            }
            if (BinaryReceiver<PositionAxis>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.PositionLock, out var positionLockChanged))
            {
                positionLockChanged.AddHandler(positionLock =>
                {
                    PositionLockReceived?.Invoke(positionLock);
                });
            }
            if (BinaryReceiver<RotationAxis>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.RotationLock, out var rotationLockChanged))
            {
                rotationLockChanged.AddHandler(rotationLock =>
                {
                    RotationLockReceived?.Invoke(rotationLock);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.AutoHorizon, out var autoHorizonChanged))
            {
                autoHorizonChanged.AddHandler(autoHorizon =>
                {
                    AutoHorizonReceived?.Invoke(autoHorizon);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ErgonomicTilt, out var ergonomicTiltChanged))
            {
                ergonomicTiltChanged.AddHandler(ergonomicTilt =>
                {
                    ErgonomicTiltReceived?.Invoke(ergonomicTilt);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.Rebasing, out var rebasingChanged))
            {
                rebasingChanged.AddHandler(rebasing =>
                {
                    RebasingReceived?.Invoke(rebasing);
                });
            }
            if (BinaryReceiver<Vector3>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.MotionScale, out var motionScaleChanged))
            {
                motionScaleChanged.AddHandler(motionScale =>
                {
                    MotionScaleReceived?.Invoke(motionScale);
                });
            }
            if (BinaryReceiver<Vector3>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.JoystickSensitivity, out var joystickSensitivityChanged))
            {
                joystickSensitivityChanged.AddHandler(sensitivity =>
                {
                    JoystickSensitivityReceived?.Invoke(sensitivity);
                });
            }
            if (BinaryReceiver<Space>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.PedestalSpace, out var pedestalSpaceChanged))
            {
                pedestalSpaceChanged.AddHandler(space =>
                {
                    PedestalSpaceReceived?.Invoke(space);
                });
            }
            if (BinaryReceiver<Space>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.MotionSpace, out var motionSpaceChanged))
            {
                motionSpaceChanged.AddHandler(space =>
                {
                    MotionSpaceReceived?.Invoke(space);
                });
            }
            if (BinaryReceiver<FocusMode>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocusMode, out var focusModeChanged))
            {
                focusModeChanged.AddHandler(focusMode =>
                {
                    FocusModeReceived?.Invoke(focusMode);
                });
            }
            if (BinaryReceiver<Vector2>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocusReticlePosition, out var focusReticlePositionChanged))
            {
                focusReticlePositionChanged.AddHandler(focusReticlePosition =>
                {
                    FocusReticlePositionReceived?.Invoke(focusReticlePosition);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.FocusDistanceOffset, out var focusDistanceOffsetChanged))
            {
                focusDistanceOffsetChanged.AddHandler(offset =>
                {
                    FocusDistanceOffsetReceived?.Invoke(offset);
                });
            }
            if (BinaryReceiver<float>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.CropAspect, out var cropAspectChanged))
            {
                cropAspectChanged.AddHandler(aspect =>
                {
                    CropAspectReceived?.Invoke(aspect);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ShowGateMask, out var showGateMaskChanged))
            {
                showGateMaskChanged.AddHandler(show =>
                {
                    ShowGateMaskReceived?.Invoke(show);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ShowFrameLines, out var showFrameLinesChanged))
            {
                showFrameLinesChanged.AddHandler(show =>
                {
                    ShowFrameLinesReceived?.Invoke(show);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ShowCenterMarker, out var showCenterMarkerChanged))
            {
                showCenterMarkerChanged.AddHandler(show =>
                {
                    ShowCenterMarkerReceived?.Invoke(show);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.ShowFocusPlane, out var showFocusPlaneChanged))
            {
                showFocusPlaneChanged.AddHandler(show =>
                {
                    ShowFocusPlaneReceived?.Invoke(show);
                });
            }
            if (BoolReceiver.TryGet(m_Protocol, VirtualCameraMessages.ToClient.VideoStreamIsRunning, out var videoStreamIsRunningChanged))
            {
                videoStreamIsRunningChanged.AddHandler(isRunning =>
                {
                    VideoStreamIsRunningReceived?.Invoke(isRunning);
                });
            }
            if (BinaryReceiver<int>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.VideoStreamPort, out var videoStreamPortChanged))
            {
                videoStreamPortChanged.AddHandler(port =>
                {
                    VideoStreamPortReceived?.Invoke(port);
                });
            }
            if (JsonReceiver<LensKitDescriptorV0>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.LensKitDescriptor_V0, out var lensKitDescriptorChanged_V0))
            {
                lensKitDescriptorChanged_V0.AddHandler(lensKit =>
                {
                    LensKitDescriptorReceived?.Invoke((LensKitDescriptor)lensKit);
                });
            }
            if (BinaryReceiver<int>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.SelectedLensAsset, out var selectedLensAssetChanged))
            {
                selectedLensAssetChanged.AddHandler(lensAsset =>
                {
                    SelectedLensAssetReceived?.Invoke(lensAsset);
                });
            }
            if (JsonReceiver<SnapshotListDescriptorV0>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.SnapshotListDescriptor_V0, out var SnapshotListDescriptorChanged_V0))
            {
                SnapshotListDescriptorChanged_V0.AddHandler(snapshotList =>
                {
                    SnapshotListDescriptorReceived?.Invoke((SnapshotListDescriptor)snapshotList);
                });
            }
            if (JsonReceiver<VcamTrackMetadataListDescriptorV0>.TryGet(m_Protocol, VirtualCameraMessages.ToClient.VcamTrackMetadataListDescriptor_V0, out var VcamTrackMetadataListDescriptorChanged_V0))
            {
                VcamTrackMetadataListDescriptorChanged_V0.AddHandler(metadataList =>
                {
                    VcamTrackMetadataListDescriptorReceived?.Invoke((VcamTrackMetadataListDescriptor)metadataList);
                });
            }
        }

        /// <summary>
        /// Sends channel flags to the server.
        /// </summary>
        /// <param name="channelFlags">The channel flags.</param>
        public void SendChannelFlags(VirtualCameraChannelFlags channelFlags)
        {
            m_ChannelFlagsSender?.Send(channelFlags);
        }

        /// <summary>
        /// Sends a frame sample to the server.
        /// </summary>
        /// <param name="sample">The frame sample.</param>
        public void SendInput(InputSample sample)
        {
            m_InputSender?.Send((InputSampleV0)sample);
        }

        /// <summary>
        /// Sends a joystick sample to the server.
        /// </summary>
        /// <param name="sample">The joysticks sample.</param>
        public void SendJoysticks(JoysticksSample sample)
        {
            m_JoysticksSender?.Send((JoysticksSampleV0)sample);
        }

        /// <summary>
        /// Sends a gamepad sample to the server.
        /// </summary>
        /// <param name="sample">The gamepad sample.</param>
        public void SendGamepad(GamepadSample sample)
        {
            m_GamepadSender?.Send((GamepadSampleV0)sample);
        }

        /// <summary>
        /// Sends a pose sample to the server.
        /// </summary>
        /// <param name="sample">The pose sample.</param>
        public void SendPose(PoseSample sample)
        {
            m_SendPoseImpl?.Invoke(sample);
        }

        /// <summary>
        /// Sends a focal length sample to the server.
        /// </summary>
        /// <param name="sample">The focal length sample.</param>
        public void SendFocalLength(FocalLengthSample sample)
        {
            m_SendFocalLengthImpl?.Invoke(sample);
        }

        /// <summary>
        /// Sends a focus distance sample to the server.
        /// </summary>
        /// <param name="sample">The aperture sample.</param>
        public void SendFocusDistance(FocusDistanceSample sample)
        {
            m_SendFocusDistanceImpl?.Invoke(sample);
        }

        /// <summary>
        /// Sends an aperture sample to the server.
        /// </summary>
        /// <param name="sample">The aperture sample.</param>
        public void SendAperture(ApertureSample sample)
        {
            m_SendApertureImpl?.Invoke(sample);
        }

        public void SendDampingEnabled(bool value)
        {
            m_DampingEnabledSender?.Send(value);
        }

        public void SendBodyDamping(Vector3 value)
        {
            m_BodyDampingSender?.Send(value);
        }

        public void SendAimDamping(float value)
        {
            m_AimDampingSender?.Send(value);
        }

        public void SendFocalLengthDamping(float value)
        {
            m_FocalLengthDampingSender?.Send(value);
        }

        public void SendFocusDistanceDamping(float value)
        {
            m_FocusDistanceDampingSender?.Send(value);
        }

        public void SendApertureDamping(float value)
        {
            m_ApertureDampingSender?.Send(value);
        }

        public void SendPositionLock(PositionAxis value)
        {
            m_PositionLockSender?.Send(value);
        }

        public void SendRotationLock(RotationAxis value)
        {
            m_RotationLockSender?.Send(value);
        }

        public void SendAutoHorizon(bool value)
        {
            m_AutoHorizonSender?.Send(value);
        }

        public void SendErgonomicTilt(float value)
        {
            m_ErgonomicTiltSender?.Send(value);
        }

        public void SendRebasing(bool value)
        {
            m_RebasingSender?.Send(value);
        }

        public void SendMotionScale(Vector3 value)
        {
            m_MotionScaleSender?.Send(value);
        }

        public void SendJoystickSensitivity(Vector3 value)
        {
            m_JoystickSensitivitySender?.Send(value);
        }

        public void SendPedestalSpace(Space value)
        {
            m_PedestalSpaceSender?.Send(value);
        }

        public void SendMotionSpace(Space value)
        {
            m_MotionSpaceSender?.Send(value);
        }

        public void SendFocusMode(FocusMode value)
        {
            m_FocusModeSender?.Send(value);
        }

        public void SendFocusReticlePosition(Vector2 value)
        {
            m_FocusReticlePositionSender?.Send(value);
        }

        public void SendFocusDistanceOffset(float value)
        {
            m_FocusDistanceOffsetSender?.Send(value);
        }

        public void SendCropAspect(float value)
        {
            m_CropAspectSender?.Send(value);
        }

        public void SendShowGateMask(bool value)
        {
            m_ShowGateMaskSender?.Send(value);
        }

        public void SendShowFrameLines(bool value)
        {
            m_ShowFrameLinesSender?.Send(value);
        }

        public void SendShowCenterMarker(bool value)
        {
            m_ShowCenterMarkerSender?.Send(value);
        }

        public void SendShowFocusPlane(bool value)
        {
            m_ShowFocusPlaneSender?.Send(value);
        }

        /// <summary>
        /// Sets the ergonomic tilt.
        /// </summary>
        /// <param name="ergonomicTilt">The ergonomic tilt in degrees..</param>
        public void SetErgonomicTilt(float ergonomicTilt)
        {
            m_ErgonomicTiltSender?.Send(ergonomicTilt);
        }

        /// <summary>
        /// Moves the virtual camera back to the origin.
        /// </summary>
        public void SetPoseToOrigin()
        {
            m_PoseToOriginSender?.Send();
        }

        /// <summary>
        /// Requests to set a lens asset with Guid.
        /// </summary>
        /// <param name="guid">The Guid of the lens asset to set.</param>
        public void SetLensAsset(Guid guid)
        {
            m_SetLensAssetSender?.Send(guid);
        }

        /// <summary>
        /// Requests to take a snapshot.
        /// </summary>
        public void TakeSnapshot()
        {
            m_TakeSnapshotSender?.Send();
        }

        /// <summary>
        /// Requests to go to a snapshot with index.
        /// </summary>
        /// <param name="index">The index of the snapshot in the list to set.</param>
        public void GoToSnapshot(int index)
        {
            m_GoToSnapshotSender?.Send(index);
        }

        /// <summary>
        /// Requests to load a snapshot with index.
        /// </summary>
        /// <param name="index">The index of the snapshot in the list to set.</param>
        public void LoadSnapshot(int index)
        {
            m_LoadSnapshotSender?.Send(index);
        }

        /// <summary>
        /// Requests to delete a snapshot with index.
        /// </summary>
        /// <param name="index">The index of the snapshot in the list to delete.</param>
        public void DeleteSnapshot(int index)
        {
            m_DeleteSnapshotSender?.Send(index);
        }
    }
}
