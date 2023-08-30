using Zenject;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    class RotationLockMediator : IInitializable
    {
        [Inject]
        IRotationLockView m_RotationLockView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_RotationLockView.onRotationLockChanged += OnRotationLockChanged;
            m_RotationLockView.onZeroDutchChanged += OnZeroDutchChanged;
        }

        void OnZeroDutchChanged(bool on)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.AutoHorizon,
                BoolValue = on
            });
        }

        void OnRotationLockChanged(RotationAxis rotationAxis)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.RotationLock,
                RotationAxisValue = rotationAxis
            });
        }
    }
}
