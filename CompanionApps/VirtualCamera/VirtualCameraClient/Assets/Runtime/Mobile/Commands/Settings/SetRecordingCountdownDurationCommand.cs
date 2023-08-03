using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SetRecordingCountdownDurationCommand
    {
        [Inject]
        DeviceDataSystem m_DeviceDataSystem;

        public void Execute(SetRecordingCountdownDurationSignal signal)
        {
            var data = m_DeviceDataSystem.deviceData;
            data.countdownDuration = signal.value;
            m_DeviceDataSystem.deviceData = data;
        }
    }
}
