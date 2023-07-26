using System;
using UnityEngine;

namespace Unity.TouchFramework
{
    public class ExpandableContainer : MonoBehaviour
    {
        [SerializeField]
        RectTransform m_ArrowIcon;

        [SerializeField]
        GameObject m_Content;

        [SerializeField]
        SimpleButton m_HeaderButton;

        [SerializeField]
        SimpleButton m_Button;

        public Action<ExpandableContainer> onExpand = delegate {};

        bool m_Expanded;

        public bool Expanded
        {
            get => m_Expanded;
            set
            {
                if (value != m_Expanded)
                {
                    SetExpanded(value);
                }
            }
        }

        void Awake()
        {
            SetExpanded(true);

            m_HeaderButton.onClick += ToggleExpanded;
            m_Button.onClick += ToggleExpanded;
        }

        void OnDestroy()
        {
            m_HeaderButton.onClick -= ToggleExpanded;
            m_Button.onClick -= ToggleExpanded;
        }

        [ContextMenu("Toggle Expanded")]
        void ToggleExpanded()
        {
            SetExpanded(!m_Expanded);
        }

        void SetExpanded(bool value)
        {
            m_Expanded = value;
            var zRotation = m_Expanded ? 180 : 0;
            m_ArrowIcon.localRotation = Quaternion.Euler(0, 0, zRotation);
            m_Content.SetActive(m_Expanded);
            onExpand.Invoke(this);
        }
    }
}
