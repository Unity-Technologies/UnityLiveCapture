using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    [RequireComponent(typeof(ToggleGroup))]
    class RigSettingsView : DialogView, IRigSettingsView
    {
        enum ViewType
        {
            Damping = 0,
            Position = 1,
            Rotation = 2
        }

        public event Action onRotationViewShowed = delegate {};
        public event Action onPositionViewShowed = delegate {};
        public event Action onDampingViewShowed = delegate {};
        public event Action onDoneClicked = delegate {};

        [SerializeField]
        Button m_DoneButton;
        [SerializeField]
        Toggle m_DampingToggle;
        [SerializeField]
        Toggle m_PositionToggle;
        [SerializeField]
        Toggle m_RotationToggle;
        [SerializeField]
        DampingView m_DampingView;
        [SerializeField]
        PositionView m_PositionView;
        [SerializeField]
        RotationLockView m_RotationLockView;

        GameObject[] m_Views;

        void Awake()
        {
            var toggleGroup = GetComponent<ToggleGroup>();
            m_DampingToggle.group = toggleGroup;
            m_PositionToggle.group = toggleGroup;
            m_RotationToggle.group = toggleGroup;

            m_Views = new[]
            {
                m_DampingView.gameObject,
                m_PositionView.gameObject,
                m_RotationLockView.gameObject
            };

            // Initial sync.
            m_DampingToggle.isOn = true;
            UpdateView(ViewType.Damping);
        }

        void OnEnable()
        {
            // Done button is optional.
            if (m_DoneButton != null)
            {
                m_DoneButton.onClick.AddListener(OnDoneClicked);
            }
            m_DampingToggle.onValueChanged.AddListener(OnDampingToggleChanged);
            m_PositionToggle.onValueChanged.AddListener(OnPositionToggleChanged);
            m_RotationToggle.onValueChanged.AddListener(OnRotationToggleChanged);
        }

        void OnDisable()
        {
            if (m_DoneButton != null)
            {
                m_DoneButton.onClick.RemoveListener(OnDoneClicked);
            }
            m_DampingToggle.onValueChanged.RemoveListener(OnDampingToggleChanged);
            m_PositionToggle.onValueChanged.RemoveListener(OnPositionToggleChanged);
            m_RotationToggle.onValueChanged.RemoveListener(OnRotationToggleChanged);
        }

        void UpdateView(ViewType viewType)
        {
            for (var i = 0; i != m_Views.Length; ++i)
            {
                m_Views[i].SetActive(i == (int)viewType);
            }
        }

        void OnDoneClicked() => onDoneClicked.Invoke();

        void OnDampingToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Damping);
                onDampingViewShowed.Invoke();
            }
        }

        void OnPositionToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Position);
                onPositionViewShowed.Invoke();
            }
        }

        void OnRotationToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Rotation);
                onRotationViewShowed.Invoke();
            }
        }
    }
}
