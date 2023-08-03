using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ReceiveSettingCommand
    {
        [Inject]
        StateModel m_State;
        [Inject]
        CameraModel m_Camera;
        [Inject]
        SettingsModel m_Settings;
        [Inject]
        IDampingView m_DampingView;
        [Inject]
        IPositionView m_PositionView;
        [Inject]
        IRotationLockView m_RotationLockView;
        [Inject]
        ISettingsView m_SettingsView;
        [Inject]
        IJoystickGeneralTabView m_JoysticksSettingsView;
        [Inject]
        IGamepadConfigurationTabView m_GamepadSettingsView;
        [Inject]
        IResetView m_ResetView;
        [Inject]
        List<IFocalLengthListener> m_FocalLengthListeners;
        [Inject]
        List<IFocusDistanceListener> m_FocusDistanceListeners;
        [Inject]
        List<IFocusDistanceOffsetListener> m_FocusDistanceOffsetListeners;
        [Inject]
        List<IApertureListener> m_ApertureListeners;
        [Inject]
        List<IFocusModeListener> m_FocusModeListeners;
        [Inject]
        List<ILensIntrinsicsListener> m_LensIntrinsicsListeners;
        [Inject]
        List<ISensorSizeListener> m_SensorSizeListeners;
        [Inject]
        List<IReticlePositionListener> m_ReticlePositionListeners;
        [Inject]
        List<IChannelFlagsListener> m_ChannelFlagsListeners;

        public void Execute(ReceiveHostSignal signal)
        {
            switch (signal.value)
            {
                case HostMessageType.DampingEnabled:
                    m_DampingView.SetDampingEnabled(m_Settings.DampingEnabled);
                    break;
                case HostMessageType.BodyDamping:
                    m_DampingView.SetBodyDamping(m_Settings.BodyDamping);
                    break;
                case HostMessageType.AimDamping:
                    m_DampingView.SetAimDamping(m_Settings.AimDamping);
                    break;
                case HostMessageType.FocalLengthDamping:
                    m_SettingsView.SetFocalLengthDamping(m_Settings.FocalLengthDamping);
                    break;
                case HostMessageType.FocusDistanceDamping:
                    m_SettingsView.SetFocusDistanceDamping(m_Settings.FocusDistanceDamping);
                    break;
                case HostMessageType.ApertureDamping:
                    m_SettingsView.SetApertureDamping(m_Settings.ApertureDamping);
                    break;
                case HostMessageType.PositionLock:
                    m_PositionView.SetPositionLock(m_Settings.PositionLock);
                    break;
                case HostMessageType.RotationLock:
                    m_RotationLockView.SetRotationLock(m_Settings.RotationLock);
                    break;
                case HostMessageType.AutoHorizon:
                    m_RotationLockView.SetAutoHorizon(m_Settings.AutoHorizon);
                    break;
                case HostMessageType.ErgonomicTilt:
                    m_SettingsView.SetTilt(m_Settings.ErgonomicTilt);
                    break;
                case HostMessageType.Rebasing:
                    m_ResetView.SetRebasing(m_Settings.Rebasing);
                    break;
                case HostMessageType.MotionScale:
                    m_PositionView.SetMotionScale(m_Settings.MotionScale);
                    break;
                case HostMessageType.JoystickSensitivity:
                    m_JoysticksSettingsView.SetSensitivity(m_Settings.JoystickSensitivity);
                    break;
                case HostMessageType.PedestalSpace:
                    m_JoysticksSettingsView.SetPedestalSpace(m_Settings.PedestalSpace);
                    m_GamepadSettingsView.SetPedestalSpace(m_Settings.PedestalSpace);
                    break;
                case HostMessageType.MotionSpace:
                    m_JoysticksSettingsView.SetMotionSpace(m_Settings.MotionSpace);
                    m_GamepadSettingsView.SetMotionSpace(m_Settings.MotionSpace);
                    break;
                case HostMessageType.FocusReticlePosition:
                    SetReticlePosition();
                    break;
                case HostMessageType.FocusDistanceOffset:
                    SetFocusDistanceOffset();
                    break;
                case HostMessageType.CropAspect:
                    // TODO not handled yet.
                    break;
                case HostMessageType.ShowGateMask:
                    m_SettingsView.SetGateMaskEnabled(m_Settings.GateMask);
                    break;
                case HostMessageType.ShowFrameLines:
                    m_SettingsView.SetFrameLinesEnabled(m_Settings.AspectRatioLines);
                    break;
                case HostMessageType.ShowCenterMarker:
                    m_SettingsView.SetCenterMarkerEnabled(m_Settings.CenterMarker);
                    break;
                case HostMessageType.ShowFocusPlane:
                    m_SettingsView.SetFocusPlaneEnabled(m_Settings.FocusPlane);
                    break;
                case HostMessageType.SetPoseToOrigin:
                    // TODO, not expected on the client, throw for the time being.
                    break;
                case HostMessageType.SensorSize:
                    SetSensorSize();
                    break;
                case HostMessageType.Iso:
                    break;
                case HostMessageType.ShutterSpeed:
                    break;
                case HostMessageType.LensIntrinsics:
                    SetLensIntrinsics();
                    break;
                case HostMessageType.FocalLength:
                    SetFocalLength();
                    break;
                case HostMessageType.FocusDistance:
                    SetFocusDistance();
                    break;
                case HostMessageType.Aperture:
                    SetAperture();
                    break;
                case HostMessageType.FocusMode:
                    SetFocusMode();
                    break;
                case HostMessageType.ChannelFlags:
                    SetChannelFlags();
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled {signal.value} in {nameof(ReceiveSettingCommand)}.");
            }
        }

        void SetSensorSize()
        {
            foreach (var listener in m_SensorSizeListeners)
            {
                listener.SetSensorSize(m_Camera.Body.SensorSize);
            }
        }

        void SetFocalLength()
        {
            foreach (var listener in m_FocalLengthListeners)
            {
                listener.SetFocalLength(
                    m_Camera.Lens.FocalLength,
                    m_Camera.Intrinsics.FocalLengthRange);
            }
        }

        void SetFocusDistance()
        {
            foreach (var listener in m_FocusDistanceListeners)
            {
                listener.SetFocusDistance(
                    m_Camera.Lens.FocusDistance,
                    new Vector2(m_Camera.Intrinsics.CloseFocusDistance, LensLimits.FocusDistance.y));
            }
        }

        void SetFocusDistanceOffset()
        {
            foreach (var listener in m_FocusDistanceOffsetListeners)
            {
                listener.SetFocusDistanceOffset(m_Settings.FocusDistanceOffset);
            }
        }

        void SetAperture()
        {
            foreach (var listener in m_ApertureListeners)
            {
                listener.SetAperture(
                    m_Camera.Lens.Aperture,
                    m_Camera.Intrinsics.ApertureRange);
            }
        }

        void SetFocusMode()
        {
            foreach (var listener in m_FocusModeListeners)
            {
                listener.SetFocusMode(m_Settings.FocusMode);
            }
        }

        void SetLensIntrinsics()
        {
            foreach (var listener in m_LensIntrinsicsListeners)
            {
                listener.SetLensIntrinsics(m_Camera.Intrinsics);
            }
        }

        void SetReticlePosition()
        {
            foreach (var listener in m_ReticlePositionListeners)
            {
                listener.SetReticlePosition(m_Settings.ReticlePosition);
            }
        }

        void SetChannelFlags()
        {
            foreach (var listener in m_ChannelFlagsListeners)
            {
                listener.SetChannelFlags(m_State.ChannelFlags);
            }
        }
    }

    class ReceiveSettingCommand2
    {
        [Inject]
        StateModel m_State;
        [Inject]
        SettingsModel m_Settings;
        [Inject]
        CameraModel m_Camera;
        [Inject]
        ISettingsView m_SettingsView;
        [Inject]
        IDampingView m_DampingView;
        [Inject]
        IPositionView m_PositionView;
        [Inject]
        IRotationLockView m_RotationLockView;
        [Inject]
        IResetView m_ResetView;
        [Inject]
        IJoystickGeneralTabView m_JoysticksSettingsView;
        [Inject]
        IGamepadConfigurationTabView m_GamepadSettingsView;
        [Inject]
        List<IFocalLengthListener> m_FocalLengthListeners;
        [Inject]
        List<IFocusDistanceListener> m_FocusDistanceListeners;
        [Inject]
        List<IFocusDistanceOffsetListener> m_FocusDistanceOffsetListeners;
        [Inject]
        List<IApertureListener> m_ApertureListeners;
        [Inject]
        List<IFocusModeListener> m_FocusModeListeners;
        [Inject]
        List<ILensIntrinsicsListener> m_LensIntrinsicsListeners;
        [Inject]
        List<ISensorSizeListener> m_SensorSizeListeners;
        [Inject]
        List<IReticlePositionListener> m_ReticlePositionListeners;
        [Inject]
        List<IChannelFlagsListener> m_ChannelFlagsListeners;

        public void Execute(ReceiveHostSignal signal)
        {
            switch (signal.value)
            {
                case HostMessageType.DampingEnabled:
                    m_DampingView.SetDampingEnabled(m_Settings.DampingEnabled);
                    break;
                case HostMessageType.BodyDamping:
                    m_DampingView.SetBodyDamping(m_Settings.BodyDamping);
                    break;
                case HostMessageType.AimDamping:
                    m_DampingView.SetAimDamping(m_Settings.AimDamping);
                    break;
                case HostMessageType.FocalLengthDamping:
                    m_SettingsView.SetFocalLengthDamping(m_Settings.FocalLengthDamping);
                    break;
                case HostMessageType.FocusDistanceDamping:
                    m_SettingsView.SetFocusDistanceDamping(m_Settings.FocusDistanceDamping);
                    break;
                case HostMessageType.ApertureDamping:
                    m_SettingsView.SetApertureDamping(m_Settings.ApertureDamping);
                    break;
                case HostMessageType.PositionLock:
                    m_PositionView.SetPositionLock(m_Settings.PositionLock);
                    break;
                case HostMessageType.RotationLock:
                    m_RotationLockView.SetRotationLock(m_Settings.RotationLock);
                    break;
                case HostMessageType.AutoHorizon:
                    m_RotationLockView.SetAutoHorizon(m_Settings.AutoHorizon);
                    break;
                case HostMessageType.ErgonomicTilt:
                    m_SettingsView.SetTilt(m_Settings.ErgonomicTilt);
                    break;
                case HostMessageType.Rebasing:
                    m_ResetView.SetRebasing(m_Settings.Rebasing);
                    break;
                case HostMessageType.MotionScale:
                    break;
                case HostMessageType.JoystickSensitivity:
                    m_JoysticksSettingsView.SetSensitivity(m_Settings.JoystickSensitivity);
                    break;
                case HostMessageType.PedestalSpace:
                    m_JoysticksSettingsView.SetPedestalSpace(m_Settings.PedestalSpace);
                    m_GamepadSettingsView.SetPedestalSpace(m_Settings.PedestalSpace);
                    break;
                case HostMessageType.MotionSpace:
                    m_JoysticksSettingsView.SetMotionSpace(m_Settings.MotionSpace);
                    m_GamepadSettingsView.SetMotionSpace(m_Settings.MotionSpace);
                    break;
                case HostMessageType.FocusReticlePosition:
                    SetReticlePosition();
                    break;
                case HostMessageType.FocusDistanceOffset:
                    SetFocusDistanceOffset();
                    break;
                case HostMessageType.CropAspect:
                    break;
                case HostMessageType.ShowGateMask:
                    m_SettingsView.SetGateMaskEnabled(m_Settings.GateMask);
                    break;
                case HostMessageType.ShowFrameLines:
                    m_SettingsView.SetFrameLinesEnabled(m_Settings.AspectRatioLines);
                    break;
                case HostMessageType.ShowCenterMarker:
                    m_SettingsView.SetCenterMarkerEnabled(m_Settings.CenterMarker);
                    break;
                case HostMessageType.ShowFocusPlane:
                    m_SettingsView.SetFocusPlaneEnabled(m_Settings.FocusPlane);
                    break;
                case HostMessageType.SetPoseToOrigin:
                    break;
                case HostMessageType.SensorSize:
                    SetSensorSize();
                    break;
                case HostMessageType.Iso:
                    break;
                case HostMessageType.ShutterSpeed:
                    break;
                case HostMessageType.LensIntrinsics:
                    SetLensIntrinsics();
                    break;
                case HostMessageType.FocalLength:
                    SetFocalLength();
                    break;
                case HostMessageType.FocusDistance:
                    SetFocusDistance();
                    break;
                case HostMessageType.Aperture:
                    SetAperture();
                    break;
                case HostMessageType.FocusMode:
                    SetFocusMode();
                    break;
                case HostMessageType.ChannelFlags:
                    SetChannelFlags();
                    break;
                default:
                    Debug.LogWarning($"Unhandled {signal.value} in {nameof(ReceiveSettingCommand)}.");
                    break;
            }
        }

        void SetSensorSize()
        {
            foreach (var listener in m_SensorSizeListeners)
            {
                listener.SetSensorSize(m_Camera.Body.SensorSize);
            }
        }

        void SetFocalLength()
        {
            foreach (var listener in m_FocalLengthListeners)
            {
                listener.SetFocalLength(
                    m_Camera.Lens.FocalLength,
                    m_Camera.Intrinsics.FocalLengthRange);
            }
        }

        void SetFocusDistance()
        {
            foreach (var listener in m_FocusDistanceListeners)
            {
                listener.SetFocusDistance(
                    m_Camera.Lens.FocusDistance,
                    new Vector2(m_Camera.Intrinsics.CloseFocusDistance, LensLimits.FocusDistance.y));
            }
        }

        void SetFocusDistanceOffset()
        {
            foreach (var listener in m_FocusDistanceOffsetListeners)
            {
                listener.SetFocusDistanceOffset(m_Settings.FocusDistanceOffset);
            }
        }

        void SetAperture()
        {
            foreach (var listener in m_ApertureListeners)
            {
                listener.SetAperture(
                    m_Camera.Lens.Aperture,
                    m_Camera.Intrinsics.ApertureRange);
            }
        }

        void SetFocusMode()
        {
            foreach (var listener in m_FocusModeListeners)
            {
                listener.SetFocusMode(m_Settings.FocusMode);
            }
        }

        void SetLensIntrinsics()
        {
            foreach (var listener in m_LensIntrinsicsListeners)
            {
                listener.SetLensIntrinsics(m_Camera.Intrinsics);
            }
        }

        void SetDeviceMode()
        {
            /*
            if (m_State.DeviceMode == DeviceMode.Playback)
            {
                m_SignalBus.Fire(new HideMainViewSignal());
            }
            */
        }

        void SetReticlePosition()
        {
            foreach (var listener in m_ReticlePositionListeners)
            {
                listener.SetReticlePosition(m_Settings.ReticlePosition);
            }
        }

        void SetChannelFlags()
        {
            foreach (var listener in m_ChannelFlagsListeners)
            {
                listener.SetChannelFlags(m_State.ChannelFlags);
            }
        }
    }
}
