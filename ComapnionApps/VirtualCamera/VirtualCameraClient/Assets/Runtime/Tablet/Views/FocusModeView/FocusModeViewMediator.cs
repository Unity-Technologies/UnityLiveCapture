using Unity.LiveCapture.VirtualCamera;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class FocusModeViewMediator : IInitializable
    {
        [Inject]
        IFocusModeView m_FocusModeView;
        [Inject]
        SignalBus m_SignalBus;

        /*
        [Inject]
        IFocusReticleView m_FocusReticleView;
        */

        public void Initialize()
        {
            m_FocusModeView.FocusModeChanged += OnFocusModeChanged;
        }

        void OnFocusModeChanged(FocusMode focusMode)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.FocusMode,
                FocusModeValue = focusMode
            });

            /*
            // TODO centralize this logic in the reticle mediator
            if (focusMode != FocusMode.Clear && focusMode != FocusMode.Manual)
                m_SignalBus.Fire(new ShowViewSignal() { value = m_FocusReticleView });
            else
                m_SignalBus.Fire(new HideViewSignal() { value = m_FocusReticleView });
            */
        }
    }
}
