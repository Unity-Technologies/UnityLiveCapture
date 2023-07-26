using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.CompanionAppCommon
{
    class OrphanTouch : MonoBehaviour, IOrphanTouch, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public event Action<Vector2> onPointerDown = delegate {};
        public event Action<Vector2> onBeginDrag = delegate {};
        public event Action<Vector2> onDrag = delegate {};
        public event Action<Vector2> onEndDrag = delegate {};

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag.Invoke(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag.Invoke(eventData.position);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown.Invoke(eventData.position);
        }
    }
}
