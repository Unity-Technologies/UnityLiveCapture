using System;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class JoystickGeneralTabMediator : IInitializable, IDisposable
    {
        [Inject]
        IJoystickGeneralTabView m_View;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_View.onPedestalSpaceChanged += OnPedestalSpaceChanged;
            m_View.onMotionSpaceChanged += OnMotionSpaceChanged;
            m_View.onSensitivityChanged += OnSensitivityChanged;
        }

        public void Dispose()
        {
            m_View.onPedestalSpaceChanged -= OnPedestalSpaceChanged;
            m_View.onMotionSpaceChanged -= OnMotionSpaceChanged;
            m_View.onSensitivityChanged -= OnSensitivityChanged;
        }

        void OnPedestalSpaceChanged(Space value)
        {
            m_SignalBus.Fire(new SendHostSignal
            {
                Type = HostMessageType.PedestalSpace,
                SpaceValue = value
            });
        }

        void OnMotionSpaceChanged(Space value)
        {
            m_SignalBus.Fire(new SendHostSignal
            {
                Type = HostMessageType.MotionSpace,
                SpaceValue = value
            });
        }

        void OnSensitivityChanged(Vector3 value)
        {
            m_SignalBus.Fire(new SendHostSignal
            {
                Type = HostMessageType.JoystickSensitivity,
                Vector3Value = value
            });
        }
    }
}
