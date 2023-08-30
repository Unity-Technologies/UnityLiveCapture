using UnityEngine;
using Zenject;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    // TODO rename, covers broader communication with the host
    class SettingsSystem : IInitializable
    {
        [Inject]
        IRemoteSystem m_Remote;
        [Inject]
        StateModel m_State;
        [Inject]
        SettingsModel m_Settings;
        [Inject]
        CameraModel m_Camera;
        [Inject]
        TakeLibraryModel m_TakeLibrary;
        [Inject]
        SignalBus m_SignalBus;
        VideoStreamState m_VideoStreamState;

        public void Initialize()
        {
            m_SignalBus.Subscribe<RemoteConnectedSignal>(OnConnected);
            m_SignalBus.Subscribe<RemoteDisconnectedSignal>(OnDisconnected);
        }

        void OnConnected()
        {
            var host = m_Remote.Host as VirtualCameraHost;

            if (host != null)
            {
                host.FocalLengthReceived += OnFocalLengthReceived;
                host.FocusDistanceReceived += OnFocusDistanceReceived;
                host.ApertureReceived += OnApertureReceived;
                host.LensKitDescriptorReceived += OnLensKitReceived;
                host.SelectedLensAssetReceived += OnSelectedLensAssetReceived;
                host.SensorSizeReceived += OnSensorSizeReceived;
                host.IsoReceived += OnIsoReceived;
                host.ShutterSpeedReceived += OnShutterSpeedReceived;
                host.DampingEnabledReceived += OnDampingEnabledReceived;
                host.BodyDampingReceived += OnBodyDampingReceived;
                host.AimDampingReceived += OnAimDampingReceived;
                host.FocalLengthDampingReceived += OnFocalLengthDampingReceived;
                host.FocusDistanceDampingReceived += OnFocusDistanceDampingReceived;
                host.ApertureDampingReceived += OnApertureDampingReceived;
                host.PositionLockReceived += OnPositionLockReceived;
                host.RotationLockReceived += OnRotationLockReceived;
                host.AutoHorizonReceived += OnAutoHorizonReceived;
                host.ErgonomicTiltReceived += OnErgonomicTiltReceived;
                host.RebasingReceived += OnRebasingReceived;
                host.MotionScaleReceived += OnMotionScaleReceived;
                host.JoystickSensitivityReceived += OnJoystickSensitivityReceived;
                host.PedestalSpaceReceived += OnPedestalSpaceReceived;
                host.MotionSpaceReceived += OnMotionSpaceReceived;
                host.FocusModeReceived += OnFocusModeReceived;
                host.FocusReticlePositionReceived += OnFocusReticlePositionReceived;
                host.FocusDistanceOffsetReceived += OnFocusDistanceOffsetReceived;
                host.CropAspectReceived += OnCropAspectReceived;
                host.ShowGateMaskReceived += OnShowGateMaskReceived;
                host.ShowFrameLinesReceived += OnShowFrameLinesReceived;
                host.ShowCenterMarkerReceived += OnShowCenterMarkerReceived;
                host.ShowFocusPlaneReceived += OnShowFocusPlaneReceived;
                host.VideoStreamIsRunningReceived += OnVideoStreamIsRunningReceived;
                host.VideoStreamPortReceived += OnVideoStreamPortReceived;
                host.VcamTrackMetadataListDescriptorReceived += OnMetadataReceived;
                host.ChannelFlagsReceived += OnChannelFlagsReceived;
            }
        }

        public void OnDisconnected()
        {
            var host = m_Remote.Host as VirtualCameraHost;

            if (host != null)
            {
                host.FocalLengthReceived -= OnFocalLengthReceived;
                host.FocusDistanceReceived -= OnFocusDistanceReceived;
                host.ApertureReceived -= OnApertureReceived;
                host.LensKitDescriptorReceived -= OnLensKitReceived;
                host.SelectedLensAssetReceived -= OnSelectedLensAssetReceived;
                host.SensorSizeReceived -= OnSensorSizeReceived;
                host.IsoReceived -= OnIsoReceived;
                host.ShutterSpeedReceived -= OnShutterSpeedReceived;
                host.DampingEnabledReceived -= OnDampingEnabledReceived;
                host.BodyDampingReceived -= OnBodyDampingReceived;
                host.AimDampingReceived -= OnAimDampingReceived;
                host.FocalLengthDampingReceived -= OnFocalLengthDampingReceived;
                host.FocusDistanceDampingReceived -= OnFocusDistanceDampingReceived;
                host.ApertureDampingReceived -= OnApertureDampingReceived;
                host.PositionLockReceived -= OnPositionLockReceived;
                host.RotationLockReceived -= OnRotationLockReceived;
                host.AutoHorizonReceived -= OnAutoHorizonReceived;
                host.ErgonomicTiltReceived -= OnErgonomicTiltReceived;
                host.RebasingReceived -= OnRebasingReceived;
                host.MotionScaleReceived -= OnMotionScaleReceived;
                host.JoystickSensitivityReceived -= OnJoystickSensitivityReceived;
                host.PedestalSpaceReceived -= OnPedestalSpaceReceived;
                host.MotionSpaceReceived -= OnMotionSpaceReceived;
                host.FocusModeReceived -= OnFocusModeReceived;
                host.FocusReticlePositionReceived -= OnFocusReticlePositionReceived;
                host.FocusDistanceOffsetReceived -= OnFocusDistanceOffsetReceived;
                host.CropAspectReceived -= OnCropAspectReceived;
                host.ShowGateMaskReceived -= OnShowGateMaskReceived;
                host.ShowFrameLinesReceived -= OnShowFrameLinesReceived;
                host.ShowCenterMarkerReceived -= OnShowCenterMarkerReceived;
                host.ShowFocusPlaneReceived -= OnShowFocusPlaneReceived;
                host.VideoStreamIsRunningReceived -= OnVideoStreamIsRunningReceived;
                host.VideoStreamPortReceived -= OnVideoStreamPortReceived;
                host.VcamTrackMetadataListDescriptorReceived -= OnMetadataReceived;
                host.ChannelFlagsReceived -= OnChannelFlagsReceived;
            }
        }

        void OnFocalLengthReceived(float focalLength)
        {
            var lens = m_Camera.Lens;
            lens.FocalLength = focalLength;
            m_Camera.Lens = lens;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocalLength });
        }

        void OnFocusDistanceReceived(float focusDistance)
        {
            var lens = m_Camera.Lens;
            lens.FocusDistance = focusDistance;
            m_Camera.Lens = lens;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocusDistance });
        }

        void OnApertureReceived(float aperture)
        {
            var lens = m_Camera.Lens;
            lens.Aperture = aperture;
            m_Camera.Lens = lens;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.Aperture });
        }

        void OnLensKitReceived(LensKitDescriptor lensKit)
        {
            m_Camera.LensKit = lensKit;
        }

        void OnSelectedLensAssetReceived(int selectedIndex)
        {
            var intrinsics = LensIntrinsics.DefaultParams;
            var lenses = m_Camera.LensKit.Lenses;

            if (selectedIndex > -1 && selectedIndex < lenses.Length)
            {
                intrinsics = lenses[selectedIndex].Intrinsics;
            }

            m_Camera.Intrinsics = intrinsics;
            m_Camera.SelectedLensIndex = selectedIndex;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.LensIntrinsics });
        }

        void OnSensorSizeReceived(Vector2 sensorSize)
        {
            var cameraBody = m_Camera.Body;
            cameraBody.SensorSize = sensorSize;
            m_Camera.Body = cameraBody;

            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.SensorSize });
        }

        void OnIsoReceived(int iso)
        {
            var cameraBody = m_Camera.Body;
            cameraBody.Iso = iso;
            m_Camera.Body = cameraBody;

            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.Iso });
        }

        void OnShutterSpeedReceived(float shutterSpeed)
        {
            var cameraBody = m_Camera.Body;
            cameraBody.ShutterSpeed = shutterSpeed;
            m_Camera.Body = cameraBody;

            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ShutterSpeed });
        }

        void OnDampingEnabledReceived(bool damping)
        {
            m_Settings.DampingEnabled = damping;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.DampingEnabled });
        }

        void OnBodyDampingReceived(Vector3 damping)
        {
            m_Settings.BodyDamping = damping;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.BodyDamping });
        }

        void OnAimDampingReceived(float damping)
        {
            m_Settings.AimDamping = damping;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.AimDamping });
        }

        void OnFocalLengthDampingReceived(float damping)
        {
            m_Settings.FocalLengthDamping = damping;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocalLengthDamping });
        }

        void OnFocusDistanceDampingReceived(float damping)
        {
            m_Settings.FocusDistanceDamping = damping;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocusDistanceDamping });
        }

        void OnApertureDampingReceived(float damping)
        {
            m_Settings.ApertureDamping = damping;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ApertureDamping });
        }

        void OnPositionLockReceived(PositionAxis axes)
        {
            m_Settings.PositionLock = axes;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.PositionLock });
        }

        void OnRotationLockReceived(RotationAxis axes)
        {
            m_Settings.RotationLock = axes;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.RotationLock });
        }

        void OnAutoHorizonReceived(bool autoHorizon)
        {
            m_Settings.AutoHorizon = autoHorizon;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.AutoHorizon });
        }

        void OnErgonomicTiltReceived(float tilt)
        {
            m_Settings.ErgonomicTilt = tilt;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ErgonomicTilt });
        }

        void OnRebasingReceived(bool rebasing)
        {
            m_Settings.Rebasing = rebasing;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.Rebasing });
        }

        void OnMotionScaleReceived(Vector3 scale)
        {
            m_Settings.MotionScale = scale;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.MotionScale });
        }

        void OnJoystickSensitivityReceived(Vector3 sensitivity)
        {
            m_Settings.JoystickSensitivity = sensitivity;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.JoystickSensitivity });
        }

        void OnPedestalSpaceReceived(Space space)
        {
            m_Settings.PedestalSpace = space;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.PedestalSpace });
        }

        void OnMotionSpaceReceived(Space space)
        {
            m_Settings.MotionSpace = space;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.MotionSpace });
        }

        void OnFocusModeReceived(FocusMode mode)
        {
            m_Settings.FocusMode = mode;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocusMode });
        }

        void OnFocusReticlePositionReceived(Vector2 position)
        {
            m_Settings.ReticlePosition = position;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocusReticlePosition });
        }

        void OnFocusDistanceOffsetReceived(float offset)
        {
            m_Settings.FocusDistanceOffset = offset;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.FocusDistanceOffset });
        }

        void OnCropAspectReceived(float aspect)
        {
            m_Settings.AspectRatio = aspect;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.CropAspect });
        }

        void OnShowGateMaskReceived(bool show)
        {
            m_Settings.GateMask = show;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ShowGateMask });
        }

        void OnShowFrameLinesReceived(bool show)
        {
            m_Settings.AspectRatioLines = show;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ShowFrameLines });
        }

        void OnShowCenterMarkerReceived(bool show)
        {
            m_Settings.CenterMarker = show;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ShowCenterMarker });
        }

        void OnShowFocusPlaneReceived(bool show)
        {
            m_Settings.FocusPlane = show;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ShowFocusPlane });
        }

        void OnVideoStreamIsRunningReceived(bool isRunning)
        {
            m_VideoStreamState.IsRunning = isRunning;

            FireVideoStreamChangedSignal();
        }

        void OnVideoStreamPortReceived(int port)
        {
            m_VideoStreamState.Port = port;

            FireVideoStreamChangedSignal();
        }

        void FireVideoStreamChangedSignal()
        {
            m_SignalBus.Fire(new VideoStreamChangedSignal() { value = m_VideoStreamState });
        }

        void OnMetadataReceived(VcamTrackMetadataListDescriptor descriptorsList)
        {
            m_TakeLibrary.SetVcamTrackMetadataDescriptors(descriptorsList.Descriptors);
        }

        void OnChannelFlagsReceived(VirtualCameraChannelFlags channelFlags)
        {
            m_State.ChannelFlags = channelFlags;
            m_SignalBus.Fire(new ReceiveHostSignal() { value = HostMessageType.ChannelFlags });
        }
    }
}
