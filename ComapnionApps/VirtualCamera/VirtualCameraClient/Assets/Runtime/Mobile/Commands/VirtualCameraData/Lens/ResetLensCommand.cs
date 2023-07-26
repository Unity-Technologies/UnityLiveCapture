using Unity.LiveCapture.VirtualCamera;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ResetLensCommand
    {
        [Inject] SignalBus m_SignalBus;

        public void Execute(ResetLensSignal signal)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocalLength,
                FloatValue = Lens.DefaultParams.FocalLength,
            });

            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocusDistance,
                FloatValue = Lens.DefaultParams.FocusDistance,
            });

            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.Aperture,
                FloatValue = Lens.DefaultParams.Aperture,
            });

            // TODO confirm we'll receive updated lens from server, no need to cache locally
        }
    }
}
