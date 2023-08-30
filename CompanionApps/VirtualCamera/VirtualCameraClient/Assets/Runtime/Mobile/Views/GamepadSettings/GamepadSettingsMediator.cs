using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadSettingsMediator : IInitializable
    {
        [Inject]
        IGamepadSettingsView m_View;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_View.onDoneClicked += OnDoneClicked;
        }

        void OnDoneClicked()
        {
            // Used on mobile only.
            m_SignalBus.Fire(new GamepadSettingsViewSignals.Close());
        }
    }
}
