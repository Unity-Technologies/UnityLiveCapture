using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class DialControlMarkedEntry : MonoBehaviour
    {
        [Serializable]
        public class EntryTappedEvent : UnityEvent<float> {}

        float m_Angle;
        float m_Radius;
        float m_FontSize;
        TextMeshProUGUI m_Text;
        Button m_Button;
        Image m_ButtonImage;
        RectTransform m_Rect;
        EntryTappedEvent m_EntryTappedEvent = new EntryTappedEvent();

        public EntryTappedEvent onEntryTapped => m_EntryTappedEvent;

        public float radius
        {
            set
            {
                if (Math.Abs(value - m_Radius) < Mathf.Epsilon)
                    return;

                m_Radius = value;
                var pos = m_Rect.anchoredPosition;
                pos.x = -m_Radius;
                m_Rect.anchoredPosition = pos;
            }
        }

        public string text
        {
            get => m_Text.text;
            set => m_Text.text = value;
        }

        public float angle
        {
            get => m_Angle;
            set => m_Angle = value;
        }

        public float fontSize
        {
            set
            {
                if (Math.Abs(value - m_FontSize) < Mathf.Epsilon)
                    return;

                m_FontSize = value;
                m_Text.fontSize = m_FontSize;
            }
        }

        public bool interactable
        {
            set
            {
                m_Button.interactable = value;
                m_ButtonImage.raycastTarget = value;
            }
        }

        void Awake()
        {
            m_Text = GetComponentInChildren<TextMeshProUGUI>();
            m_Button = GetComponentInChildren<Button>();
            m_ButtonImage = m_Button.GetComponent<Image>();
            m_Rect = m_Button.GetComponent<RectTransform>();
            m_Button.onClick.AddListener(OnButtonClick);
        }

        void OnButtonClick()
        {
            onEntryTapped.Invoke(angle);
        }

        void OnDestroy()
        {
            onEntryTapped.RemoveAllListeners();
        }
    }
}
