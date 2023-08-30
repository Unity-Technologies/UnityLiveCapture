using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class DialogDoneView : DialogView
    {
        public event Action DoneClicked = delegate {};

        RectTransform m_ContentsContainer;
        RectTransform m_Footer;

        public RectTransform Contents => m_ContentsContainer;

        void Awake()
        {
            InitComponents();
        }

        void OnValidate()
        {
            InitComponents();
        }

        void InitComponents()
        {
            m_ContentsContainer = transform.Find("Contents").GetComponent<RectTransform>();
            m_Footer = transform.Find("Footer").GetComponent<RectTransform>();
            m_Footer.GetComponentInChildren<Button>().onClick.AddListener(() => DoneClicked.Invoke());
        }

        public void ShowFooter(bool value)
        {
            m_Footer.gameObject.SetActive(value);
        }
    }
}
