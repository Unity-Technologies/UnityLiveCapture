using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    public abstract class BaseToggle : MonoBehaviour, IPointerClickHandler
    {
        public Toggle.ToggleEvent onValueChanged = new Toggle.ToggleEvent();

        [SerializeField]
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

        protected virtual void Awake() => SetValueWithoutNotify(m_IsOn);

        protected virtual void OnValidate() => UpdateView();

        public void OnPointerClick(PointerEventData eventData) => IsOn = !m_IsOn;

        public void SetValueWithoutNotify(bool value)
        {
            m_IsOn = value;
            UpdateView();
        }

        protected abstract void UpdateView();
    }
}
