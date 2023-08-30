using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class HostRecordingChangedCommand : IRecordingStateListener
    {
        [Inject]
        ITimeSystem m_TimeSystem;
        [Inject]
        SignalBus m_SignalBus;

        public void SetRecordingState(bool isRecording)
        {
            if (isRecording)
            {
                m_TimeSystem.Reset();
            }
        }
    }
}
