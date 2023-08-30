using System;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    public class AppDisposer : IDisposable
    {
        [Inject]
        SignalBus m_SignalBus;

        [Inject]
        DeviceDataSystem m_DeviceDataSystem;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        public void Dispose()
        {
            m_DeviceDataSystem.SaveData();

            m_CompanionApp.StopRecording();
        }
    }
}
