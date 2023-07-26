using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.FaceCapture
{
    class LaunchScreenView : DialogView, ILaunchScreenView
    {
        public event Action SystemPermissionsClicked = delegate {};
        public event Action DocumentationClicked = delegate {};
        public event Action SupportClicked = delegate {};
        public event Action RestartWizardClicked = delegate {};

        [SerializeField]
        Button m_SystemPermissions;

        [SerializeField]
        Button m_Documentation;

        [SerializeField]
        Button m_Support;

        [SerializeField]
        Button m_RestartWizard;

        void Awake()
        {
            m_SystemPermissions.onClick.AddListener(OnSystemPermissionsClick);
            m_Documentation.onClick.AddListener(OnDocumentationClick);
            m_Support.onClick.AddListener(OnSupportClick);
            m_RestartWizard.onClick.AddListener(OnRestartWizard);
        }

        void OnDestroy()
        {
            m_SystemPermissions.onClick.RemoveListener(OnSystemPermissionsClick);
            m_Documentation.onClick.RemoveListener(OnDocumentationClick);
            m_Support.onClick.RemoveListener(OnSupportClick);
            m_RestartWizard.onClick.RemoveListener(OnRestartWizard);
        }

        void OnSystemPermissionsClick()
        {
            SystemPermissionsClicked.Invoke();
        }

        void OnDocumentationClick()
        {
            DocumentationClicked.Invoke();
        }

        void OnSupportClick()
        {
            SupportClicked.Invoke();
        }

        void OnRestartWizard()
        {
            RestartWizardClicked.Invoke();
        }
    }
}
