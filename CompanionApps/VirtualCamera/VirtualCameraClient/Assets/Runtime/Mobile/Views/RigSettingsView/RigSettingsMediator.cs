using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    public class RigSettingsMediator : IInitializable
    {
        [Inject]
        StateModel m_State;
        [Inject]
        IRigSettingsView m_RigSettingsView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_RigSettingsView.onDoneClicked += OnDoneClicked;
            m_RigSettingsView.onDampingViewShowed += OnDampingViewShowed;
            m_RigSettingsView.onPositionViewShowed += OnPositionViewShowed;
            m_RigSettingsView.onRotationViewShowed += OnRotationViewShowed;
        }

        void OnDoneClicked()
        {
            // Used on mobile only.
            m_SignalBus.Fire(new RigSettingsViewSignals.Close());
        }

        void OnDampingViewShowed()
        {
            if (m_State.IsHelpMode)
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.Damping });
        }

        void OnPositionViewShowed()
        {
            if (m_State.IsHelpMode)
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.MotionScale });
        }

        void OnRotationViewShowed()
        {
            if (m_State.IsHelpMode)
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.AxisLockRotation});
        }
    }
}
