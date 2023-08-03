using System;
using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SettingsMediator : IInitializable
    {
        [Inject]
        ISettingsView m_SettingsView;

        [Inject]
        SignalBus m_SignalBus;

        [Inject]
        IMainView m_MainView;

        public void Initialize()
        {
            m_SettingsView.DoneClicked += CloseSettings;

            m_SettingsView.PrivacyPolicyClicked += OnPrivacyPolicyClick;
            m_SettingsView.DocumentationClicked += OnDocumentationClick;
            m_SettingsView.SupportClicked += OnSupportClick;

            m_SettingsView.JoysticksToggled += OnJoysticksToggle;
            m_SettingsView.CameraSettingsToggled += OnCameraSettingsToggle;
            m_SettingsView.LensSettingsToggled += OnLensSettingsToggle;
            m_SettingsView.InformationBarToggled += OnInformationBarToggle;

            m_SettingsView.GateMaskToggled += OnShowGateMaskEnabled;
            m_SettingsView.FrameLinesToggled += OnShowFrameLinesEnabled;
            m_SettingsView.CenterMarkerToggled += OnShowCenterMarkerEnabled;
            m_SettingsView.FocusPlaneToggled += OnShowFocusPlaneEnabled;

            m_SettingsView.ResetTiltClicked += ResetTilt;
            m_SettingsView.SetToCurrentTiltClicked += SetToCurrentTilt;
            m_SettingsView.TiltChanged += SetTiltValue;

            m_SettingsView.FocusDistanceDampingChanged += OnFocusDistanceDampingChanged;
            m_SettingsView.FocalLengthDampingChanged += OnFocalLengthDampingChanged;
            m_SettingsView.ApertureDampingChanged += OnApertureDampingChanged;

            m_SettingsView.CountdownEnableToggled += OnRecordingCountdownEnabled;
            m_SettingsView.CountdownTimeChanged += OnCountdownTimeChanged;

            m_SettingsView.TimecodeSourceChanged += OnTimecodeSourceChanged;
        }

        void OnTimecodeSourceChanged(string value)
        {
            m_SignalBus.Fire(new SetTimecodeSourceSignal { Id = value });
        }

        void OnPrivacyPolicyClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenPrivacyPolicy());
        }

        void OnDocumentationClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenDocumentation());
        }

        void OnSupportClick()
        {
            m_SignalBus.Fire(new HelpSignals.OpenSupport());
        }

        void OnJoysticksToggle(bool on) => SendMainViewOption(MainViewOptions.Joysticks, on);
        void OnCameraSettingsToggle(bool on) => SendMainViewOption(MainViewOptions.CameraSettings, on);
        void OnLensSettingsToggle(bool on) => SendMainViewOption(MainViewOptions.LensSettings, on);
        void OnInformationBarToggle(bool on) => SendMainViewOption(MainViewOptions.InformationBar, on);

        void SendMainViewOption(MainViewOptions option, bool value)
        {
            m_SignalBus.Fire(new SetMainViewOptionSignal() { value = (option, value) });
        }

        void OnShowGateMaskEnabled(bool on) => SendCameraOption(HostMessageType.ShowGateMask, on);
        void OnShowFrameLinesEnabled(bool on) => SendCameraOption(HostMessageType.ShowFrameLines, on);
        void OnShowCenterMarkerEnabled(bool on) => SendCameraOption(HostMessageType.ShowCenterMarker, on);
        void OnShowFocusPlaneEnabled(bool on) => SendCameraOption(HostMessageType.ShowFocusPlane, on);

        void SendCameraOption(HostMessageType option, bool value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = option,
                BoolValue = value
            });
        }

        void OnFocusDistanceDampingChanged(float value) => SendLensDamping(HostMessageType.FocusDistanceDamping, value);

        void OnFocalLengthDampingChanged(float value) => SendLensDamping(HostMessageType.FocalLengthDamping, value);

        void OnApertureDampingChanged(float value) => SendLensDamping(HostMessageType.ApertureDamping, value);

        void SetLensDamping(HostMessageType type, float value)
        {
            switch (type)
            {
                case HostMessageType.FocusDistanceDamping:
                    m_SettingsView.SetFocusDistanceDamping(value);
                    return;
                case HostMessageType.FocalLengthDamping:
                    m_SettingsView.SetFocalLengthDamping(value);
                    return;
                case HostMessageType.ApertureDamping:
                    m_SettingsView.SetApertureDamping(value);
                    return;
            }

            throw new InvalidOperationException($"HostMessageType {type} is not Lens Damping.");
        }

        void SendLensDamping(HostMessageType type, float value)
        {
            // The server won't send the value back so we need local validation.
            var clampedValue = Mathf.Clamp01(value);
            if (clampedValue != value)
            {
                SetLensDamping(type, clampedValue);
            }

            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = type,
                FloatValue = clampedValue
            });
        }

        void OnRecordingCountdownEnabled(bool on)
        {
            m_SignalBus.Fire(new SetRecordingCountdownEnabledSignal() {value = on});
        }

        void OnCountdownTimeChanged(int time)
        {
            m_SignalBus.Fire(new SetRecordingCountdownDurationSignal() {value = time});
        }

        void ResetTilt()
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.ErgonomicTilt,
                FloatValue = 0
            });
        }

        void SetToCurrentTilt()
        {
            m_SignalBus.Fire(new SetToCurrentTiltSignal());
        }

        void SetTiltValue(float value)
        {
            m_SettingsView.SetTilt(value);

            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.ErgonomicTilt,
                FloatValue = value
            });
        }

        void CloseSettings()
        {
            m_SignalBus.Fire(new SettingsViewSignals.Close());
        }
    }
}
