using System;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class WizardPermissionsView : WizardView, IWizardPermissionsView
    {
        public event Action OpenPermissionsSettings = delegate {};

        [SerializeField]
        Button m_OpenPermissionsSettingsButton;

        public override void Initialize()
        {
            base.Initialize();
            m_OpenPermissionsSettingsButton.onClick.AddListener(OnOpenPermissionsSettings);
        }

        public override void Dispose()
        {
            base.Dispose();
            m_OpenPermissionsSettingsButton.onClick.RemoveListener(OnOpenPermissionsSettings);
        }

        void OnOpenPermissionsSettings()
        {
            OpenPermissionsSettings.Invoke();
        }
    }
}
