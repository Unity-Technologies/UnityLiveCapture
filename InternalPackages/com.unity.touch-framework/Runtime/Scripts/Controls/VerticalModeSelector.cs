using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Control that lets the user select a value from a list of options.
    /// </summary>
    public class VerticalModeSelector : MonoBehaviour
    {
        const float k_SegmentSwitchDragDistance = 110f;

        [Serializable]
        public class SegmentSelectEvent : UnityEvent<int> {}

        [Tooltip("The currently active index")]
        [SerializeField]
        int m_ActivePropertyIndex = 0;

#pragma warning disable CS0649
        [Tooltip("The selectable components representing each segment")]
        [SerializeField]
        List<Selectable> m_Segments = new List<Selectable>();
        [SerializeField]
        float m_SegmentSize = 100f;
        [SerializeField]
        RectTransform m_Container;
        [SerializeField]
        bool m_ChangeTextColorOfDisabledSegments;
        [SerializeField]
        bool m_Interactable = true;
#pragma warning restore CS0649
        bool m_CanClick;
        bool m_CanDragSelect;
        Selectable m_InteractingSegment;
        Vector2 m_DragStartPointerPosition;
        FloatTween m_FloatTween;
        TweenRunner<FloatTween> m_TweenRunner;
        Dictionary<Selectable, TextMeshProUGUI> m_SegmentTexts = new Dictionary<Selectable, TextMeshProUGUI>();
        SegmentSelectEvent m_OnValueChanged = new SegmentSelectEvent();
        float m_MoveStartYPos;

        public SegmentSelectEvent onValueChanged => m_OnValueChanged;

        /// <summary>
        /// The active property index
        /// </summary>
        public int activePropertyIndex
        {
            get => m_ActivePropertyIndex;
            set
            {
                if (m_ActivePropertyIndex == value)
                    return;

                m_ActivePropertyIndex = value;
                if (Application.isPlaying)
                {
                    m_MoveStartYPos = m_Container.anchoredPosition.y;
                    m_TweenRunner.StartTween(m_FloatTween, EaseType.EaseInCubic);
                }
                else
                {
                    SetContainerPosition(1f);
                }
                UpdateVisualState();
            }
        }

        public bool interactable
        {
            get => m_Interactable;
            set
            {
                m_Interactable = value;

                for (var i = 0; i < m_Segments.Count; i++)
                {
                    if (m_Interactable)
                    {
                        m_SegmentTexts[m_Segments[i]].color = UIConfig.propertyTextBaseColor;
                    }
                    else if (i != activePropertyIndex)
                    {
                        m_SegmentTexts[m_Segments[i]].color = UIConfig.propertyTextInactiveColor;
                    }
                }
            }
        }

        void UpdateVisualState()
        {
            if (m_ChangeTextColorOfDisabledSegments)
            {
                var i = 0;
                foreach (var segment in m_Segments)
                {
                    m_SegmentTexts[segment].color = i == m_ActivePropertyIndex ? UIConfig.propertyTextBaseColor : UIConfig.propertyTextInactiveColor;
                    i++;
                }
            }
        }

        void Awake()
        {
            RegisterTextToButtons();
            UpdateVisualState();
        }

        void RegisterTextToButtons()
        {
            foreach (var segment in m_Segments)
            {
                if (!m_SegmentTexts.ContainsKey(segment))
                {
                    var text = segment.GetComponentInChildren<TextMeshProUGUI>();
                    m_SegmentTexts.Add(segment, text);
                }
            }
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                foreach (var segment in m_Segments)
                {
                    EventTriggerUtility.CreateEventTrigger(segment.gameObject, OnControlPointerDown,
                        EventTriggerType.PointerDown);
                    EventTriggerUtility.CreateEventTrigger(segment.gameObject, OnControlPan,
                        EventTriggerType.Drag);
                    EventTriggerUtility.CreateEventTrigger(segment.gameObject, OnControlPointerUp,
                        EventTriggerType.PointerUp);
                }

                m_FloatTween = new FloatTween()
                {
                    duration = UIConfig.propertyColorTransitionTime,
                    ignoreTimeScale = true,
                    startValue = 0f,
                    targetValue = 1f
                };
                m_FloatTween.AddOnChangedCallback(SetContainerPosition);
                m_TweenRunner = new TweenRunner<FloatTween>();
                m_TweenRunner.Init(this);
            }
        }

        void SetContainerPosition(float value)
        {
            var targetYPos = activePropertyIndex * -1f * m_SegmentSize;
            var newYPos = Mathf.Lerp(m_MoveStartYPos, targetYPos, value);
            m_Container.anchoredPosition = new Vector2(m_Container.anchoredPosition.x, newYPos);
        }

        void OnControlPointerDown(BaseEventData eventData)
        {
            if (!interactable)
                return;

            var pointerPosition = ((PointerEventData)eventData).position;
            m_DragStartPointerPosition = pointerPosition;

            m_CanClick = true;
            m_CanDragSelect = true;
            m_InteractingSegment = eventData.selectedObject.GetComponent<Selectable>();
        }

        void OnControlPan(BaseEventData eventData)
        {
            if (!m_CanDragSelect)
                return;

            var pointerPosition = ((PointerEventData)eventData).position;
            var dragDistance = m_DragStartPointerPosition.y - pointerPosition.y;
            if (Mathf.Abs(dragDistance) > k_SegmentSwitchDragDistance)
            {
                var index = m_Segments.IndexOf(m_InteractingSegment);
                if (dragDistance > 0 && index < m_Segments.Count - 1)
                {
                    OnSegmentSelected(index + 1);
                    m_CanDragSelect = false;
                    m_CanClick = false;
                }
                else if (dragDistance < 0 && index > 0)
                {
                    OnSegmentSelected(index - 1);
                    m_CanDragSelect = false;
                    m_CanClick = false;
                }
            }
        }

        void OnControlPointerUp(BaseEventData eventData)
        {
            OnControlPan(eventData);

            m_CanDragSelect = false;

            if (m_CanClick)
            {
                var i = 0;
                foreach (var button in m_Segments)
                {
                    if (eventData.selectedObject == button.gameObject)
                        OnSegmentSelected(i);
                    i++;
                }
            }

            m_CanClick = false;
        }

        void OnSegmentSelected(int index)
        {
            if (activePropertyIndex == index)
                return;

            activePropertyIndex = index;

            if (onValueChanged != null)
                onValueChanged.Invoke(activePropertyIndex);
        }

        void Reset()
        {
            GetComponentsInChildren(m_Segments);
        }
    }
}
