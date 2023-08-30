using System;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SendSettingCommand
    {
        [Inject]
        IRemoteSystem m_Remote;
        [Inject]
        ITimeSystem m_TimeSystem;
        [Inject]
        SettingsModel m_Settings;

        public void Execute(SendHostSignal signal)
        {
            var host = m_Remote.Host as VirtualCameraHost;

            if (host == null)
            {
                return;
            }

            switch (signal.Type)
            {
                case HostMessageType.DampingEnabled:
                    host.SendDampingEnabled(signal.BoolValue);
                    break;
                case HostMessageType.BodyDamping:
                    host.SendBodyDamping(signal.Vector3Value);
                    break;
                case HostMessageType.AimDamping:
                    host.SendAimDamping(signal.FloatValue);
                    break;
                case HostMessageType.FocalLengthDamping:
                    host.SendFocalLengthDamping(signal.FloatValue);
                    break;
                case HostMessageType.FocusDistanceDamping:
                    host.SendFocusDistanceDamping(signal.FloatValue);
                    break;
                case HostMessageType.ApertureDamping:
                    host.SendApertureDamping(signal.FloatValue);
                    break;
                case HostMessageType.PositionLock:
                    host.SendPositionLock(signal.PositionAxisValue);
                    break;
                case HostMessageType.RotationLock:
                    host.SendRotationLock(signal.RotationAxisValue);
                    break;
                case HostMessageType.AutoHorizon:
                    host.SendAutoHorizon(signal.BoolValue);
                    break;
                case HostMessageType.ErgonomicTilt:
                    host.SendErgonomicTilt(signal.FloatValue);
                    break;
                case HostMessageType.Rebasing:
                    host.SendRebasing(signal.BoolValue);
                    break;
                case HostMessageType.MotionScale:
                    host.SendMotionScale(signal.Vector3Value);
                    break;
                case HostMessageType.JoystickSensitivity:
                    host.SendJoystickSensitivity(signal.Vector3Value);
                    break;
                case HostMessageType.PedestalSpace:
                    host.SendPedestalSpace(signal.SpaceValue);
                    break;
                case HostMessageType.MotionSpace:
                    host.SendMotionSpace(signal.SpaceValue);
                    break;
                case HostMessageType.FocusMode:
                    host.SendFocusMode(signal.FocusModeValue);
                    break;
                case HostMessageType.FocusReticlePosition:
                    host.SendFocusReticlePosition(signal.Vector2Value);
                    break;
                case HostMessageType.FocusDistanceOffset:
                    host.SendFocusDistanceOffset(signal.FloatValue);
                    break;
                case HostMessageType.CropAspect:
                    host.SendCropAspect(signal.FloatValue);
                    break;
                case HostMessageType.ShowGateMask:
                    host.SendShowGateMask(signal.BoolValue);
                    break;
                case HostMessageType.ShowFrameLines:
                    host.SendShowFrameLines(signal.BoolValue);
                    break;
                case HostMessageType.ShowCenterMarker:
                    host.SendShowCenterMarker(signal.BoolValue);
                    break;
                case HostMessageType.ShowFocusPlane:
                    host.SendShowFocusPlane(signal.BoolValue);
                    break;
                case HostMessageType.SetPoseToOrigin:
                    host.SetPoseToOrigin();
                    break;
                case HostMessageType.FocalLength:
                    host.SendFocalLength(new FocalLengthSample
                    {
                        FocalLength = signal.FloatValue,
                        Time = m_TimeSystem.Time
                    });
                    break;
                case HostMessageType.FocusDistance:
                    host.SendFocusDistance(new FocusDistanceSample
                    {
                        FocusDistance = signal.FloatValue,
                        Time = m_TimeSystem.Time
                    });
                    ExitClearFocusModeIfNeeded(host);
                    break;
                case HostMessageType.Aperture:
                    host.SendAperture(new ApertureSample
                    {
                        Aperture = signal.FloatValue,
                        Time = m_TimeSystem.Time
                    });
                    ExitClearFocusModeIfNeeded(host);
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled {signal.Type} in {nameof(SendSettingCommand)}.");
            }
        }

        // Automatically activate focus when we change related values.
        void ExitClearFocusModeIfNeeded(VirtualCameraHost host)
        {
            if (m_Settings.FocusMode == FocusMode.Clear)
            {
                host.SendFocusMode(FocusMode.Manual);
            }
        }
    }
}
