using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SetRecordingCountdownEnabledCommand
    {
        [Inject]
        DeviceDataSystem m_DeviceDataSystem;

        public void Execute(SetRecordingCountdownEnabledSignal signal)
        {
            var data = m_DeviceDataSystem.deviceData;
            data.isCountdownEnabled = signal.value;
            m_DeviceDataSystem.deviceData = data;
        }
    }
}
