using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    // TODO harmonize main-view sub-views handling and remove these specialized signal-command pairs.
    class ShowTakeIterationOrHelpCommand
    {
        [Inject]
        StateModel m_State;

        [Inject]
        TakeIterationToggle m_Toggle;

        [Inject]
        SignalBus m_SignalBus;

        public void Execute(ShowTakeIterationOrHelpSignal signal)
        {
            m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());

            if (signal.value && m_State.IsHelpMode)
            {
                m_Toggle.SetIsOnWithoutNotify(false);
                m_SignalBus.Fire(new HelpModeSignals.OpenTooltip() { value = HelpTooltipId.TakeIteration });
                return;
            }

            m_SignalBus.Fire(new ShowTakeIterationSignal(){value = signal.value});
        }
    }


    class ShowTakeIterationCommand
    {
        [Inject]
        ITakeIterationView m_View;

        [InjectOptional]
        TakeIterationToggle m_Toggle;

        public void Execute(ShowTakeIterationSignal signal)
        {
            if (m_Toggle != null)
            {
                m_Toggle.SetIsOnWithoutNotify(signal.value);
            }

            if (signal.value)
            {
                m_View.Show();
            }
            else
            {
                m_View.Hide();
            }
        }
    }
}
