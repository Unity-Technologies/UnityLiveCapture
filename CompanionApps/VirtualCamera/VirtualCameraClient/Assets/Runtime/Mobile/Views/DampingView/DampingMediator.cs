using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class DampingMediator : IInitializable
    {
        [Inject]
        IDampingView m_View;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_View.onBodyDampingChanged += OnBodyChanged;
            m_View.onAimChanged += OnAimChanged;
            m_View.onDampingEnabledChanged += OnEnabledChanged;
        }

        void OnBodyChanged(Vector3 body)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.BodyDamping,
                Vector3Value = body
            });
        }

        void OnAimChanged(float aim)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.AimDamping,
                FloatValue = aim
            });
        }

        void OnEnabledChanged(bool enabled)
        {
            m_SignalBus.Fire(new SendHostSignal()
            {
                Type = HostMessageType.DampingEnabled,
                BoolValue = enabled
            });
        }
    }
}
