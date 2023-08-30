using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.TouchFramework
{
    public class ListViewLinker : MonoBehaviour
    {
        static readonly string k_ErrorMessage = $"{nameof(ListViewLinker)} requires both {nameof(m_ActiveList)} and {nameof(m_PassiveList)} fields to be non null.";

        public event Action<bool> onLinkChanged = delegate {};

#pragma warning disable 649
        [SerializeField]
        ListView m_ActiveList;
        [SerializeField]
        ListView m_PassiveList;
#pragma warning restore 649

        bool m_IsLinked;

        public bool isLinked
        {
            get => m_IsLinked;
            set
            {
                m_IsLinked = value;
                onLinkChanged.Invoke(m_IsLinked);
            }
        }

        void OnLinkChanged(bool _)
        {
            if (m_IsLinked)
            {
                if (m_ActiveList != null)
                {
                    m_ActiveList.onScrollPositionChanged += MirrorPos;
                    m_PassiveList.SetSelectedIndex(m_ActiveList.GetSelectedIndex(), false);
                }
            }
            else
            {
                if (m_ActiveList != null)
                {
                    m_ActiveList.onScrollPositionChanged -= MirrorPos;
                    m_PassiveList.SetSelectedIndex(m_PassiveList.GetCurrentScrollPositionIndex(), true);
                }
            }

            UpdateVerticalScrolling();
        }

        void MirrorPos(float scrollPos)
        {
            m_PassiveList.SetScrollPosition(m_ActiveList.GetScrollPosition());
        }

        void Awake()
        {
            Assert.IsNotNull(m_ActiveList, k_ErrorMessage);
            Assert.IsNotNull(m_PassiveList, k_ErrorMessage);

            UpdateVerticalScrolling();

            m_ActiveList.selectedValueChanged += UpdateFromController;
            onLinkChanged += OnLinkChanged;
        }

        void OnDestroy()
        {
            onLinkChanged -= OnLinkChanged;
            m_ActiveList.selectedValueChanged -= UpdateFromController;
        }

        void UpdateVerticalScrolling()
        {
            m_PassiveList.enabled = !m_IsLinked;
        }

        /// <summary>
        /// The linked scroll list view relies on this callback to update its value, since it does not update on its own when linked
        /// </summary>
        /// <param name="controllerValue"></param>
        void UpdateFromController(float controllerValue)
        {
            if (isLinked)
            {
                m_PassiveList.SetSelectedIndex(m_ActiveList.GetSelectedIndex(), false);
            }
        }
    }
}
