using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    class TakeSelectionViewFullScreen : BaseTakeSelectionView, ITakeSelectionView
    {
        public event Action CancelClicked = delegate {};
        public event Action DoneClicked = delegate {};

        [SerializeField]
        Button m_CancelButton;

        [SerializeField]
        Button m_DoneButton;

        // Not implemented in this variant, no impact on logic
        public bool DoneButtonEnabled
        {
            set {}
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
