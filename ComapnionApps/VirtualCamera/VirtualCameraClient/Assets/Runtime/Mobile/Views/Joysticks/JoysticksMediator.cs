using Zenject;


namespace Unity.CompanionApps.VirtualCamera
{
    public class JoysticksMediator : IInitializable
    {
        [Inject]
        IJoysticksView m_JoysticksView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_JoysticksView.onForwardAxisChanged += OnForwardAxisChanged;
            m_JoysticksView.onLateralAxisChanged += OnLateralAxisChanged;
            m_JoysticksView.onVerticalAxisChanged += OnVerticalAxisChanged;
        }

        void OnForwardAxisChanged(float axis)
        {
            m_SignalBus.Fire(new SetJoystickForwardSignal() { value = axis });
        }

        void OnLateralAxisChanged(float axis)
        {
            m_SignalBus.Fire(new SetJoystickLateralSignal() { value = axis });
        }

        void OnVerticalAxisChanged(float axis)
        {
            m_SignalBus.Fire(new SetJoystickVerticalSignal() { value = axis });
        }
    }
}
