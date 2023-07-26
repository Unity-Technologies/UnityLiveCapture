using UnityEngine;
using Unity.LiveCapture;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class TimeControlViewMediator : IInitializable
    {
        [Inject]
        Platform m_Platform;
        [Inject]
        StateModel m_State;
        [Inject]
        ICompanionAppHost m_CompanionApp;
        [Inject]
        ITimeControlView m_TimeControlView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_TimeControlView.PlayButtonClicked += OnPlayButtonClicked;
            m_TimeControlView.ValueChanged += OnValueChanged;
            m_TimeControlView.FrameSkipButtonClicked += OnFrameSkipClicked;
        }

        void OnPlayButtonClicked()
        {
            DispatchHelpIfNeeded();

            m_SignalBus.Fire(new TogglePlaybackSignal());
        }

        void OnValueChanged(float value)
        {
            DispatchHelpIfNeeded();

            m_CompanionApp.SetPlaybackTime(value);
        }

        void OnFrameSkipClicked(int framesToSkip)
        {
            DispatchHelpIfNeeded();

            m_SignalBus.Fire(new SkipFramesSignal() { value = framesToSkip });
        }

        void DispatchHelpIfNeeded()
        {
            if (m_State.IsHelpMode)
            {
                // TODO main-view display of sub-views needs harmonization.
                if (m_Platform == Platform.Tablet)
                {
                    m_SignalBus.Fire(new ShowMainViewSignal { value = MainViewId.None });
                }

                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.PlayerScrubber });
            }
        }
    }
}
