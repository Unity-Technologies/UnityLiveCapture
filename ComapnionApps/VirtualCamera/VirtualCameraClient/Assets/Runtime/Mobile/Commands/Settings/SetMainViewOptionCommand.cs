using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SetMainViewOptionCommand
    {
        [Inject]
        DeviceDataSystem m_DeviceDataSystem;
        [Inject]
        IMainView m_MainView;
        [Inject]
        ISettingsView m_SettingsView;

        public void Execute(SetMainViewOptionSignal signal)
        {
            var data = m_DeviceDataSystem.deviceData;

            if (signal.value.Item2)
            {
                data.mainViewOptions |= signal.value.Item1;
            }
            else
            {
                data.mainViewOptions &= ~signal.value.Item1;
            }

            m_DeviceDataSystem.deviceData = data;

            m_MainView.SetOptions(data.mainViewOptions);
            m_SettingsView.SetOptions(data.mainViewOptions);
        }
    }
}
