using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    public class TakeDeleteView : BaseTakeDialog
    {
        public event Action canceled = delegate {};
        public event Action validate = delegate {};

        [SerializeField]
        Button m_CancelButton;
        [SerializeField]
        Button m_ValidateButton;
        [SerializeField]
        TextMeshProUGUI m_MessageTextField;

        public string takeName
        {
            set => m_MessageTextField.text = $"Are you sure you want to delete {value}?";
        }

        void OnEnable()
        {
            m_ValidateButton.onClick.AddListener(OnValidateClicked);
            m_CancelButton.onClick.AddListener(OnCancelClicked);
        }

        void OnDisable()
        {
            m_ValidateButton.onClick.RemoveListener(OnValidateClicked);
            m_CancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        void OnValidateClicked()
        {
            validate.Invoke();
        }

        void OnCancelClicked()
        {
            canceled.Invoke();
        }
    }
}
