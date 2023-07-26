using System;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadSlidersTabMediator : IInitializable, IDisposable
    {
        [Inject]
        IGamepadSlidersTabView m_View;
        [Inject]
        Gamepad.GamepadSystem m_GamepadSystem;

        public void Initialize()
        {
            m_View.onSensitivityChanged += OnSensitivityChanged;

            for (var i = 0; i < (int) Gamepad.AxisID.Count; i++)
            {
                var axis = (Gamepad.AxisID) i;
                var sensitivity = m_GamepadSystem.Sensitivities.GetSensitivity(axis);
                m_View.SetSensitivity(axis, sensitivity);
            }
        }

        public void Dispose()
        {
            m_View.onSensitivityChanged -= OnSensitivityChanged;
        }

        void OnSensitivityChanged(Gamepad.AxisID axis, float sensitivity)
        {
            m_GamepadSystem.Sensitivities.SetSensitivity(axis, sensitivity);
            m_GamepadSystem.Save();
        }
    }
}
