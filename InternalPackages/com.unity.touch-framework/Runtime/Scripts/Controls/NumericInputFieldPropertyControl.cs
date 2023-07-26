using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    public class NumericInputFieldPropertyControl : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public enum NumberType
        {
            Float,
            Int,
        }

        protected const string k_IntFieldFormatString = "#######0";
        const float k_FloatDragSensitivity = 1f;
        const float k_IntDragSensitivity = 2f;
        const float k_MaxFloatDelta = 0.1f;

        [Serializable]
        public class IntChangedEvent : UnityEvent<int> {}

        [Serializable]
        public class FloatChangedEvent : UnityEvent<float> {}

#pragma warning disable 649
        [SerializeField]
        protected NumberType m_NumberType = NumberType.Float;
        [SerializeField]
        protected string m_FloatFieldFormatString = "F1";
        [SerializeField]
        TextMeshProUGUI m_Text;
        [SerializeField]
        protected RectTransform m_ContainerRect;
#pragma warning restore 649

        Vector2 m_LastPointerPosition;
        IntChangedEvent m_OnIntValueChanged = new IntChangedEvent();
        FloatChangedEvent m_OnFloatValueChanged = new FloatChangedEvent();

        protected string text
        {
            get => m_Text.text;
            set => m_Text.text = value;
        }

        public IntChangedEvent onIntValueChanged => m_OnIntValueChanged;
        public FloatChangedEvent onFloatValueChanged => m_OnFloatValueChanged;

        public void SetValue(float value)
        {
            value = ProcessNumber(value);
            text = value.ToString(m_FloatFieldFormatString);
        }

        public void SetValue(int value)
        {
            value = ProcessNumber(value);
            text = value.ToString(k_IntFieldFormatString);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            DoBeginDrag(eventData);
        }

        protected virtual void DoBeginDrag(PointerEventData eventData)
        {
            m_LastPointerPosition = eventData.LocalPosition(m_ContainerRect);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            DoDrag(eventData);
        }

        protected virtual void DoDrag(PointerEventData eventData)
        {
            var pointerPos = eventData.LocalPosition(m_ContainerRect);
            var delta = pointerPos - m_LastPointerPosition;

            if (m_NumberType == NumberType.Float)
            {
                if (!float.TryParse(text, out var num))
                    num = 0f;

                var deltaX = Mathf.Clamp(delta.x, -k_MaxFloatDelta, k_MaxFloatDelta);
                var dragSensitivity = CalculateDragSensitivity(num) * k_FloatDragSensitivity;
                num += deltaX * dragSensitivity;
                num = ProcessNumber(num);

                text = num.ToString(m_FloatFieldFormatString);
                m_LastPointerPosition = pointerPos;

                m_OnFloatValueChanged.Invoke(num);
            }
            else
            {
                if (!int.TryParse(text, out var intNum))
                    intNum = 0;

                var dragSensitivity = CalculateDragSensitivity(intNum) * k_IntDragSensitivity;
                var change = (int)(delta.x * dragSensitivity);

                intNum += change;
                intNum = ProcessNumber(intNum);

                text = intNum.ToString(k_IntFieldFormatString);
                if (change != 0)
                    m_LastPointerPosition = pointerPos;

                m_OnIntValueChanged.Invoke(intNum);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            DoEndDrag(eventData);
        }

        protected virtual void DoEndDrag(PointerEventData eventData)
        {
            var pointerPos = eventData.position;
            m_LastPointerPosition = pointerPos;
        }

        protected virtual float ProcessNumber(float number)
        {
            return number;
        }

        protected virtual int ProcessNumber(int number)
        {
            return number;
        }

        static float CalculateDragSensitivity(float value)
        {
            if (float.IsInfinity(value) || float.IsNaN(value))
                return 0f;

            return Mathf.Max(1, Mathf.Pow(Mathf.Abs(value), 0.5f));
        }
    }
}
