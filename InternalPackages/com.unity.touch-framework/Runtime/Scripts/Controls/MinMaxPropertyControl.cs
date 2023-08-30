using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    [DisallowMultipleComponent]
    public class MinMaxPropertyControl : NumericInputFieldPropertyControl, IPointerDownHandler, IPointerUpHandler
    {
        const float k_SliderColorTransitionDuration = 0.2f;

#pragma warning disable 649
        [SerializeField]
        float m_MinFloat;
        [SerializeField]
        float m_MaxFloat = 10f;
        [SerializeField]
        float m_MinInt;
        [SerializeField]
        float m_MaxInt = 10;
        [SerializeField]
        Image m_Slider;
        [SerializeField]
        Color m_InactiveColor;
#pragma warning restore 649
        ColorTween m_SliderColorTween;
        TweenRunner<ColorTween> m_SliderColorTweenRunner;
        Vector2 m_StartPointerPosition;
        int m_StartDragInt;
        float m_StartDragFloat;
        bool m_Pressed;

        void Start()
        {
            m_SliderColorTween = new ColorTween()
            {
                duration = k_SliderColorTransitionDuration,
                ignoreTimeScale = true,
                tweenMode = ColorTween.ColorTweenMode.RGB
            };
            m_SliderColorTween.AddOnChangedCallback(SetSliderColor);
            m_SliderColorTweenRunner = new TweenRunner<ColorTween>();
            m_SliderColorTweenRunner.Init(this);
        }

        void SetSliderColor(Color color)
        {
            m_Slider.color = color;
        }

        protected override void DoBeginDrag(PointerEventData eventData)
        {
            m_StartPointerPosition = eventData.LocalPosition(m_ContainerRect);

            if (m_NumberType == NumberType.Float)
            {
                if (!float.TryParse(text, out var num))
                    num = 0f;

                m_StartDragFloat = num;
            }
            else
            {
                if (!int.TryParse(text, out var intNum))
                    intNum = 0;

                m_StartDragInt = intNum;
            }
        }

        protected override void DoDrag(PointerEventData eventData)
        {
            float delta;
            var pointerPos = eventData.LocalPosition(m_ContainerRect);
            var dragDistance = (pointerPos - m_StartPointerPosition).x;
            var dragFactor = dragDistance / m_ContainerRect.rect.width;
            if (m_NumberType == NumberType.Float)
            {
                delta = (m_MaxFloat - m_MinFloat) * dragFactor;
                var newFloat = ProcessNumber(m_StartDragFloat + delta);
                text = newFloat.ToString(m_FloatFieldFormatString);

                onFloatValueChanged.Invoke(newFloat);
            }
            else
            {
                delta = (m_MaxInt - m_MinInt) * dragFactor;
                var newInt = ProcessNumber(Mathf.RoundToInt(m_StartDragInt + delta));
                text = newInt.ToString(k_IntFieldFormatString);

                onIntValueChanged.Invoke(newInt);
            }
        }

        protected override float ProcessNumber(float num)
        {
            var clampedNumber = Mathf.Clamp(num, m_MinFloat, m_MaxFloat);
            UpdateSlider(clampedNumber);
            return clampedNumber;
        }

        protected override int ProcessNumber(int num)
        {
            var clampedNumber = (int)Mathf.Clamp(num, m_MinInt, m_MaxInt);
            UpdateSlider(clampedNumber);
            return clampedNumber;
        }

        void UpdateSlider(float num)
        {
            var fillAmount = Mathf.InverseLerp(m_MinFloat, m_MaxFloat, num);
            m_Slider.fillAmount = fillAmount;
        }

        void UpdateSlider(int num)
        {
            var fillAmount = Mathf.InverseLerp(m_MinInt, m_MaxInt, num);
            m_Slider.fillAmount = fillAmount;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            // If the slider is close to zero users may not be able to see that it can be dragged, in that case, set
            // it to where they tap the slider
            if (Math.Abs(m_Slider.fillAmount) < 0.01f)
            {
                var pointerPos = eventData.LocalPosition(m_ContainerRect);
                var rect = m_ContainerRect.rect;
                var factor = (pointerPos.x + rect.width / 2f) / rect.width;
                if (m_NumberType == NumberType.Float)
                {
                    var value = Mathf.Lerp(m_MinFloat, m_MaxFloat, factor);
                    var newFloat = ProcessNumber(value);
                    text = newFloat.ToString(m_FloatFieldFormatString);

                    onFloatValueChanged.Invoke(newFloat);
                }
                else
                {
                    var value = Mathf.Lerp(m_MinInt, m_MaxInt, factor);
                    var newInt = ProcessNumber(Mathf.RoundToInt(value));
                    text = newInt.ToString(k_IntFieldFormatString);

                    onIntValueChanged.Invoke(newInt);
                }
            }

            m_SliderColorTween.startColor = m_Slider.color;
            m_SliderColorTween.targetColor = UIConfig.propertySelectedColor;
            m_SliderColorTweenRunner.StartTween(m_SliderColorTween, EaseType.EaseOutCubic);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            m_SliderColorTween.startColor = m_Slider.color;
            m_SliderColorTween.targetColor = m_InactiveColor;
            m_SliderColorTweenRunner.StartTween(m_SliderColorTween, EaseType.EaseOutCubic);
        }
    }
}
