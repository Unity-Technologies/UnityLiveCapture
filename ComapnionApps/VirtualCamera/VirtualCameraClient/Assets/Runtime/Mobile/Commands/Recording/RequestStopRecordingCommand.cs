using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class RequestStopRecordingCommand
    {
        [Inject]
        ICountdownView m_CountdownView;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        [Inject]
        SignalBus m_SignalBus;

        public void Execute(RequestStopRecordingSignal signal)
        {
            m_CountdownView.Hide();

            m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());

            m_CompanionApp.StopRecording();
        }
    }
}
