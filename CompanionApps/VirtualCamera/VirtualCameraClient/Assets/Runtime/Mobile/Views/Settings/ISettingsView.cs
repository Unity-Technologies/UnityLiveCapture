using System;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    interface ISettingsView : IDialogView
    {
        event Action DoneClicked;
        event Action PrivacyPolicyClicked;
        event Action DocumentationClicked;
        event Action SupportClicked;

        event Action<string> TimecodeSourceChanged;

        event Action<bool> JoysticksToggled;
        event Action<bool> CameraSettingsToggled;
        event Action<bool> LensSettingsToggled;
        event Action<bool> InformationBarToggled;

        event Action<bool> GateMaskToggled;
        event Action<bool> FrameLinesToggled;
        event Action<bool> CenterMarkerToggled;
        event Action<bool> FocusPlaneToggled;

        event Action SetToCurrentTiltClicked;
        event Action ResetTiltClicked;
        event Action<float> TiltChanged;

        event Action<float> FocusDistanceDampingChanged;
        event Action<float> FocalLengthDampingChanged;
        event Action<float> ApertureDampingChanged;

        event Action<bool> CountdownEnableToggled;
        event Action<int> CountdownTimeChanged;

        void SetGateMaskEnabled(bool on);
        void SetFrameLinesEnabled(bool on);
        void SetCenterMarkerEnabled(bool on);
        void SetFocusPlaneEnabled(bool on);

        void UpdateDeviceData(DeviceData deviceData);
        void SetOptions(MainViewOptions options);

        void SetTilt(float value);

        void SetFocusDistanceDamping(float value);
        void SetFocalLengthDamping(float value);
        void SetApertureDamping(float value);

        void CollapseAllSections();
    }
}
