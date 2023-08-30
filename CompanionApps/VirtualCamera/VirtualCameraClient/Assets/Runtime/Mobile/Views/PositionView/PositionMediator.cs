using UnityEngine;
using Zenject;
using Unity.LiveCapture.VirtualCamera;

namespace Unity.CompanionApps.VirtualCamera
{
    class PositionMediator : IInitializable
    {
        [Inject]
        IPositionView m_PositionView;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_PositionView.onAxisLockChanged += OnAxisLockChanged;
            m_PositionView.onMotionScaleChanged += OnMotionScaleChanged;
        }

        void OnAxisLockChanged(PositionAxis axis)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.PositionLock,
                PositionAxisValue = axis
            });
        }

        void OnMotionScaleChanged(Vector3 motionScale)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.MotionScale,
                Vector3Value = motionScale
            });
        }
    }
}
