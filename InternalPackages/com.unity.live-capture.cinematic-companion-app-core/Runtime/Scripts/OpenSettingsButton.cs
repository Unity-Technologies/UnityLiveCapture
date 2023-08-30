using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionAppCommon
{
    [RequireComponent(typeof(Button))]
    public class OpenSettingsButton : MonoBehaviour
    {
        Button m_Button;

        private void OnEnable()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            m_Button.onClick.RemoveListener(OnButtonClicked);
        }

        void OnButtonClicked()
        {
            IOSHelper.OpenSettings();
        }
    }
}
