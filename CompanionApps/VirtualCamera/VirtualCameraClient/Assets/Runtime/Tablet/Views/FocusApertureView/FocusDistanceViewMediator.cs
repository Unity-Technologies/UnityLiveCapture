using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusDistanceViewMediator : IInitializable
    {
        [Inject]
        IFocusDistanceView m_FocusDistanceView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_FocusDistanceView.FocusDistanceValueChanged += OnValueChanged;
            m_FocusDistanceView.FocusOffsetValueChanged += OnOffsetValueChanged;
        }

        void OnValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocusDistance,
                FloatValue = value
            });
        }

        void OnOffsetValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocusDistanceOffset,
                FloatValue = value
            });
        }
    }
}
