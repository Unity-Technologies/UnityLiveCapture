using System;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class TakeSelectionView : BaseTakeSelectionView, ITakeSelectionView
    {
        public event Action CancelClicked = delegate {};
        public event Action DoneClicked = delegate {};

        [SerializeField]
        RoundedRectButton m_CancelButton;
        [SerializeField]
        RoundedRectButton m_DoneButton;

        public bool DoneButtonEnabled
        {
            set => m_DoneButton.enabled = value;
        }

        void OnEnable()
        {
            m_CancelButton.onClick.AddListener(OnCancelClicked);
            m_DoneButton.onClick.AddListener(OnDoneClicked);
        }

        void OnDisable()
        {
            m_CancelButton.onClick.RemoveListener(OnCancelClicked);
            m_DoneButton.onClick.RemoveListener(OnDoneClicked);
        }

        void OnCancelClicked() => CancelClicked.Invoke();

        void OnDoneClicked() => DoneClicked.Invoke();
    }
}
