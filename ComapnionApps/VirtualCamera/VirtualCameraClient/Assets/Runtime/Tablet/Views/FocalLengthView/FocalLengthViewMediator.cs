using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocalLengthViewMediator : IInitializable
    {
        [Inject]
        IFocalLengthView m_FocalLengthView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_FocalLengthView.ValueChanged += OnValueChanged;
        }

        void OnValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocalLength,
                FloatValue = value
            });
        }
    }
}
