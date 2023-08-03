using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class RequestStartRecordingCommand
    {
        [Inject]
        ICountdownView m_CountdownView;
        [Inject]
        DeviceDataSystem m_DeviceDataSystem;
        [Inject]
        IRemoteSystem m_Remote;
        [Inject]
        IMainView m_MainView;
        [Inject]
        SignalBus m_SignalBus;

        public void Execute(RequestStartRecordingSignal signal)
        {
            var isCountdownActive = m_DeviceDataSystem.deviceData.isCountdownEnabled;
            var seconds = m_DeviceDataSystem.deviceData.countdownDuration;
            var isCountdownPlaying = m_CountdownView.IsPlaying;

            m_CountdownView.Hide();

            if (isCountdownActive && seconds > 0f)
            {
                if (isCountdownPlaying)
                {
                    m_CountdownView.Hide();

                    m_MainView.SetRecordingState(false);
                }
                else
                {
                    m_CountdownView.Show();
                    m_CountdownView.PlayCountdown(seconds);

                    m_MainView.SetRecordingState(true);
                }
            }
            else
            {
                if (m_Remote.Host != null)
                {
                    m_Remote.Host.StartRecording();
                }
            }

            m_SignalBus.Fire(new HelpModeSignals.CloseTooltip());
        }
    }
}
