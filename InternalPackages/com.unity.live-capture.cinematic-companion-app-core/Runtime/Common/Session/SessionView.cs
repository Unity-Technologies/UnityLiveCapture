using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionAppCommon
{
    class SessionView : DialogView, ISessionView
    {
        public event Action DocumentationClicked = delegate {};

        [SerializeField]
        TextMeshProUGUI m_MessageTextField;

        [SerializeField]
        Button m_DocumentationButton;

        void OnEnable()
        {
            m_DocumentationButton.onClick.AddListener(OnDocumentationClicked);
        }

        void OnDisable()
        {
            m_DocumentationButton.onClick.RemoveListener(OnDocumentationClicked);
        }

        public void SetMessage(string value) => m_MessageTextField.text = value;

        void OnDocumentationClicked() => DocumentationClicked.Invoke();
    }
}
