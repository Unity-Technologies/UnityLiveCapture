using System;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadConfigurationTabMediator : IInitializable, IDisposable
    {
        [Inject]
        IGamepadConfigurationTabView m_View;
        [Inject]
        IGamepadLayoutView m_LayoutView;
        [Inject]
        Gamepad.IGamepadDriver m_GamepadDriver;
        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_GamepadDriver.OnHasDeviceChanged += OnHasDeviceChanged;
            m_View.onViewLayoutPressed += OnViewLayoutPressed;
            m_View.onPedestalSpaceChanged += OnPedestalSpaceChanged;
            m_View.onMotionSpaceChanged += OnMotionSpaceChanged;
        }

        public void Dispose()
        {
            m_GamepadDriver.OnHasDeviceChanged -= OnHasDeviceChanged;
            m_View.onViewLayoutPressed -= OnViewLayoutPressed;
            m_View.onPedestalSpaceChanged -= OnPedestalSpaceChanged;
            m_View.onMotionSpaceChanged -= OnMotionSpaceChanged;
        }

        void OnViewLayoutPressed()
        {
            m_LayoutView.Show();
        }

        void OnHasDeviceChanged(bool hasDevice)
        {
            var deviceName = hasDevice ? m_GamepadDriver.DeviceName : "Not Detected";
            m_View.SetDeviceName(deviceName);
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
    }
}
