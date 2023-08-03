using System;
using Unity.CompanionApps.VirtualCamera.Gamepad;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadPositionTabMediator : IInitializable, IDisposable
    {
        [Inject]
        IGamepadPositionTabView m_View;
        [Inject]
        GamepadSystem m_GamepadSystem;

        public void Initialize()
        {
            m_View.onSensitivityChanged += OnSensitivityChanged;

            var sensitivity = new Vector3
            (
                m_GamepadSystem.Sensitivities.GetSensitivity(AxisID.MoveLateral),
                m_GamepadSystem.Sensitivities.GetSensitivity(AxisID.MoveVertical),
                m_GamepadSystem.Sensitivities.GetSensitivity(AxisID.MoveForward)
            );
            m_View.SetSensitivity(sensitivity);
        }

        public void Dispose()
        {
            m_View.onSensitivityChanged -= OnSensitivityChanged;
        }

        void OnSensitivityChanged(Vector3 sensitivity)
        {
            m_GamepadSystem.Sensitivities.SetSensitivity(AxisID.MoveLateral, sensitivity.x);
            m_GamepadSystem.Sensitivities.SetSensitivity(AxisID.MoveVertical, sensitivity.y);
            m_GamepadSystem.Sensitivities.SetSensitivity(AxisID.MoveForward, sensitivity.z);

            m_GamepadSystem.Save();
        }
    }
}
