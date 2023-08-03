using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SetJoystickValueCommand
    {
        [Inject]
        ARSystem m_ARSystem;

        public void SetJoystickLateral(SetJoystickLateralSignal signal)
        {
            var value = m_ARSystem.JoysticksValue;
            value.x = signal.value;
            m_ARSystem.JoysticksValue = value;
        }

        public void SetJoystickVertical(SetJoystickVerticalSignal signal)
        {
            var value = m_ARSystem.JoysticksValue;
            value.y = signal.value;
            m_ARSystem.JoysticksValue = value;
        }

        public void SetJoystickForward(SetJoystickForwardSignal signal)
        {
            var value = m_ARSystem.JoysticksValue;
            value.z = signal.value;
            m_ARSystem.JoysticksValue = value;
        }

        public void SetGamepadLateral(SetGamepadLateralSignal signal)
        {
            var value = m_ARSystem.GamepadMoveValue;
            value.x = signal.value;
            m_ARSystem.GamepadMoveValue = value;
        }

        public void SetGamepadVertical(SetGamepadVerticalSignal signal)
        {
            var value = m_ARSystem.GamepadMoveValue;
            value.y = signal.value;
            m_ARSystem.GamepadMoveValue = value;
        }

        public void SetGamepadForward(SetGamepadForwardSignal signal)
        {
            var value = m_ARSystem.GamepadMoveValue;
            value.z = signal.value;
            m_ARSystem.GamepadMoveValue = value;
        }

        public void SetGamepadTilt(SetGamepadTiltSignal signal)
        {
            var value = m_ARSystem.GamepadLookValue;
            value.x = signal.value;
            m_ARSystem.GamepadLookValue = value;
        }

        public void SetGamepadPan(SetGamepadPanSignal signal)
        {
            var value = m_ARSystem.GamepadLookValue;
            value.y = signal.value;
            m_ARSystem.GamepadLookValue = value;
        }

        public void SetGamepadRoll(SetGamepadRollSignal signal)
        {
            var value = m_ARSystem.GamepadLookValue;
            value.z = signal.value;
            m_ARSystem.GamepadLookValue = value;
        }
    }
}
