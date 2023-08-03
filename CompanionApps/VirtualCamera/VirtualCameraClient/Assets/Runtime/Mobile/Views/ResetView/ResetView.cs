using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class ResetView : DialogView, IResetView
    {
        public event Action onResetPoseClicked = delegate {};
        public event Action onResetLensClicked = delegate {};
        public event Action onRebaseToggled = delegate {};

        [SerializeField]
        Button m_ResetPoseButton;
        [SerializeField]
        Button m_ResetLensButton;
        [SerializeField]
        Button m_RebaseButton;
        [SerializeField]
        Image m_RebaseImage;
        [SerializeField]
        Sprite m_AREnabledIcon;
        [SerializeField]
        Sprite m_ARDisabledIcon;

        void OnEnable()
        {
            m_ResetPoseButton.onClick.AddListener(OnResetPoseClicked);
            m_ResetLensButton.onClick.AddListener(OnResetLensClicked);
            m_RebaseButton.onClick.AddListener(OnRebaseClicked);
        }

        void OnDisable()
        {
            m_ResetPoseButton.onClick.RemoveListener(OnResetPoseClicked);
            m_ResetLensButton.onClick.RemoveListener(OnResetLensClicked);
            m_RebaseButton.onClick.RemoveListener(OnRebaseClicked);
        }

        public void SetRebasing(bool value) => m_RebaseImage.sprite = value ? m_ARDisabledIcon : m_AREnabledIcon;

        void OnResetPoseClicked() => onResetPoseClicked.Invoke();

        void OnResetLensClicked() => onResetLensClicked.Invoke();

        void OnRebaseClicked() => onRebaseToggled.Invoke();
    }
}
