using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class ModalPopup : BasePopup
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_NegativeButton;
        [SerializeField]
        Button m_PositiveButton;
        [SerializeField]
        float m_DefaultBackgroundOpacity = 0.5f;
#pragma warning restore CS0649

        TextMeshProUGUI m_Title;
        TextMeshProUGUI m_Body;
        TextMeshProUGUI m_PositiveText;
        TextMeshProUGUI m_NegativeText;
        Image m_BackgroundImage;
        float m_FadeDuration;
        ModalPopupData m_Data;

        public struct ModalPopupData
        {
            public string title;
            public string text;
            public string positiveText;
            public string negativeText;
            public Action positiveCallback;
            public Action negativeCallback;
            public bool closePopupOnConfirm;
            public float fadeDuration;
            public float backgroundOpacity;
        }

        public ModalPopupData DefaultData()
        {
            return new ModalPopupData
            {
                title = "Alert",
                text = string.Empty,
                positiveText = "Done",
                negativeText = String.Empty,
                positiveCallback = delegate {},
                negativeCallback = delegate {},
                closePopupOnConfirm = true,
                fadeDuration = m_DefaultFadeDuration,
                backgroundOpacity = m_DefaultBackgroundOpacity,
            };
        }

        public event Action positiveAction;
        public event Action negativeAction;

        void Awake()
        {
            Initialize();
            var textFields = m_PopUpRect.GetComponentsInChildren<TextMeshProUGUI>();
            m_Title = textFields[0];
            m_Body = textFields[1];
            m_NegativeText = m_NegativeButton.GetComponentInChildren<TextMeshProUGUI>();
            m_PositiveText = m_PositiveButton.GetComponentInChildren<TextMeshProUGUI>();
            m_BackgroundImage = GetComponent<Image>();
            m_NegativeButton.onClick.AddListener(OnNegativeClick);
            m_PositiveButton.onClick.AddListener(OnPositiveClick);
        }

        public void Display(ModalPopupData data)
        {
            m_Data = data;

            m_Title.text = data.title;
            m_Body.text = data.text;
            m_PositiveText.text = data.positiveText;
            m_NegativeText.text = data.negativeText;
            m_FadeDuration = data.fadeDuration;
            positiveAction = data.positiveCallback;
            negativeAction = data.negativeCallback;
            var color = m_BackgroundImage.color;
            color.a = data.backgroundOpacity;
            m_BackgroundImage.color = color;

            m_NegativeButton.gameObject.SetActive(!string.IsNullOrEmpty(m_NegativeText.text));

            if (data.backgroundOpacity > 0f)
                m_BackgroundImage.enabled = true;

            StartAnimation(AnimationIn(data.fadeDuration));
        }

        void OnNegativeClick()
        {
            negativeAction();

            if (m_Data.closePopupOnConfirm)
            {
                Close();
            }
        }

        void OnPositiveClick()
        {
            positiveAction();

            if (m_Data.closePopupOnConfirm)
            {
                Close();
            }
        }

        public void Close()
        {
            m_BackgroundImage.enabled = false;
            StartAnimation(AnimationOut(m_FadeDuration));
        }
    }
}
