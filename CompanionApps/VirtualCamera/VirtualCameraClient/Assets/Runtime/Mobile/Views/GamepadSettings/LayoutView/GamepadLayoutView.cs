using System;
using Unity.CompanionAppCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    class GamepadLayoutView : DialogView, IGamepadLayoutView
    {
        [SerializeField]
        Button m_CloseButton;

        public event Action onCloseClicked = delegate {};

        void OnEnable()
        {
            m_CloseButton.onClick.AddListener(OnCloseClicked);
        }

        void OnDisable()
        {
            m_CloseButton.onClick.RemoveListener(OnCloseClicked);
        }

        void OnCloseClicked()
        {
            onCloseClicked.Invoke();
        }
    }
}
