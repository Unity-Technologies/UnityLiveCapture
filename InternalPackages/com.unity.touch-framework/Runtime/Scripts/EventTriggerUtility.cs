using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    public static class EventTriggerUtility
    {
        /// <summary>
        /// Creates an <see cref="EventTrigger"/> or modifies and existing one on any <see cref="GameObject"/> capable
        /// of receiving events through the UI <see cref="EventSystem"/>. This is useful for receiving UI events on
        /// components that aren't on the UI component that you want to receive events on.
        /// </summary>
        /// <param name="go">The Game Object that will generate the events.</param>
        /// <param name="onEvent">The event delegate. The method passed in here will be called when the event
        /// type is triggered.</param>
        /// <param name="type">The type of event the trigger should receive.</param>
        public static void CreateEventTrigger(GameObject go, UnityAction<BaseEventData> onEvent, EventTriggerType type)
        {
            var eventTrigger = go.GetComponent<EventTrigger>();

            if (eventTrigger == null)
                eventTrigger = go.AddComponent<EventTrigger>();

            if (eventTrigger.triggers == null)
                eventTrigger.triggers = new List<EventTrigger.Entry>();

            var entry = new EventTrigger.Entry();
            var callback = new EventTrigger.TriggerEvent();
            var functionCall = new UnityAction<BaseEventData>(onEvent);
            callback.AddListener(functionCall);
            entry.eventID = type;
            entry.callback = callback;

            eventTrigger.triggers.Add(entry);
        }
    }
}
