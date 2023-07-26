using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadSettingsView : DialogView, IGamepadSettingsView
    {
        enum ViewType
        {
            Configuration = 0,
            Position = 1,
            Rotation = 2,
            Sliders = 3
        }

        public event Action onDoneClicked = delegate {};

        [SerializeField]
        Button m_DoneButton;

        [SerializeField]
        Toggle m_ConfigurationToggle;
        [SerializeField]
        Toggle m_PositionToggle;
        [SerializeField]
        Toggle m_RotationToggle;
        [SerializeField]
        Toggle m_SlidersToggle;

        [SerializeField]
        GamepadConfigurationTabView m_ConfigurationTabView;
        [SerializeField]
        GamepadPositionTabView m_PositionTabView;
        [SerializeField]
        GamepadRotationTabView m_RotationTabView;
        [SerializeField]
        GamepadSlidersTabView m_SlidersTabView;

        bool m_InitializedViews;
        GameObject[] m_Views;

        void Awake()
        {
            var toggleGroup = GetComponent<ToggleGroup>();
            m_ConfigurationToggle.group = toggleGroup;
            m_PositionToggle.group = toggleGroup;
            m_RotationToggle.group = toggleGroup;
            m_SlidersToggle.group = toggleGroup;

            m_Views = new[]
            {
                m_ConfigurationTabView.gameObject,
                m_PositionTabView.gameObject,
                m_RotationTabView.gameObject,
                m_SlidersTabView.gameObject
            };
        }

        void OnEnable()
        {
            // Done button is optional.
            if (m_DoneButton != null)
            {
                m_DoneButton.onClick.AddListener(OnDoneClicked);
            }

            m_ConfigurationToggle.onValueChanged.AddListener(OnConfigurationToggleChanged);
            m_PositionToggle.onValueChanged.AddListener(OnPositionToggleChanged);
            m_RotationToggle.onValueChanged.AddListener(OnRotationToggleChanged);
            m_SlidersToggle.onValueChanged.AddListener(OnSlidersToggleChanged);
        }

        void OnDisable()
        {
            if (m_DoneButton != null)
            {
                m_DoneButton.onClick.RemoveListener(OnDoneClicked);
            }

            m_ConfigurationToggle.onValueChanged.RemoveListener(OnConfigurationToggleChanged);
            m_PositionToggle.onValueChanged.RemoveListener(OnPositionToggleChanged);
            m_RotationToggle.onValueChanged.RemoveListener(OnRotationToggleChanged);
            m_SlidersToggle.onValueChanged.RemoveListener(OnSlidersToggleChanged);
        }

        void Update()
        {
            // Initially, all tabs are active so that their Awake() and OnEnable() are called.
            // We manage their visibility on the first frame:
            if (!m_InitializedViews)
            {
                m_ConfigurationToggle.isOn = true;
                UpdateView(ViewType.Configuration);

                m_InitializedViews = true;
            }
        }

        void UpdateView(ViewType viewType)
        {
            for (var i = 0; i != m_Views.Length; ++i)
            {
                m_Views[i].SetActive(i == (int)viewType);
            }
        }

        void OnDoneClicked() => onDoneClicked.Invoke();

        void OnConfigurationToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Configuration);
            }
        }

        void OnPositionToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Position);
            }
        }

        void OnRotationToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Rotation);
            }
        }

        void OnSlidersToggleChanged(bool value)
        {
            if (value)
            {
                UpdateView(ViewType.Sliders);
            }
        }
    }
}
