using System;
using UnityEngine;
using Unity.TouchFramework;

namespace Unity.CompanionApps.VirtualCamera
{
    public class JoysticksView : MonoBehaviour, IJoysticksView
    {
        public event Action<float> onForwardAxisChanged = delegate {};
        public event Action<float> onLateralAxisChanged = delegate {};
        public event Action<float> onVerticalAxisChanged = delegate {};

        [SerializeField, Tooltip("Reference to the left JoystickControl.")]
        JoystickControl m_LeftControl;

        [SerializeField, Tooltip("Reference to the right JoystickControl.")]
        JoystickControl m_RightControl;

        Vector2 m_PreviousHorizontalInput;
        Vector2 m_PreviousVerticalInput;

        bool m_GamepadIsValid = false;
        bool m_Show = true;

        public void SetGamepadIsValid(bool isValid)
        {
            m_GamepadIsValid = isValid;
            UpdateVisibility();
        }

        public void Show()
        {
            m_Show = true;
            UpdateVisibility();
        }

        public void Hide()
        {
            m_Show = false;
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            gameObject.SetActive(m_Show && !m_GamepadIsValid);
        }

        void Update()
        {
            if (m_PreviousHorizontalInput != m_LeftControl.inputAxis)
            {
                onForwardAxisChanged.Invoke(m_LeftControl.inputAxis.y);
                onLateralAxisChanged.Invoke(m_LeftControl.inputAxis.x);
            }

            if (m_PreviousVerticalInput != m_RightControl.inputAxis)
                onVerticalAxisChanged.Invoke(m_RightControl.inputAxis.y);

            m_PreviousHorizontalInput = m_LeftControl.inputAxis;
            m_PreviousVerticalInput = m_RightControl.inputAxis;
        }
    }
}
