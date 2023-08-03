using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class JoystickSettingsView : DialogView, IJoystickSettingsView
    {
        public event Action onDoneClicked = delegate {};

        [SerializeField]
        Button m_DoneButton;

        void OnEnable()
        {
            // Done button is optional.
            if (m_DoneButton != null)
            {
                m_DoneButton.onClick.AddListener(OnDoneClicked);
            }
        }

        void OnDisable()
        {
            if (m_DoneButton != null)
            {
                m_DoneButton.onClick.RemoveListener(OnDoneClicked);
            }
        }

        void OnDoneClicked() => onDoneClicked.Invoke();
    }
}
