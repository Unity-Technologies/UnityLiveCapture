using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    [RequireComponent(typeof(RoundedCornerBox))]
    public class ToggleButton : MonoBehaviour, IPointerUpHandler
    {
        // TODO: extract consts and share them.
        static Color k_LabelColor = Utilities.FromHexString("#E4E4E4");
        static Color k_BackgroundSelectedColor = Utilities.FromHexString("#474747");
        static Color k_BackgroundDeselectedColor = Utilities.FromHexString("#2E2E2E");
        static Color k_ShadowColor = new Color(0, 0, 0, 0.66f);

        public Action<bool> onChanged = delegate {};

        TextMeshProUGUI m_Label;
        RoundedCornerBox m_Background;
        bool m_Selected;

        public bool Selected
        {
            get => m_Selected;
            set
            {
                if (m_Selected != value)
                {
                    m_Selected = value;
                    UpdateGraphics();
                }
            }
        }

        void Awake()
        {
            m_Background = GetComponent<RoundedCornerBox>();
            m_Label = GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(m_Label);

            UpdateGraphics();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_Selected = !m_Selected;
            UpdateGraphics();
            onChanged.Invoke(m_Selected);
        }

        void UpdateGraphics()
        {
            m_Label.color = k_LabelColor;

            if (m_Selected)
            {
                m_Background.Color = k_BackgroundSelectedColor;
                m_Background.EnableShadow = true;
                m_Background.ShadowColor = k_ShadowColor;
                m_Background.ShadowRadius = 4;
            }
            else
            {
                m_Background.Color = k_BackgroundDeselectedColor;
                m_Background.EnableShadow = false;
            }
        }
    }
}
