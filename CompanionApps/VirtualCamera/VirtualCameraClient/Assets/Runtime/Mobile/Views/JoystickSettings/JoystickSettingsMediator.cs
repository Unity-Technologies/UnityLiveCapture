using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class JoystickSettingsMediator : IInitializable
    {
        [Inject]
        IJoystickSettingsView m_View;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_View.onDoneClicked += OnDoneClicked;
        }

        void OnDoneClicked()
        {
            // Used on mobile only.
            m_SignalBus.Fire(new JoystickSettingsViewSignals.Close());
        }
    }
}
