using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    public class ButtonToggle : MonoBehaviour
    {
        public Toggle.ToggleEvent onValueChanged = new Toggle.ToggleEvent();

        [SerializeField]
        RoundedRectButton m_OnButton;
        [SerializeField]
        RoundedRectButton m_OffButton;

        bool m_IsOn;

        public bool IsOn
        {
            get => m_IsOn;
            set
            {
                if (m_IsOn == value)
                {
                    return;
                }

                SetValueWithoutNotify(value);
                onValueChanged.Invoke(m_IsOn);
            }
        }

        void Awake() => SetValueWithoutNotify(m_IsOn);

        void OnValidate() => UpdateView();

        void OnEnable()
        {
            m_OnButton.onClick.AddListener(OnOnButtonClicked);
            m_OffButton.onClick.AddListener(OnOffButtonClicked);
        }

        void OnDisable()
        {
            m_OnButton.onClick.RemoveListener(OnOnButtonClicked);
            m_OffButton.onClick.RemoveListener(OnOffButtonClicked);
        }

        public void SetValueWithoutNotify(bool value)
        {
            m_IsOn = value;
            UpdateView();
        }

        void UpdateView()
        {
            m_OnButton.Pressed = IsOn;
            m_OffButton.Pressed = !IsOn;
        }

        void OnOnButtonClicked() => IsOn = true;

        void OnOffButtonClicked() => IsOn = false;
    }
}
