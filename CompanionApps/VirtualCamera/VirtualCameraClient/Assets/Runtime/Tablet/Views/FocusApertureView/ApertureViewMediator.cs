using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ApertureViewMediator : IInitializable
    {
        [Inject]
        IApertureView m_ApertureView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_ApertureView.ApertureValueChanged += OnValueChanged;
        }

        void OnValueChanged(float value)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.Aperture,
                FloatValue = value
            });
        }
    }
}
