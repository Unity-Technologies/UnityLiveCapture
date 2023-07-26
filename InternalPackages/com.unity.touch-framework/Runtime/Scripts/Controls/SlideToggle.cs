using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    /// <summary>
    /// A slide toggle typically found in iOS native apps.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SlideToggle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        const float k_ToggleDragDistance = 20f;

        [Serializable]
        public class ToggleValueChanged : UnityEvent<bool> {}

        [SerializeField]
        bool m_IsOn;

#pragma warning disable CS0649
        [SerializeField]
        RectTransform m_HandleRectTransform;
        [SerializeField]
        Color m_BackgroundOffColor = UIConfig.propertyTextInactiveColor;
        [SerializeField]
        Color m_BackgroundOnColor = UIConfig.propertyTextSelectedColor;
#pragma warning restore CS0649

        bool m_CanClick;
        bool m_CanDrag;
        float m_OnXPos;
        float m_OffXPos;
        Image m_BackgroundImage;
        Vector2 m_DragStartPointerPosition;
        ColorTween m_ColorTween;
        TweenRunner<ColorTween> m_ColorTweenRunner;
        FloatTween m_MoveHandleTween;
        TweenRunner<FloatTween> m_MoveHandleTweenRunner;

        ToggleValueChanged m_OnValueChanged = new ToggleValueChanged();
        bool m_Initialized;

        public ToggleValueChanged onValueChanged => m_OnValueChanged;

        /// <summary>
        /// Gets and sets the toggle state.
        /// </summary>
        public bool on
        {
            get => m_IsOn;
            set
            {
                if (m_IsOn == value)
                    return;

                m_IsOn = value;

                if (m_Initialized)
                {
                    UpdateVisualState(gameObject.activeInHierarchy);
                }
            }
        }

        void UpdateVisualState(bool animate)
        {
            var targetPosition = on ? m_OnXPos : m_OffXPos;
            var targetColor = on ? m_BackgroundOnColor : m_BackgroundOffColor;

            m_ColorTweenRunner.StopTween();
            m_MoveHandleTweenRunner.StopTween();

            if (animate)
            {
                m_ColorTween.startColor = m_BackgroundImage.color;
                m_ColorTween.targetColor = targetColor;
                m_ColorTweenRunner.StartTween(m_ColorTween, EaseType.EaseInCubic);

                m_MoveHandleTween.startValue = m_HandleRectTransform.anchoredPosition.x;
                m_MoveHandleTween.targetValue = targetPosition;
                m_MoveHandleTweenRunner.StartTween(m_MoveHandleTween, EaseType.EaseInCubic);
            }
            else
            {
                m_HandleRectTransform.anchoredPosition = new Vector2(targetPosition, 0);
                m_BackgroundImage.color = targetColor;
            }
        }

        void Awake()
        {
            m_Initialized = true;
            m_BackgroundImage = GetComponent<Image>();

            m_OnXPos = m_BackgroundImage.rectTransform.sizeDelta.x / 4f;
            m_OffXPos = -m_OnXPos;

            m_ColorTween = new ColorTween()
            {
                duration = UIConfig.propertyColorTransitionTime,
                ignoreTimeScale = true,
                tweenMode = ColorTween.ColorTweenMode.RGB
            };
            m_ColorTween.AddOnChangedCallback(UpdateImageColor);
            m_ColorTweenRunner = new TweenRunner<ColorTween>();
            m_ColorTweenRunner.Init(this);

            m_MoveHandleTween = new FloatTween()
            {
                duration = UIConfig.propertyColorTransitionTime,
                ignoreTimeScale = true
            };
            m_MoveHandleTween.AddOnChangedCallback(UpdateHandlePosition);
            m_MoveHandleTweenRunner = new TweenRunner<FloatTween>();
            m_MoveHandleTweenRunner.Init(this);
        }

        void OnEnable()
        {
            UpdateVisualState(false);
        }

        void OnDisable()
        {
            m_ColorTweenRunner.StopTween();
            m_MoveHandleTweenRunner.StopTween();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_DragStartPointerPosition = eventData.position;
            m_CanClick = true;
            m_CanDrag = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_CanDrag)
                return;

            var dragXDistance = eventData.position.x - m_DragStartPointerPosition.x;

            if (Mathf.Abs(dragXDistance) > k_ToggleDragDistance)
            {
                m_CanClick = false;

                if (dragXDistance > 0 && !on || dragXDistance < 0 && on)
                {
                    m_CanDrag = false;
                    on = !on;
                    onValueChanged?.Invoke(on);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_CanClick)
            {
                on = !on;
                onValueChanged?.Invoke(on);
            }

            m_CanDrag = false;
            m_CanClick = false;
        }

        void UpdateHandlePosition(float position)
        {
            m_HandleRectTransform.anchoredPosition = new Vector2(position, 0);
        }

        void UpdateImageColor(Color color)
        {
            m_BackgroundImage.color = color;
        }
    }
}
