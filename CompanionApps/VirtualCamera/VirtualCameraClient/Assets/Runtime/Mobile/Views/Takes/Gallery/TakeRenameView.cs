using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    public class TakeRenameView : BaseTakeDialog
    {
        public event Action cancelClicked = delegate {};
        public event Action renameClicked = delegate {};

        [SerializeField]
        TMP_InputField m_SceneNumberInput;
        [SerializeField]
        TMP_InputField m_ShotNameInput;
        [SerializeField]
        TMP_InputField m_TakeNumberInput;
        [SerializeField]
        TMP_InputField m_DescriptionInput;
        [SerializeField]
        Button m_CancelButton;
        [SerializeField]
        Button m_RenameButton;

        // TODO restrict to positive integers?
        public int SceneNumber
        {
            get => FromString(m_SceneNumberInput.text);
            set => m_SceneNumberInput.text = value.ToString();
        }

        public string ShotName
        {
            get => m_ShotNameInput.text;
            set => m_ShotNameInput.text = value;
        }

        // TODO restrict to positive integers?
        public int TakeNumber
        {
            get => FromString(m_TakeNumberInput.text);
            set => m_TakeNumberInput.text = value.ToString();
        }

        public string Description
        {
            get => m_DescriptionInput.text;
            set => m_DescriptionInput.text = value;
        }

        void OnEnable()
        {
            m_ShotNameInput.characterLimit = 3;
            m_TakeNumberInput.characterLimit = 3;
            m_ShotNameInput.characterLimit = 32;
            m_DescriptionInput.characterLimit = 280;

            m_CancelButton.onClick.AddListener(OnCancelClicked);
            m_RenameButton.onClick.AddListener(OnRenameClicked);
        }

        void OnDisable()
        {
            m_CancelButton.onClick.RemoveListener(OnCancelClicked);
            m_RenameButton.onClick.RemoveListener(OnRenameClicked);
        }

        void OnCancelClicked()
        {
            cancelClicked.Invoke();
        }

        void OnRenameClicked()
        {
            renameClicked.Invoke();
        }

        // TODO could be promoted.
        static int FromString(string str)
        {
            if (int.TryParse(str, out var value))
            {
                return value;
            }

            return 0;
        }
    }
}
