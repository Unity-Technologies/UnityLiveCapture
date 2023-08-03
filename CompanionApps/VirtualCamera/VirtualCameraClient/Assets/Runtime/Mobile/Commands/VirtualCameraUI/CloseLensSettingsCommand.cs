using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class CloseLensSettingsCommand
    {
        [Inject]
        ICameraLensSettingsView m_CameraLensSettingsView;

        public void Execute(OrphanTouchSignal signal)
        {
            if (signal.type == OrphanTouchType.PointerDown)
            {
                m_CameraLensSettingsView.ToggleOff();
            }
        }
    }
}
