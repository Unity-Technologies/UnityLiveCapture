using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class ConnectionScreenView : DialogView, IConnectionScreenView
    {
        public event Action<bool> OnShow = delegate {};
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

        DialogDoneView m_Dialog;


        public DialogDoneView DialogDone => m_Dialog;

        protected override void OnShowChanged()
        {
            OnShow.Invoke(IsShown);
        }

        void Awake()
        {
            m_Dialog = GetComponentInChildren<DialogDoneView>();

            m_SystemPermissions.onClick.AddListener(OnSystemPermissionsClick);
            m_Documentation.onClick.AddListener(OnDocumentationClick);
            m_Support.onClick.AddListener(OnSupportClick);
            m_RestartWizard.onClick.AddListener(OnRestartWizardClick);
        }

        void OnDestroy()
        {
            m_SystemPermissions.onClick.RemoveListener(OnSystemPermissionsClick);
            m_Documentation.onClick.RemoveListener(OnDocumentationClick);
            m_Support.onClick.RemoveListener(OnSupportClick);
            m_RestartWizard.onClick.RemoveListener(OnRestartWizardClick);
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

        void OnRestartWizardClick()
        {
            RestartWizardClicked.Invoke();
        }
    }
}
