using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RoundedCornerBox))]
    [RequireComponent(typeof(CanvasGroup))]
    public class RoundedRectButton : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick = new UnityEvent();

        [SerializeField]
        TextMeshProUGUI m_Label;
        [SerializeField]
        RoundedCornerBox m_Background;
        [SerializeField]
        RoundedRectButtonStyle m_Style;
        [SerializeField]
        CanvasGroup m_CanvasGroup;

        bool m_Pressed;

        public bool Pressed
        {
            get => m_Pressed;
            set
            {
                m_Pressed = value;
                UpdateView();
            }
        }

        public void OnPointerClick(PointerEventData eventData) => onClick.Invoke();

        void OnValidate()
        {
            if (m_Label == null)
            {
                m_Label = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (m_Background == null)
            {
                m_Background = GetComponent<RoundedCornerBox>();
            }

            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }

            TryAssignStyle();
            UpdateView();
        }

        void Awake() => UpdateView();

        void OnEnable() => UpdateView(true);

        void OnDisable() => UpdateView(false);

        void TryAssignStyle()
        {
            if (m_Style == null)
            {
                m_Style = FindObjectOfType<RoundedRectButtonStyle>();
            }
        }

        void UpdateView() => UpdateView(enabled);

        void UpdateView(bool @enabled)
        {
            if (m_Style != null)
            {
                ApplyViewState(m_Pressed ? m_Style.Pressed : m_Style.Normal);
            }

            m_CanvasGroup.alpha = @enabled ? 1 : .5f;
            m_CanvasGroup.interactable = @enabled;
        }

        void ApplyViewState(RoundedRectButtonStyle.Style state)
        {
            m_Label.color = state.LabelColor;
            m_Background.Color = state.BackgroundColor;
            m_Background.Border = state.Border;
            m_Background.BorderColor = state.BackgroundBorderColor;
            m_Background.EnableShadow = state.EnableShadow;
            m_Background.ShadowColor = state.ShadowColor;
            m_Background.ShadowRadius = state.ShadowRadius;
        }
    }
}
