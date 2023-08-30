using System;
using Unity.LiveCapture.CompanionApp;
using Unity.LiveCapture.Networking;
using Unity.LiveCapture.Networking.Protocols;
using Unity.LiveCapture.VirtualCamera.Networking;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// An interface used to communicate with the virtual camera companion app.
    /// </summary>
    public interface IVirtualCameraClient : ICompanionAppClient
    {
    }

    interface IVirtualCameraClientInternal : IVirtualCameraClient, ICompanionAppClientInternal
    {
        /// <summary>
        /// An event invoked when channel flags are received.
        /// </summary>
        event Action<VirtualCameraChannelFlags> ChannelFlagsReceived;

        /// <summary>
        /// An event invoked when a input sample is received.
        /// </summary>
        event Action<InputSample> InputSampleReceived;

        /// <summary>
        /// An event invoked when a focal length sample is received.
        /// </summary>
        event Action<FocalLengthSample> FocalLengthSampleReceived;

        /// <summary>
        /// An event invoked when a focus distance sample is received.
        /// </summary>
        event Action<FocusDistanceSample> FocusDistanceSampleReceived;

        /// <summary>
        /// An event invoked when a aperture sample is received.
        /// </summary>
        event Action<ApertureSample> ApertureSampleReceived;

        /// <summary>
        /// An event invoked when the client sets whether damping should be applied to the camera.
        /// </summary>
        event Action<bool> DampingEnabledReceived;

        /// <summary>
        /// An event invoked when the client sets the camera body damping strength.
        /// </summary>
        event Action<Vector3> BodyDampingReceived;

        /// <summary>
        /// An event invoked when the client sets the camera aiming damping strength.
        /// </summary>
        event Action<float> AimDampingReceived;

        /// <summary>
        /// An event invoked when the client sets the camera focal length damping strength.
        /// </summary>
        event Action<float> FocalLengthDampingReceived;

        /// <summary>
        /// An event invoked when the client sets the camera focus distance damping strength.
        /// </summary>
        event Action<float> FocusDistanceDampingReceived;

        /// <summary>
        /// An event invoked when the client sets the camera aperture damping strength.
        /// </summary>
        event Action<float> ApertureDampingReceived;

        /// <summary>
        /// An event invoked when the client sets the axes along which camera translation is locked.
        /// </summary>
        event Action<PositionAxis> PositionLockReceived;

        /// <summary>
        /// An event invoked when the client sets the axes along which camera rotation is locked.
        /// </summary>
        event Action<RotationAxis> RotationLockReceived;

        /// <summary>
        /// An event invoked when the client sets if the camera should not be rolled.
        /// </summary>
        event Action<bool> AutoHorizonReceived;

        /// <summary>
        /// An event invoked when the client sets the ergonomic tilt.
        /// </summary>
        event Action<float> ErgonomicTiltReceived;

        /// <summary>
        /// An event invoked when the client sets whether the camera is rebasing.
        /// </summary>
        event Action<bool> RebasingReceived;

        /// <summary>
        /// An event invoked when the client sets motion scale of the camera translation.
        /// </summary>
        event Action<Vector3> MotionScaleReceived;

        /// <summary>
        /// An event invoked when the client sets the joystick sensitivity.
        /// </summary>
        event Action<Vector3> JoystickSensitivityReceived;

        /// <summary>
        /// An event invoked when the client sets the pedestal space.
        /// </summary>
        event Action<Space> PedestalSpaceReceived;

        /// <summary>
        /// An event invoked when the client sets the motion space.
        /// </summary>
        event Action<Space> MotionSpaceReceived;

        /// <summary>
        /// An event invoked when the client sets the focus mode of the camera.
        /// </summary>
        event Action<FocusMode> FocusModeReceived;

        /// <summary>
        /// An event invoked when the client changes the virtual camera's auto-focus reticle position.
        /// </summary>
        /// <remarks>
        /// The value is the view space position of the reticle in the camera.
        /// </remarks>
        event Action<Vector2> FocusReticlePositionReceived;

        /// <summary>
        /// An event invoked when the client sets the pedestal space.
        /// </summary>
        event Action<float> FocusDistanceOffsetReceived;

        /// <summary>
        /// An event invoked when the client sets the crop aspect of the camera.
        /// </summary>
        event Action<float> CropAspectReceived;

        /// <summary>
        /// An event invoked when the client sets the gate fit of the camera.
        /// </summary>
        event Action<GateFit> GateFitReceived;

        /// <summary>
        /// An event invoked when the client sets whether the gate mask should be visible.
        /// </summary>
        event Action<bool> ShowGateMaskReceived;

        /// <summary>
        /// An event invoked when the client sets whether the frame lines should be visible.
        /// </summary>
        event Action<bool> ShowFrameLinesReceived;

        /// <summary>
        /// An event invoked when the client sets whether the center marker should be visible.
        /// </summary>
        event Action<bool> ShowCenterMarkerReceived;

        /// <summary>
        /// An event invoked when the client sets whether the focus plane should be visible.
        /// </summary>
        event Action<bool> ShowFocusPlaneReceived;

        /// <summary>
        /// An event invoked when the client wants to move the virtual camera back to the origin.
        /// </summary>
        event Action SetPoseToOrigin;

        /// <summary>
        /// An event invoked when the client requests the lens asset to select.
        /// </summary>
        event Action<SerializableGuid> SetLensAsset;

        /// <summary>
        /// An event invoked when the client requests to take a snapshot.
        /// </summary>
        event Action TakeSnapshot;

        /// <summary>
        /// An event invoked when the client requests to go to a snapshot.
        /// </summary>
        event Action<int> GoToSnapshot;

        /// <summary>
        /// An event invoked when the client requests to load a snapshot.
        /// </summary>
        event Action<int> LoadSnapshot;

        /// <summary>
        /// An event invoked when the client requests to delete a snapshot.
        /// </summary>
        event Action<int> DeleteSnapshot;

        /// <summary>
        /// Sends the <see cref="VirtualCameraChannelFlags"/> parameters to the client.
        /// </summary>
        /// <param name="channelFlags">The channel flags to send.</param>
        void SendChannelFlags(VirtualCameraChannelFlags channelFlags);

        /// <summary>
        /// Sends the <see cref="Lens"/> parameters to the client.
        /// </summary>
        /// <param name="lens">The lens parameters to send.</param>
        void SendLens(Lens lens);

        /// <summary>
        /// Sends the camera body parameters to the client.
        /// </summary>
        /// <param name="body">The body parameters to send.</param>
        void SendCameraBody(CameraBody body);

        /// <summary>
        /// Sends the camera settings to the client.
        /// </summary>
        /// <param name="settings">The state to send.</param>
        void SendSettings(Settings settings);

        /// <summary>
        /// Sends the video stream state to the client.
        /// </summary>
        /// <param name="state">The state to send.</param>
        void SendVideoStreamState(VideoStreamState state);

        /// <summary>
        /// Sends the lens kit descriptor to the client.
        /// </summary>
        /// <param name="descriptor">The lens kit information to send.</param>
        void SendLensKitDescriptor(LensKitDescriptor descriptor);

        /// <summary>
        /// Sends the snapshot list descriptor to the client.
        /// </summary>
        /// <param name="descriptor">The snapshot list information to send.</param>
        void SendSnapshotListDescriptor(SnapshotListDescriptor descriptor);

        /// <summary>
        /// Sends the virtual camera track metadata list descriptor to the client.
        /// </summary>
        /// <param name="descriptor">The virtual camera track metadata information to send<./param>
        void SendVirtualCameraTrackMetadataListDescriptor(VcamTrackMetadataListDescriptor descriptor);
    }

    /// <summary>
    /// A class used to communicate with the virtual camera companion app.
    /// </summary>
    [Preserve]
    [Client(k_ClientType)]
    class VirtualCameraClient : CompanionAppClient, IVirtualCameraClientInternal
    {
        /// <summary>
        /// The type of client this device supports.
        /// </summary>
        const string k_ClientType = "Virtual Camera";

        readonly BinarySender<VirtualCameraChannelFlags> m_ChannelFlagsSender;
        readonly BinarySender<float> m_FocalLengthSender;
        readonly BinarySender<float> m_FocusDistanceSender;
        readonly BinarySender<float> m_ApertureSender;
        readonly BinarySender<Vector2> m_SensorSizeSender;
        readonly BinarySender<int> m_IsoSender;
        readonly BinarySender<float> m_ShutterSpeedSender;
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
        readonly BoolSender m_VideoStreamIsRunningSender;
        readonly BinarySender<int> m_VideoStreamPortSender;
        readonly JsonSender<LensKitDescriptorV0> m_LensKitDescriptorSender;
        readonly BinarySender<int> m_SelectedLensAssetSender;
        readonly JsonSender<SnapshotListDescriptorV0> m_SnapshotListDescriptorSender;
        readonly JsonSender<VcamTrackMetadataListDescriptorV0> m_VirtualCameraTrackMetadataListDescriptorSender;

        /// <inheritdoc />
        public event Action<VirtualCameraChannelFlags> ChannelFlagsReceived;
        /// <inheritdoc />
        public event Action<FocalLengthSample> FocalLengthSampleReceived;
        /// <inheritdoc />
        public event Action<FocusDistanceSample> FocusDistanceSampleReceived;
        /// <inheritdoc />
        public event Action<ApertureSample> ApertureSampleReceived;
        /// <inheritdoc />
        public event Action<InputSample> InputSampleReceived;
        /// <inheritdoc />
        public event Action<bool> DampingEnabledReceived;
        /// <inheritdoc />
        public event Action<Vector3> BodyDampingReceived;
        /// <inheritdoc />
        public event Action<float> AimDampingReceived;
        /// <inheritdoc />
        public event Action<float> FocalLengthDampingReceived;
        /// <inheritdoc />
        public event Action<float> FocusDistanceDampingReceived;
        /// <inheritdoc />
        public event Action<float> ApertureDampingReceived;
        /// <inheritdoc />
        public event Action<PositionAxis> PositionLockReceived;
        /// <inheritdoc />
        public event Action<RotationAxis> RotationLockReceived;
        /// <inheritdoc />
        public event Action<bool> AutoHorizonReceived;
        /// <inheritdoc />
        public event Action<float> ErgonomicTiltReceived;
        /// <inheritdoc />
        public event Action<bool> RebasingReceived;
        /// <inheritdoc />
        public event Action<Vector3> MotionScaleReceived;
        /// <inheritdoc />
        public event Action<Vector3> JoystickSensitivityReceived;
        /// <inheritdoc />
        public event Action<Space> PedestalSpaceReceived;
        /// <inheritdoc />
        public event Action<Space> MotionSpaceReceived;
        /// <inheritdoc />
        public event Action<FocusMode> FocusModeReceived;
        /// <inheritdoc />
        public event Action<Vector2> FocusReticlePositionReceived;
        /// <inheritdoc />
        public event Action<float> FocusDistanceOffsetReceived;
        /// <inheritdoc />
        public event Action<float> CropAspectReceived;
        /// <inheritdoc />
        public event Action<GateFit> GateFitReceived;
        /// <inheritdoc />
        public event Action<bool> ShowGateMaskReceived;
        /// <inheritdoc />
        public event Action<bool> ShowFrameLinesReceived;
        /// <inheritdoc />
        public event Action<bool> ShowCenterMarkerReceived;
        /// <inheritdoc />
        public event Action<bool> ShowFocusPlaneReceived;
        /// <inheritdoc />
        public event Action SetPoseToOrigin;
        /// <inheritdoc />
        public event Action<SerializableGuid> SetLensAsset;
        /// <inheritdoc />
        public event Action TakeSnapshot;
        /// <inheritdoc />
        public event Action<int> GoToSnapshot;
        /// <inheritdoc />
        public event Action<int> LoadSnapshot;
        /// <inheritdoc />
        public event Action<int> DeleteSnapshot;

        /// <inheritdoc />
        public VirtualCameraClient(NetworkBase network, Remote remote, ClientInitialization data)
            : base(network, remote, data)
        {
            m_ChannelFlagsSender = m_Protocol.Add(new BinarySender<VirtualCameraChannelFlags>(VirtualCameraMessages.ToClient.ChannelFlags));
            m_FocalLengthSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.FocalLength));
            m_FocusDistanceSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.FocusDistance));
            m_ApertureSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.Aperture));
            m_SensorSizeSender = m_Protocol.Add(new BinarySender<Vector2>(VirtualCameraMessages.ToClient.SensorSize));
            m_IsoSender = m_Protocol.Add(new BinarySender<int>(VirtualCameraMessages.ToClient.Iso));
            m_ShutterSpeedSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.ShutterSpeed));
            m_DampingEnabledSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.DampingEnabled));
            m_BodyDampingSender = m_Protocol.Add(new BinarySender<Vector3>(VirtualCameraMessages.ToClient.BodyDamping));
            m_AimDampingSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.AimDamping));
            m_FocalLengthDampingSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.FocalLengthDamping));
            m_FocusDistanceDampingSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.FocusDistanceDamping));
            m_ApertureDampingSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.ApertureDamping));
            m_PositionLockSender = m_Protocol.Add(new BinarySender<PositionAxis>(VirtualCameraMessages.ToClient.PositionLock));
            m_RotationLockSender = m_Protocol.Add(new BinarySender<RotationAxis>(VirtualCameraMessages.ToClient.RotationLock));
            m_AutoHorizonSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.AutoHorizon));
            m_ErgonomicTiltSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.ErgonomicTilt));
            m_RebasingSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.Rebasing));
            m_MotionScaleSender = m_Protocol.Add(new BinarySender<Vector3>(VirtualCameraMessages.ToClient.MotionScale));
            m_JoystickSensitivitySender = m_Protocol.Add(new BinarySender<Vector3>(VirtualCameraMessages.ToClient.JoystickSensitivity));
            m_PedestalSpaceSender = m_Protocol.Add(new BinarySender<Space>(VirtualCameraMessages.ToClient.PedestalSpace));
            m_MotionSpaceSender = m_Protocol.Add(new BinarySender<Space>(VirtualCameraMessages.ToClient.MotionSpace));
            m_FocusModeSender = m_Protocol.Add(new BinarySender<FocusMode>(VirtualCameraMessages.ToClient.FocusMode));
            m_FocusReticlePositionSender = m_Protocol.Add(new BinarySender<Vector2>(VirtualCameraMessages.ToClient.FocusReticlePosition));
            m_FocusDistanceOffsetSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.FocusDistanceOffset));
            m_CropAspectSender = m_Protocol.Add(new BinarySender<float>(VirtualCameraMessages.ToClient.CropAspect));
            m_GateFitSender = m_Protocol.Add(new BinarySender<GateFit>(VirtualCameraMessages.ToClient.GateFit));
            m_ShowGateMaskSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.ShowGateMask));
            m_ShowFrameLinesSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.ShowFrameLines));
            m_ShowCenterMarkerSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.ShowCenterMarker));
            m_ShowFocusPlaneSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.ShowFocusPlane));
            m_VideoStreamIsRunningSender = m_Protocol.Add(new BoolSender(VirtualCameraMessages.ToClient.VideoStreamIsRunning));
            m_VideoStreamPortSender = m_Protocol.Add(new BinarySender<int>(VirtualCameraMessages.ToClient.VideoStreamPort));
            m_LensKitDescriptorSender = m_Protocol.Add(new JsonSender<LensKitDescriptorV0>(VirtualCameraMessages.ToClient.LensKitDescriptor_V0));
            m_SelectedLensAssetSender = m_Protocol.Add(new BinarySender<int>(VirtualCameraMessages.ToClient.SelectedLensAsset, options: DataOptions.None));
            m_SnapshotListDescriptorSender = m_Protocol.Add(new JsonSender<SnapshotListDescriptorV0>(VirtualCameraMessages.ToClient.SnapshotListDescriptor_V0));
            m_VirtualCameraTrackMetadataListDescriptorSender = m_Protocol.Add(new JsonSender<VcamTrackMetadataListDescriptorV0>(VirtualCameraMessages.ToClient.VcamTrackMetadataListDescriptor_V0));

            m_Protocol.Add(new BinaryReceiver<VirtualCameraChannelFlags>(VirtualCameraMessages.ToServer.ChannelFlags, options: DataOptions.None)).AddHandler(flags =>
            {
                ChannelFlagsReceived?.Invoke(flags);
            });
            m_Protocol.Add(new BinaryReceiver<InputSampleV0>(VirtualCameraMessages.ToServer.InputSample_V0, ChannelType.UnreliableUnordered)).AddHandler(sample =>
            {
                InputSampleReceived?.Invoke((InputSample)sample);
            });
            m_Protocol.Add(new BinaryReceiver<FocalLengthSampleV1>(VirtualCameraMessages.ToServer.FocalLengthSample_V1, ChannelType.UnreliableUnordered)).AddHandler(focalLength =>
            {
                FocalLengthSampleReceived?.Invoke((FocalLengthSample)focalLength);
            });
            m_Protocol.Add(new BinaryReceiver<FocusDistanceSampleV1>(VirtualCameraMessages.ToServer.FocusDistanceSample_V1, ChannelType.UnreliableUnordered)).AddHandler(focusDistance =>
            {
                FocusDistanceSampleReceived?.Invoke((FocusDistanceSample)focusDistance);
            });
            m_Protocol.Add(new BinaryReceiver<ApertureSampleV1>(VirtualCameraMessages.ToServer.ApertureSample_V1, ChannelType.UnreliableUnordered)).AddHandler(aperture =>
            {
                ApertureSampleReceived?.Invoke((ApertureSample)aperture);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.DampingEnabled)).AddHandler(damping =>
            {
                DampingEnabledReceived?.Invoke(damping);
            });
            m_Protocol.Add(new BinaryReceiver<Vector3>(VirtualCameraMessages.ToServer.BodyDamping)).AddHandler(damping =>
            {
                BodyDampingReceived?.Invoke(damping);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.AimDamping)).AddHandler(damping =>
            {
                AimDampingReceived?.Invoke(damping);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.FocalLengthDamping)).AddHandler(damping =>
            {
                FocalLengthDampingReceived?.Invoke(damping);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.FocusDistanceDamping)).AddHandler(damping =>
            {
                FocusDistanceDampingReceived?.Invoke(damping);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.ApertureDamping)).AddHandler(damping =>
            {
                ApertureDampingReceived?.Invoke(damping);
            });
            m_Protocol.Add(new BinaryReceiver<PositionAxis>(VirtualCameraMessages.ToServer.PositionLock)).AddHandler(positionLock =>
            {
                PositionLockReceived?.Invoke(positionLock);
            });
            m_Protocol.Add(new BinaryReceiver<RotationAxis>(VirtualCameraMessages.ToServer.RotationLock)).AddHandler(rotationLock =>
            {
                RotationLockReceived?.Invoke(rotationLock);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.AutoHorizon)).AddHandler(autoHorizon =>
            {
                AutoHorizonReceived?.Invoke(autoHorizon);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.ErgonomicTilt)).AddHandler(tilt =>
            {
                ErgonomicTiltReceived?.Invoke(tilt);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.Rebasing)).AddHandler(rebasing =>
            {
                RebasingReceived?.Invoke(rebasing);
            });
            m_Protocol.Add(new BinaryReceiver<Vector3>(VirtualCameraMessages.ToServer.MotionScale)).AddHandler(scale =>
            {
                MotionScaleReceived?.Invoke(scale);
            });
            m_Protocol.Add(new BinaryReceiver<Vector3>(VirtualCameraMessages.ToServer.JoystickSensitivity)).AddHandler(sensitivity =>
            {
                JoystickSensitivityReceived?.Invoke(sensitivity);
            });
            m_Protocol.Add(new BinaryReceiver<Space>(VirtualCameraMessages.ToServer.PedestalSpace)).AddHandler(space =>
            {
                PedestalSpaceReceived?.Invoke(space);
            });
            m_Protocol.Add(new BinaryReceiver<Space>(VirtualCameraMessages.ToServer.MotionSpace)).AddHandler(space =>
            {
                MotionSpaceReceived?.Invoke(space);
            });
            m_Protocol.Add(new BinaryReceiver<FocusMode>(VirtualCameraMessages.ToServer.FocusMode)).AddHandler(mode =>
            {
                FocusModeReceived?.Invoke(mode);
            });
            m_Protocol.Add(new BinaryReceiver<Vector2>(VirtualCameraMessages.ToServer.FocusReticlePosition)).AddHandler(position =>
            {
                FocusReticlePositionReceived?.Invoke(position);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.FocusDistanceOffset)).AddHandler(offset =>
            {
                FocusDistanceOffsetReceived?.Invoke(offset);
            });
            m_Protocol.Add(new BinaryReceiver<float>(VirtualCameraMessages.ToServer.CropAspect)).AddHandler(aspect =>
            {
                CropAspectReceived?.Invoke(aspect);
            });
            m_Protocol.Add(new BinaryReceiver<GateFit>(VirtualCameraMessages.ToServer.GateFit)).AddHandler(gateFit =>
            {
                GateFitReceived?.Invoke(gateFit);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.ShowGateMask)).AddHandler(show =>
            {
                ShowGateMaskReceived?.Invoke(show);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.ShowFrameLines)).AddHandler(show =>
            {
                ShowFrameLinesReceived?.Invoke(show);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.ShowCenterMarker)).AddHandler(show =>
            {
                ShowCenterMarkerReceived?.Invoke(show);
            });
            m_Protocol.Add(new BoolReceiver(VirtualCameraMessages.ToServer.ShowFocusPlane)).AddHandler(show =>
            {
                ShowFocusPlaneReceived?.Invoke(show);
            });
            m_Protocol.Add(new EventReceiver(VirtualCameraMessages.ToServer.SetPoseToOrigin)).AddHandler(() =>
            {
                SetPoseToOrigin?.Invoke();
            });
            m_Protocol.Add(new BinaryReceiver<SerializableGuid>(VirtualCameraMessages.ToServer.SetLensAsset, options: DataOptions.None)).AddHandler(guid =>
            {
                SetLensAsset?.Invoke(guid);
            });
            m_Protocol.Add(new EventReceiver(VirtualCameraMessages.ToServer.TakeSnapshot)).AddHandler(() =>
            {
                TakeSnapshot?.Invoke();
            });
            m_Protocol.Add(new BinaryReceiver<int>(VirtualCameraMessages.ToServer.GoToSnapshot, options: DataOptions.None)).AddHandler(index =>
            {
                GoToSnapshot?.Invoke(index);
            });
            m_Protocol.Add(new BinaryReceiver<int>(VirtualCameraMessages.ToServer.LoadSnapshot, options: DataOptions.None)).AddHandler(index =>
            {
                LoadSnapshot?.Invoke(index);
            });
            m_Protocol.Add(new BinaryReceiver<int>(VirtualCameraMessages.ToServer.DeleteSnapshot, options: DataOptions.None)).AddHandler(index =>
            {
                DeleteSnapshot?.Invoke(index);
            });
        }

        /// <inheritdoc />
        public void SendChannelFlags(VirtualCameraChannelFlags channelFlags)
        {
            m_ChannelFlagsSender.Send(channelFlags);
        }

        /// <inheritdoc />
        public void SendLens(Lens lens)
        {
            m_FocalLengthSender.Send(lens.FocalLength);
            m_FocusDistanceSender.Send(lens.FocusDistance);
            m_ApertureSender.Send(lens.Aperture);
        }

        /// <inheritdoc />
        public void SendCameraBody(CameraBody body)
        {
            m_SensorSizeSender.Send(body.SensorSize);
            m_IsoSender.Send(body.Iso);
            m_ShutterSpeedSender.Send(body.ShutterSpeed);
        }

        /// <inheritdoc />
        public void SendSettings(Settings state)
        {
            m_DampingEnabledSender.Send(state.Damping.Enabled);
            m_BodyDampingSender.Send(state.Damping.Body);
            m_AimDampingSender.Send(state.Damping.Aim);
            m_FocalLengthDampingSender.Send(state.FocalLengthDamping);
            m_FocusDistanceDampingSender.Send(state.FocusDistanceDamping);
            m_ApertureDampingSender.Send(state.ApertureDamping);
            m_PositionLockSender.Send(state.PositionLock);
            m_RotationLockSender.Send(state.RotationLock);
            m_AutoHorizonSender.Send(state.AutoHorizon);
            m_ErgonomicTiltSender.Send(state.ErgonomicTilt);
            m_RebasingSender.Send(state.Rebasing);
            m_MotionScaleSender.Send(state.MotionScale);
            m_JoystickSensitivitySender.Send(state.JoystickSensitivity);
            m_PedestalSpaceSender.Send(state.PedestalSpace);
            m_MotionSpaceSender.Send(state.MotionSpace);
            m_FocusModeSender.Send(state.FocusMode);
            m_FocusReticlePositionSender.Send(state.ReticlePosition);
            m_FocusDistanceOffsetSender.Send(state.FocusDistanceOffset);
            m_CropAspectSender.Send(state.AspectRatio);
            m_GateFitSender.Send(state.GateFit);
            m_ShowGateMaskSender.Send(state.GateMask);
            m_ShowFrameLinesSender.Send(state.AspectRatioLines);
            m_ShowCenterMarkerSender.Send(state.CenterMarker);
            m_ShowFocusPlaneSender.Send(state.FocusPlane);
        }

        /// <inheritdoc />
        public void SendVideoStreamState(VideoStreamState state)
        {
            m_VideoStreamIsRunningSender.Send(state.IsRunning);
            m_VideoStreamPortSender.Send(state.Port);
        }

        /// <inheritdoc />
        public void SendLensKitDescriptor(LensKitDescriptor descriptor)
        {
            m_LensKitDescriptorSender.Send((LensKitDescriptorV0)descriptor);
            m_SelectedLensAssetSender.Send(descriptor.SelectedLensAsset);
        }

        /// <inheritdoc />
        public void SendSnapshotListDescriptor(SnapshotListDescriptor descriptor)
        {
            m_SnapshotListDescriptorSender.Send((SnapshotListDescriptorV0)descriptor);
        }

        /// <inheritdoc />
        public void SendVirtualCameraTrackMetadataListDescriptor(VcamTrackMetadataListDescriptor descriptor)
        {
            m_VirtualCameraTrackMetadataListDescriptorSender.Send((VcamTrackMetadataListDescriptorV0)descriptor);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => k_ClientType;
    }
}
