using Zenject;

namespace Unity.CompanionAppCommon
{
    class CountdownMediator : IInitializable, IRecordingStateListener
    {
        [Inject]
        ICountdownView m_CountdownView;
        [Inject]
        IRemoteSystem m_Remote;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_CountdownView.countdownCompleted += OnCountdownCompleted;
        }

        public void SetRecordingState(bool isRecording)
        {
            m_CountdownView.Hide();
        }

        void OnCountdownCompleted()
        {
            if (m_Remote.Host != null)
            {
                m_Remote.Host.StartRecording();
            }
            else
            {
                m_CountdownView.Hide();
            }
        }
    }
}
