using System;
using Unity.CompanionApps.VirtualCamera.Gamepad;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadRotationTabMediator : IInitializable, IDisposable
    {
        [Inject]
        IGamepadRotationTabView m_View;
        [Inject]
        GamepadSystem m_GamepadSystem;

        public void Initialize()
        {
            m_View.onSensitivityChanged += OnSensitivityChanged;

            var sensitivity = new Vector3
            (
                m_GamepadSystem.Sensitivities.GetSensitivity(AxisID.LookTilt),
                m_GamepadSystem.Sensitivities.GetSensitivity(AxisID.LookPan),
                m_GamepadSystem.Sensitivities.GetSensitivity(AxisID.LookRoll)
            );
            m_View.SetSensitivity(sensitivity);
        }

        public void Dispose()
        {
            m_View.onSensitivityChanged -= OnSensitivityChanged;
        }

        void OnSensitivityChanged(Vector3 sensitivity)
        {
            m_GamepadSystem.Sensitivities.SetSensitivity(AxisID.LookTilt, sensitivity.x);
            m_GamepadSystem.Sensitivities.SetSensitivity(AxisID.LookPan, sensitivity.y);
            m_GamepadSystem.Sensitivities.SetSensitivity(AxisID.LookRoll, sensitivity.z);

            m_GamepadSystem.Save();
        }
    }
}
