using System;
using Unity.CompanionAppCommon;
using UnityEngine;
using UnityEngine.EventSystems;
#if !UNITY_EDITOR
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace Unity.CompanionApps.FaceCapture
{
    static class EventTriggerExtensions
    {
        public static void AddCallback(this EventTrigger eventTrigger, EventTriggerType type, Action<PointerEventData> callback)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(data => callback.Invoke((PointerEventData)data));
            eventTrigger.triggers.Add(entry);
        }
    }

    [RequireComponent(typeof(EventTrigger))]
    class BackgroundEventsView : DialogView, IBackgroundEventsView
    {
        public event Action<int, int> OnClick;
        public event Action<int> OnTouch;

        EventTrigger m_EventTrigger;

        void Awake()
        {
            m_EventTrigger = GetComponent<EventTrigger>();

            m_EventTrigger.AddCallback(EventTriggerType.PointerDown, OnPointerDown);

        }

        void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke((int)eventData.button, eventData.clickCount);

            var touchCount = 0;
#if !UNITY_EDITOR
            var touchScreen = Touchscreen.current;

            if (touchScreen != null)
            {
                foreach (var touch in touchScreen.touches)
                {
                    if (touch.ReadValue().phase == TouchPhase.Began)
                    {
                        ++touchCount;
                    }
                }
            }
#endif
            if (touchCount > 0)
            {
                OnTouch?.Invoke(touchCount);
            }
        }
    }
}
