using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Unity.CompanionApps.VirtualCamera
{
    class Slider2 : Slider
    {
        public event Action onPointerDown = delegate {};
        public event Action onPointerUp = delegate {};

        [SerializeField]
        bool m_IgnoreMoveInput = true;

        public bool IgnoreAxisMove
        {
            get => m_IgnoreMoveInput;
            set => m_IgnoreMoveInput = value;
        }

        float m_RequestedValue;
        bool m_IsValueRequested;

        public override void SetValueWithoutNotify(float input)
        {
            if (IsPressed())
            {
                m_RequestedValue = input;
                m_IsValueRequested = true;
            }
            else
            {
                base.Set(input, false);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            onPointerDown.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (m_IsValueRequested)
            {
                m_IsValueRequested = false;

                base.Set(m_RequestedValue, false);
            }

            onPointerUp.Invoke();
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!m_IgnoreMoveInput)
            {
                base.OnMove(eventData);
            }
        }
    }
}
