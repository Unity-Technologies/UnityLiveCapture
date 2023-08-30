using System;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    enum LensSettingsViewId
    {
        None,
        FocalLength,
        FocusDistance,
        Aperture,
        FocusMode
    }

    interface ICameraLensSettingsView :
        IDialogView,
        IFocalLengthListener,
        IFocusDistanceListener,
        IApertureListener,
        IFocusModeListener,
        IHelpModeListener
    {
        event Action<Toggle, LensSettingsViewId> Toggled;
        event Action CancelClicked;
        event Action<float> FocalLengthValueChanged;
        event Action<float> FocusDistanceValueChanged;
        event Action<float> ApertureValueChanged;

        void ToggleOff();
    }
}
