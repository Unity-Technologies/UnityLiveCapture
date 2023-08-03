using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    [RequireComponent(typeof(CanvasGroup))]
    class TakeIterationToggle : Toggle
    {
        [SerializeField]
        Sprite m_UnlockedSprite;
        [SerializeField]
        Sprite m_LockedSprite;
        [SerializeField]
        Color m_UnlockedColor;
        [SerializeField]
        Color m_LockedColor;
        [SerializeField]
        Image m_Icon;
        [SerializeField]
        Image m_Outline;
        [SerializeField]
        bool m_IsLocked;

        CanvasGroup m_CanvasGroup;

        public bool IsLocked
        {
            get => m_IsLocked;
            set
            {
                if (m_IsLocked == value)
                {
                    return;
                }

                m_IsLocked = value;
                UpdateLockedView();
            }
        }

        // TODO Is it better to override the base "interactable" property?
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                interactable = value;

                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = GetComponent<CanvasGroup>();
                }

                m_CanvasGroup.interactable = value;
                m_CanvasGroup.alpha = value ? 1 : .25f;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateLockedView();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateLockedView();
        }

#endif

        void UpdateLockedView()
        {
            m_Icon.sprite = m_IsLocked ? m_LockedSprite : m_UnlockedSprite;

            var color = m_IsLocked ? m_LockedColor : m_UnlockedColor;
            m_Icon.color = color;
            m_Outline.color = color;
            m_Outline.enabled = m_IsLocked;
        }
    }
}
