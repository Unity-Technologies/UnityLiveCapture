using UnityEngine.Assertions;
using Unity.CompanionAppCommon;
using Unity.LiveCapture.CompanionApp;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class DisplayMainViewCommand : IRecordingStateListener, IDeviceModeListener
    {
        [Inject]
        IMainViewTablet m_MainView;
        [Inject]
        IRigSettingsView m_RigSettingsView;
        [Inject]
        IFocalLengthView m_FocalDialView;
        [Inject]
        IJoystickSettingsView m_JoysticksView;
        [Inject]
        IGamepadSettingsView m_GamepadView;
        [Inject]
        IFocusDistanceView m_FocusDistanceView;
        [Inject]
        IFocusModeView m_FocusModeView;
        [Inject]
        ISettingsView m_SettingsView;
        [Inject]
        IResetView m_ResetView;
        [Inject]
        ITakeIterationView m_TakeIterationView;
        [Inject]
        ITakeSelectionView m_TakeSelectionView;
        [Inject]
        StateModel m_StateModel;
        [Inject]
        SignalBus m_SignalBus;

        public void SetDeviceMode(DeviceMode mode)
        {
            if (mode == DeviceMode.Playback)
            {
                HideAllViews();
            }
        }

        public void SetRecordingState(bool isRecording)
        {
            if (isRecording && m_MainView.ActiveToggle != MainViewId.FocalLength && m_MainView.ActiveToggle != MainViewId.Focus)
            {
                HideAllViews();
            }
        }

        public void Show(MainViewId mainViewId)
        {
            HideAllViews();

            m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());

            if (m_StateModel.IsHelpMode)
            {
                var id = GetHelpTooltipId(mainViewId);
                if (id != HelpTooltipId.None)
                {
                    m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = id });
                }
            }

            if (TryGetView(mainViewId, out var view))
            {
                if (m_MainView.TryGetPosition(mainViewId, out var position, out var pivot))
                {
                    view.Position = position;
                    view.Pivot = pivot;
                }

                if (m_MainView.TryGetSize(mainViewId, out var size))
                {
                    view.Size = size;
                }

                view.Show();
            }

            m_MainView.ActiveToggle = mainViewId;
        }

        HelpTooltipId GetHelpTooltipId(MainViewId id)
        {
            // TODO add Joysticks Settings.
            switch (id)
            {
                case MainViewId.Focus:
                    return HelpTooltipId.FocusAperture;
                case MainViewId.Reset:
                    return HelpTooltipId.Reset;
                case MainViewId.Settings:
                    return HelpTooltipId.Settings;
                case MainViewId.FocalLength:
                    return HelpTooltipId.FocalLength;
                case MainViewId.FocusMode:
                    return HelpTooltipId.FocusMode;
                case MainViewId.JoystickSettings:
                    return HelpTooltipId.Joysticks;
                case MainViewId.GamepadSettings:
                    return HelpTooltipId.Gamepad;
                case MainViewId.TakeIteration:
                    return HelpTooltipId.TakeIteration;
                default:
                    //Debug.LogError($"No {nameof(DialogType)} matching {nameof(MainViewId)}: {id}.");
                    return HelpTooltipId.None;
            }
        }

        // TODO remove the fault tolerant pattern once implementation is complete
        // same for TryGetPosition, etc...
        bool TryGetView(MainViewId id, out IDialogView view)
        {
            switch (id)
            {
                case MainViewId.Reset:
                    view = m_ResetView;
                    return true;
                case MainViewId.RigSettings:
                    view = m_RigSettingsView;
                    return true;
                case MainViewId.JoystickSettings:
                    view = m_JoysticksView;
                    return true;
                case MainViewId.GamepadSettings:
                    view = m_GamepadView;
                    return true;
                case MainViewId.FocalLength:
                    view = m_FocalDialView;
                    return true;
                case MainViewId.Focus:
                    view = m_FocusDistanceView;
                    return true;
                case MainViewId.FocusMode:
                    view = m_FocusModeView;
                    return true;
                case MainViewId.Settings:
                    view = m_SettingsView;
                    return true;
                case MainViewId.TakeIteration:
                    view = m_TakeIterationView;
                    return true;
                default:
                    view = null;
                    return false;
            }
        }

        public void Hide(HideMainViewSignal signal) => HideAllViews();

        public void Hide(OrphanTouchSignal signal)
        {
            // This method should be the first invoked, reticle second.
            Assert.IsFalse(signal.stopPropagation);

            // If a dialog is opened, close it and intercept signal propagation.
            signal.stopPropagation = m_MainView.ActiveToggle != MainViewId.None;

            HideAllViews();
        }

        public void HideAllViews()
        {
            if (m_StateModel.IsHelpMode)
            {
                m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());
            }

            m_RigSettingsView.Hide();
            m_JoysticksView.Hide();
            m_GamepadView.Hide();
            m_FocusModeView.Hide();
            m_SettingsView.Hide();
            m_ResetView.Hide();
            m_TakeIterationView.Hide();
            m_TakeSelectionView.Hide();
            m_FocalDialView.Hide();
            m_FocusDistanceView.Hide();

            m_MainView.ActiveToggle = MainViewId.None;
        }
    }
}
