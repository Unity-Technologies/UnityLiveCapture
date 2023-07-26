using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionAppCommon
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    class HelpView : DialogView, IHelpView
    {
        public event Action DocumentationClicked = delegate {};
        public event Action SupportClicked = delegate {};

        [SerializeField]
        Button m_Documentation;

        [SerializeField]
        Button m_Support;

        VerticalLayoutGroup m_LayoutGroup;

        public TextAnchor Alignment
        {
            get => m_LayoutGroup.childAlignment;
            set => m_LayoutGroup.childAlignment = value;
        }

        void Awake()
        {
            m_LayoutGroup = GetComponent<VerticalLayoutGroup>();

            m_Documentation.onClick.AddListener(OnDocumentationClick);
            m_Support.onClick.AddListener(OnSupportClick);
        }

        void OnDestroy()
        {
            m_Documentation.onClick.RemoveListener(OnDocumentationClick);
            m_Support.onClick.RemoveListener(OnSupportClick);
        }

        void OnDocumentationClick()
        {
            DocumentationClicked.Invoke();
        }

        void OnSupportClick()
        {
            SupportClicked.Invoke();
        }
    }
}
