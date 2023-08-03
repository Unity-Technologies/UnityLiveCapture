using System;
using UnityEngine.UI;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class CameraLensSettingsMediator : IInitializable
    {
        [Inject]
        StateModel m_State;
        [Inject]
        ICameraLensSettingsView m_CameraLensView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_CameraLensView.FocalLengthValueChanged += OnFocalLengthValueChanged;
            m_CameraLensView.ApertureValueChanged += OnApertureValueChanged;
            m_CameraLensView.FocusDistanceValueChanged += OnFocusDistanceValueChanged;
            m_CameraLensView.CancelClicked += OnCancelClicked;
            m_CameraLensView.Toggled += OnToggle;
        }

        void OnCancelClicked()
        {
            m_SignalBus.Fire(new LensSettingsViewSignals.Close());
        }

        void OnToggle(Toggle toggle, LensSettingsViewId id)
        {
            if (m_State.IsHelpMode)
            {
                if (toggle.isOn)
                {
                    m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = GetHelpTooltipId(id)});
                }
                else
                {
                    m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());
                }
            }
        }

        HelpTooltipId GetHelpTooltipId(LensSettingsViewId id)
        {
            switch (id)
            {
                case LensSettingsViewId.FocalLength:
                    return HelpTooltipId.FocalLength;
                case LensSettingsViewId.FocusDistance:
                    return HelpTooltipId.FocusDistance;
                case LensSettingsViewId.Aperture:
                    return HelpTooltipId.Aperture;
                case LensSettingsViewId.FocusMode:
                    return HelpTooltipId.FocusMode;
            }

            throw new ArgumentException($"Invalid id: {id}");
        }

        void OnFocalLengthValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocalLength,
                FloatValue = value
            });
        }

        void OnApertureValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.Aperture,
                FloatValue = value
            });
        }

        void OnFocusDistanceValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocusDistance,
                FloatValue = value
            });
        }
    }
}
